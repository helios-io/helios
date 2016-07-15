// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Net.Sockets;
using System.Threading;
using Helios.Buffers;

namespace Helios.Channels.Sockets
{
    public abstract class AbstractSocketByteChannel : AbstractSocketChannel
    {
        private static readonly string ExpectedTypes =
            string.Format(" (expected: {0})", typeof(IByteBuf).Name); //+ ", " +

        private static readonly Action<object> FlushAction = _ => ((AbstractSocketByteChannel) _).Flush();
        private static readonly Action<object, object> ReadCompletedSyncCallback = OnReadCompletedSync;

        protected AbstractSocketByteChannel(IChannel parent, Socket socket) : base(parent, socket)
        {
        }

        protected override IChannelUnsafe NewUnsafe()
        {
            return new SocketByteChannelUnsafe(this);
        }

        protected override void ScheduleSocketRead()
        {
            var operation = ReadOperation;
            var pending = Socket.ReceiveAsync(operation);
            if (!pending)
            {
                // todo: potential allocation / non-static field?
                EventLoop.Execute(ReadCompletedSyncCallback, Unsafe, operation);
            }
        }

        private static void OnReadCompletedSync(object u, object e)
        {
            ((ISocketChannelUnsafe) u).FinishRead((SocketChannelAsyncOperation) e);
        }

        protected override void DoWrite(ChannelOutboundBuffer input)
        {
            var writeSpinCount = -1;

            while (true)
            {
                var msg = input.Current;
                if (msg == null)
                {
                    // Wrote all messages.
                    break;
                }

                if (msg is IByteBuf)
                {
                    var buf = (IByteBuf) msg;
                    var readableBytes = buf.ReadableBytes;
                    if (readableBytes == 0)
                    {
                        input.Remove();
                        continue;
                    }

                    var scheduleAsync = false;
                    var done = false;
                    long flushedAmount = 0;
                    if (writeSpinCount == -1)
                    {
                        writeSpinCount = Configuration.WriteSpinCount;
                    }
                    for (var i = writeSpinCount - 1; i >= 0; i--)
                    {
                        var localFlushedAmount = DoWriteBytes(buf);
                        if (localFlushedAmount == 0)
                            // todo: check for "sent less than attempted bytes" to avoid unnecessary extra doWriteBytes call?
                        {
                            scheduleAsync = true;
                            break;
                        }

                        flushedAmount += localFlushedAmount;
                        if (!buf.IsReadable())
                        {
                            done = true;
                            break;
                        }
                    }

                    //input.Progress(flushedAmount); // todo: support progress reports on ChannelOutboundBuffer

                    if (done)
                    {
                        input.Remove();
                    }
                    else
                    {
                        IncompleteWrite(scheduleAsync, PrepareWriteOperation(buf.GetIoBuffer()));
                        break;
                    }
                }
                else
                {
                    // Should not reach here.
                    throw new InvalidOperationException();
                }
            }
        }

        protected override object FilterOutboundMessage(object msg)
        {
            if (msg is IByteBuf)
            {
                return msg;
                //IByteBuffer buf = (IByteBuffer) msg;
                //if (buf.isDirect()) {
                //    return msg;
                //}

                //return newDirectBuffer(buf);
            }

            // todo: FileRegion support
            //if (msg is FileRegion) {
            //    return msg;
            //}

            throw new NotSupportedException(
                "unsupported message type: " + msg.GetType().Name + ExpectedTypes);
        }

        protected void IncompleteWrite(bool scheduleAsync, SocketChannelAsyncOperation operation)
        {
            // Did not write completely.
            if (scheduleAsync)
            {
                this.SetState(StateFlags.WriteScheduled);
                bool pending;

                if (ExecutionContext.IsFlowSuppressed())
                {
                    pending = this.Socket.SendAsync(operation);
                }
                else
                {
                    using (ExecutionContext.SuppressFlow())
                    {
                        pending = this.Socket.SendAsync(operation);
                    }
                }

                if (!pending)
                {
                    ((ISocketChannelUnsafe) this.Unsafe).FinishWrite(operation);
                }
            }
            else
            {
                // Schedule flush again later so other tasks can be picked up input the meantime
                this.EventLoop.Execute(FlushAction, this);
            }
        }

        /// <summary>
        ///     Read bytes into the given {@link ByteBuf} and return the amount.
        /// </summary>
        protected abstract int DoReadBytes(IByteBuf buf);

        /// <summary>
        ///     Write bytes form the given {@link ByteBuf} to the underlying {@link java.nio.channels.Channel}.
        ///     @param buf           the {@link ByteBuf} from which the bytes should be written
        ///     @return amount       the amount of written bytes
        /// </summary>
        protected abstract int DoWriteBytes(IByteBuf buf);

        protected class SocketByteChannelUnsafe : AbstractSocketUnsafe
        {
            public SocketByteChannelUnsafe(AbstractSocketByteChannel channel)
                : base(channel)
            {
            }

            private new AbstractSocketByteChannel Channel
            {
                get { return (AbstractSocketByteChannel) _channel; }
            }

            private void CloseOnRead()
            {
                Channel.ShutdownInput();
                if (_channel.IsOpen)
                {
                    // todo: support half-closure
                    //if (bool.TrueString.Equals(this.channel.Configuration.getOption(ChannelOption.ALLOW_HALF_CLOSURE))) {
                    //    key.interestOps(key.interestOps() & ~readInterestOp);
                    //    this.channel.Pipeline.FireUserEventTriggered(ChannelInputShutdownEvent.INSTANCE);
                    //} else {
                    CloseAsync();
                    //}
                }
            }

            private void HandleReadException(IChannelPipeline pipeline, IByteBuf byteBuf, Exception cause, bool close)
            {
                if (byteBuf != null)
                {
                    if (byteBuf.IsReadable())
                    {
                        Channel.ReadPending = false;
                        pipeline.FireChannelRead(byteBuf);
                    }
                    else
                    {
                        byteBuf.Release();
                    }
                }
                pipeline.FireChannelReadComplete();
                pipeline.FireExceptionCaught(cause);
                if (close || cause is SocketException)
                {
                    CloseOnRead();
                }
            }

            public override void FinishRead(SocketChannelAsyncOperation operation)
            {
                var ch = Channel;
                ch.ResetState(StateFlags.ReadScheduled);
                var config = ch.Configuration;
                var pipeline = ch.Pipeline;
                var allocator = config.Allocator;
                var maxMessagesPerRead = config.MaxMessagesPerRead;
                var allocHandle = RecvBufAllocHandle;

                IByteBuf byteBuf = null;
                var messages = 0;
                var close = false;
                try
                {
                    operation.Validate();

                    var totalReadAmount = 0;
                    var readPendingReset = false;
                    do
                    {
                        byteBuf = allocHandle.Allocate(allocator);
                        var writable = byteBuf.WritableBytes;
                        var localReadAmount = ch.DoReadBytes(byteBuf);
                        if (localReadAmount <= 0)
                        {
                            // not was read release the buffer
                            byteBuf.Release();
                            byteBuf = null;
                            close = localReadAmount < 0;
                            break;
                        }
                        if (!readPendingReset)
                        {
                            readPendingReset = true;
                            ch.ReadPending = false;
                        }
                        pipeline.FireChannelRead(byteBuf);
                        byteBuf = null;

                        if (totalReadAmount >= int.MaxValue - localReadAmount)
                        {
                            // Avoid overflow.
                            totalReadAmount = int.MaxValue;
                            break;
                        }

                        totalReadAmount += localReadAmount;

                        // stop reading
                        if (!config.AutoRead)
                        {
                            break;
                        }

                        if (localReadAmount < writable)
                        {
                            // Read less than what the buffer can hold,
                            // which might mean we drained the recv buffer completely.
                            break;
                        }
                    } while (++messages < maxMessagesPerRead);

                    pipeline.FireChannelReadComplete();
                    allocHandle.Record(totalReadAmount);

                    if (close)
                    {
                        CloseOnRead();
                        close = false;
                    }
                }
                catch (Exception t)
                {
                    HandleReadException(pipeline, byteBuf, t, close);
                }
                finally
                {
                    // Check if there is a readPending which was not processed yet.
                    // This could be for two reasons:
                    // /// The user called Channel.read() or ChannelHandlerContext.read() input channelRead(...) method
                    // /// The user called Channel.read() or ChannelHandlerContext.read() input channelReadComplete(...) method
                    //
                    // See https://github.com/netty/netty/issues/2254
                    if (!close && (ch.ReadPending || config.AutoRead))
                    {
                        ch.DoBeginRead();
                    }
                }
            }
        }
    }
}