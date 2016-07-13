// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Helios.Buffers;
using Helios.Concurrency;

namespace Helios.Channels.Sockets
{
    public class TcpSocketChannel : AbstractSocketByteChannel
    {
        private readonly ISocketChannelConfiguration _config;

        /// <summary>
        ///     Create a new instance
        /// </summary>
        public TcpSocketChannel()
            : this(new Socket(SocketType.Stream, ProtocolType.Tcp))
        {
        }

        /// <summary>
        ///     Create a new instance
        /// </summary>
        public TcpSocketChannel(AddressFamily addressFamily)
            : this(new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp))
        {
        }

        /// <summary>
        ///     Create a new instance using the given {@link SocketChannel}.
        /// </summary>
        public TcpSocketChannel(Socket socket)
            : this(null, socket)
        {
        }

        /// <summary>
        ///     Create a new instance
        ///     @param parent    the {@link Channel} which created this instance or {@code null} if it was created by the user
        ///     @param socket    the {@link SocketChannel} which will be used
        /// </summary>
        public TcpSocketChannel(IChannel parent, Socket socket)
            : this(parent, socket, false)
        {
        }

        internal TcpSocketChannel(IChannel parent, Socket socket, bool connected)
            : base(parent, socket)
        {
            _config = new TcpSocketChannelConfig(this, socket);
            if (connected)
            {
                OnConnected();
            }
        }

        public override bool DisconnectSupported
        {
            get { return false; }
        }

        protected override EndPoint LocalAddressInternal
        {
            get { return Socket.LocalEndPoint; }
        }

        protected override EndPoint RemoteAddressInternal
        {
            get { return Socket.RemoteEndPoint; }
        }

        public bool IsOutputShutdown
        {
            get { throw new NotImplementedException(); } // todo: impl with stateflags
        }

        public override IChannelConfiguration Configuration
        {
            get { return _config; }
        }

        protected override IChannelUnsafe NewUnsafe()
        {
            return new TcpSocketChannelUnsafe(this);
        }

        public Task ShutdownOutputAsync()
        {
            var tcs = new TaskCompletionSource();
            var loop = EventLoop;
            if (loop.InEventLoop)
            {
                ShutdownOutput0(tcs);
            }
            else
            {
                loop.Execute(promise => ShutdownOutput0((TaskCompletionSource) promise), tcs);
            }
            return tcs.Task;
        }

        private void ShutdownOutput0(TaskCompletionSource promise)
        {
            try
            {
                Socket.Shutdown(SocketShutdown.Send);
                promise.Complete();
            }
            catch (Exception ex)
            {
                promise.SetException(ex);
            }
        }

        protected override bool DoConnect(EndPoint remoteAddress, EndPoint localAddress)
        {
            if (localAddress != null)
            {
                Socket.Bind(localAddress);
            }

            var success = false;
            try
            {
                var eventPayload = new SocketChannelAsyncOperation(this, false);
                eventPayload.RemoteEndPoint = remoteAddress;
                var connected = !Socket.ConnectAsync(eventPayload);
                success = true;
                return connected;
            }
            finally
            {
                if (!success)
                {
                    DoClose();
                }
            }
        }

        protected override void DoFinishConnect(SocketChannelAsyncOperation operation)
        {
            try
            {
                operation.Validate();
            }
            finally
            {
                operation.Dispose();
            }
            OnConnected();
        }

        private void OnConnected()
        {
            SetState(StateFlags.Active);

            // preserve local and remote addresses for later availability even if Socket fails
            CacheLocalAddress();
            CacheRemoteAddress();
        }

        protected override void DoBind(EndPoint localAddress)
        {
            Socket.Bind(localAddress);
        }

        protected override void DoDisconnect()
        {
            DoClose();
        }

        protected override void DoClose()
        {
            base.DoClose();
            if (ResetState(StateFlags.Open | StateFlags.Active))
            {
                Socket.Shutdown(SocketShutdown.Both);
                Socket.Close(0);
            }
        }

        protected override int DoReadBytes(IByteBuf buf)
        {
            if (!buf.HasArray)
            {
                throw new NotImplementedException("Only IByteBuffer implementations backed by array are supported.");
            }

            SocketError errorCode;
            int received = 0;
            try
            {
                received = Socket.Receive(buf.Array, buf.ArrayOffset + buf.WriterIndex, buf.WritableBytes,
                    SocketFlags.None, out errorCode);
            }
            catch (ObjectDisposedException)
            {
                errorCode = SocketError.Shutdown;
            }

            switch (errorCode)
            {
                case SocketError.Success:
                    if (received == 0)
                    {
                        return -1; // indicate that socket was closed
                    }
                    break;
                case SocketError.WouldBlock:
                    if (received == 0)
                    {
                        return 0;
                    }
                    break;
                case SocketError.Shutdown:
                    return -1; // socket was closed
                default:
                    throw new SocketException((int) errorCode);
            }

            buf.SetWriterIndex(buf.WriterIndex + received);

            return received;
        }

        protected override int DoWriteBytes(IByteBuf buf)
        {
            if (!buf.HasArray)
            {
                throw new NotImplementedException("Only IByteBuffer implementations backed by array are supported.");
            }

            SocketError errorCode;
            var sent = Socket.Send(buf.Array, buf.ArrayOffset + buf.ReaderIndex, buf.ReadableBytes, SocketFlags.None,
                out errorCode);

            if (errorCode != SocketError.Success && errorCode != SocketError.WouldBlock)
            {
                throw new SocketException((int) errorCode);
            }

            if (sent > 0)
            {
                buf.SetReaderIndex(buf.ReaderIndex + sent);
            }

            return sent;
        }

        protected override void DoWrite(ChannelOutboundBuffer input)
        {
            while (true)
            {
                int size = input.Count;
                if (size == 0)
                {
                    // All written
                    break;
                }
                long writtenBytes = 0;
                bool done = false;
                bool setOpWrite = false;

                // Ensure the pending writes are made of ByteBufs only.
                List<ArraySegment<byte>> nioBuffers = input.GetNioBuffers();
                int nioBufferCnt = nioBuffers.Count;
                long expectedWrittenBytes = input.NioBufferSize;
                Socket socket = this.Socket;

                // Always us nioBuffers() to workaround data-corruption.
                // See https://github.com/netty/netty/issues/2761
                switch (nioBufferCnt)
                {
                    case 0:
                        // We have something else beside ByteBuffers to write so fallback to normal writes.
                        base.DoWrite(input);
                        return;
                    case 1:
                        // Only one ByteBuf so use non-gathering write
                        ArraySegment<byte> nioBuffer = nioBuffers[0];
                        for (int i = this.Configuration.WriteSpinCount - 1; i >= 0; i--)
                        {
                            SocketError errorCode;
                            int localWrittenBytes = socket.Send(nioBuffer.Array, nioBuffer.Offset, nioBuffer.Count, SocketFlags.None, out errorCode);
                            if (errorCode != SocketError.Success && errorCode != SocketError.WouldBlock)
                            {
                                throw new SocketException((int)errorCode);
                            }

                            if (localWrittenBytes == 0)
                            {
                                setOpWrite = true;
                                break;
                            }
                            expectedWrittenBytes -= localWrittenBytes;
                            writtenBytes += localWrittenBytes;
                            if (expectedWrittenBytes == 0)
                            {
                                done = true;
                                break;
                            }
                        }
                        break;
                    default:
                        for (int i = this.Configuration.WriteSpinCount - 1; i >= 0; i--)
                        {
                            SocketError errorCode;
                            long localWrittenBytes = socket.Send(nioBuffers, SocketFlags.None, out errorCode);
                            if (errorCode != SocketError.Success && errorCode != SocketError.WouldBlock)
                            {
                                throw new SocketException((int)errorCode);
                            }

                            if (localWrittenBytes == 0)
                            {
                                setOpWrite = true;
                                break;
                            }
                            expectedWrittenBytes -= localWrittenBytes;
                            writtenBytes += localWrittenBytes;
                            if (expectedWrittenBytes == 0)
                            {
                                done = true;
                                break;
                            }
                        }
                        break;
                }

                if (!done)
                {
                    SocketChannelAsyncOperation asyncOperation = this.PrepareWriteOperation(nioBuffers);

                    // Release the fully written buffers, and update the indexes of the partially written buffer.
                    input.RemoveBytes(writtenBytes);

                    // Did not write all buffers completely.
                    this.IncompleteWrite(setOpWrite, asyncOperation);
                    break;
                }

                // Release the fully written buffers, and update the indexes of the partially written buffer.
                input.RemoveBytes(writtenBytes);
            }
        }

        private sealed class TcpSocketChannelUnsafe : SocketByteChannelUnsafe
        {
            public TcpSocketChannelUnsafe(TcpSocketChannel channel)
                : base(channel)
            {
            }
        }

        private sealed class TcpSocketChannelConfig : DefaultSocketChannelConfiguration
        {
            public TcpSocketChannelConfig(TcpSocketChannel channel, Socket javaSocket)
                : base(channel, javaSocket)
            {
            }

            protected override void AutoReadCleared() => ((TcpSocketChannel)this.Channel).ClearReadPending();
        }
    }
}

