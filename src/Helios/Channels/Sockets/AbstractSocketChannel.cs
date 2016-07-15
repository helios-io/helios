// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Helios.Buffers;
using Helios.Concurrency;
using Helios.Logging;
using Helios.Util.Concurrency;

namespace Helios.Channels.Sockets
{
    public abstract class AbstractSocketChannel : AbstractChannel, ISocketChannel
    {
        private static readonly ILogger Logger = LoggingFactory.GetLogger<AbstractSocketChannel>();

        internal static readonly EventHandler<SocketAsyncEventArgs> IoCompletedCallback = OnIoCompleted;

        private static readonly Action<object, object> ConnectCallbackAction =
            (u, e) => ((ISocketChannelUnsafe) u).FinishConnect((SocketChannelAsyncOperation) e);

        private static readonly Action<object, object> ReadCallbackAction =
            (u, e) => ((ISocketChannelUnsafe) u).FinishRead((SocketChannelAsyncOperation) e);

        private static readonly Action<object, object> WriteCallbackAction =
            (u, e) => ((ISocketChannelUnsafe) u).FinishWrite((SocketChannelAsyncOperation) e);

        protected readonly Socket Socket;
        private SocketChannelAsyncOperation _readOperation;
        private SocketChannelAsyncOperation _writeOperation;
        private IScheduledTask _connectCancellationTask;

        private TaskCompletionSource _connectPromise;
        private volatile bool _inputShutdown;
        internal bool _readPending;
        private volatile StateFlags _state;

        protected AbstractSocketChannel(IChannel parent, Socket socket)
            : base(parent)
        {
            Socket = socket;
            _state = StateFlags.Open;

            try
            {
                Socket.Blocking = false;
            }
            catch (SocketException ex)
            {
                try
                {
                    socket.Close();
                }
                catch (SocketException ex2)
                {
                    if (Logger.IsWarningEnabled)
                    {
                        Logger.Warning("Failed to close a partially initialized socket.", ex2);
                    }
                }

                throw new ChannelException("Failed to enter non-blocking mode.", ex);
            }
        }

        /// <summary>
        ///     Set read pending to <c>false</c>.
        /// </summary>
        protected internal void ClearReadPending()
        {
            if (this.Registered)
            {
                IEventLoop eventLoop = this.EventLoop;
                if (eventLoop.InEventLoop)
                {
                    this.ClearReadPending0();
                }
                else
                {
                    eventLoop.Execute(channel => ((AbstractSocketChannel) channel).ClearReadPending0(), this);
                }
            }
            else
            {
                // Best effort if we are not registered yet clear ReadPending. This happens during channel initialization.
                // NB: We only set the boolean field instead of calling ClearReadPending0(), because the SelectionKey is
                // not set yet so it would produce an assertion failure.
                this.ReadPending = false;
            }
        }

        void ClearReadPending0() => this.ReadPending = false;

        protected bool ReadPending
        {
            get { return _readPending; }
            set { _readPending = value; }
        }

        protected bool InputShutdown
        {
            get { return _inputShutdown; }
        }

        protected SocketChannelAsyncOperation ReadOperation
        {
            get { return _readOperation ?? (_readOperation = new SocketChannelAsyncOperation(this, true)); }
        }

        SocketChannelAsyncOperation WriteOperation
            => _writeOperation ?? (_writeOperation = new SocketChannelAsyncOperation(this, false));

        public override bool IsOpen
        {
            get { return IsInState(StateFlags.Open); }
        }

        public override bool IsActive
        {
            get { return IsInState(StateFlags.Active); }
        }

        public new IServerSocketChannel Parent => base.Parent as IServerSocketChannel;
        ISocketChannelConfiguration ISocketChannel.Configuration => Configuration as ISocketChannelConfiguration;

        protected void ShutdownInput()
        {
            _inputShutdown = true;
        }

        protected void SetState(StateFlags stateToSet)
        {
            _state |= stateToSet;
        }

        protected bool ResetState(StateFlags stateToReset)
        {
            var oldState = _state;
            if ((oldState & stateToReset) != 0)
            {
                _state = oldState & ~stateToReset;
                return true;
            }
            return false;
        }

        protected bool IsInState(StateFlags stateToCheck)
        {
            return (_state & stateToCheck) == stateToCheck;
        }

        protected SocketChannelAsyncOperation PrepareWriteOperation(ArraySegment<byte> buffer)
        {
            SocketChannelAsyncOperation operation = WriteOperation;
            operation.SetBuffer(buffer.Array, buffer.Offset, buffer.Count);
            return operation;
        }

        protected SocketChannelAsyncOperation PrepareWriteOperation(IList<ArraySegment<byte>> buffers)
        {
            SocketChannelAsyncOperation operation = WriteOperation;
            operation.BufferList = buffers;
            return operation;
        }

        protected void ResetWriteOperation()
        {
            var operation = _writeOperation;
            Contract.Requires(operation != null);
            if (operation.BufferList == null)
            {
                operation.SetBuffer(null, 0, 0);
            }
            else
            {
                operation.BufferList = null;
            }
        }

        /// <remarks>PORT NOTE: matches behavior of NioEventLoop.processSelectedKey</remarks>
        private static void OnIoCompleted(object sender, SocketAsyncEventArgs args)
        {
            var operation = (SocketChannelAsyncOperation) args;
            var channel = operation.Channel;
            var @unsafe = (ISocketChannelUnsafe) channel.Unsafe;
            var eventLoop = channel.EventLoop;
            switch (args.LastOperation)
            {
                case SocketAsyncOperation.Accept:
                    if (eventLoop.InEventLoop)
                    {
                        @unsafe.FinishRead(operation);
                    }
                    else
                    {
                        eventLoop.Execute(ReadCallbackAction, @unsafe, operation);
                    }
                    break;
                case SocketAsyncOperation.Connect:
                    if (eventLoop.InEventLoop)
                    {
                        @unsafe.FinishConnect(operation);
                    }
                    else
                    {
                        eventLoop.Execute(ConnectCallbackAction, @unsafe, operation);
                    }
                    break;
                case SocketAsyncOperation.Receive:
                    if (eventLoop.InEventLoop)
                    {
                        @unsafe.FinishRead(operation);
                    }
                    else
                    {
                        eventLoop.Execute(ReadCallbackAction, @unsafe, operation);
                    }
                    break;
                case SocketAsyncOperation.Send:
                    if (eventLoop.InEventLoop)
                    {
                        @unsafe.FinishWrite(operation);
                    }
                    else
                    {
                        eventLoop.Execute(WriteCallbackAction, @unsafe, operation);
                    }
                    break;
                default:
                    // todo: think of a better way to comm exception
                    throw new ArgumentException("The last operation completed on the socket was not expected");
            }
        }

        protected override bool IsCompatible(IEventLoop eventLoop)
        {
            return true;
        }

        protected override void DoBeginRead()
        {
            if (_inputShutdown)
            {
                return;
            }

            if (!IsOpen)
            {
                return;
            }

            _readPending = true;

            if (!IsInState(StateFlags.ReadScheduled))
            {
                _state |= StateFlags.ReadScheduled;
                ScheduleSocketRead();
            }
        }

        protected abstract void ScheduleSocketRead();

        /// <summary>
        ///     Connect to the remote peer
        /// </summary>
        protected abstract bool DoConnect(EndPoint remoteAddress, EndPoint localAddress);

        /// <summary>
        ///     Finish the connect
        /// </summary>
        protected abstract void DoFinishConnect(SocketChannelAsyncOperation operation);

        protected override void DoClose()
        {
            var promise = _connectPromise;
            if (promise != null)
            {
                // Use TrySetException() instead of SetException() to avoid the race against cancellation due to timeout.
                promise.TrySetException(ClosedChannelException.Instance);
                _connectPromise = null;
            }

            var cancellationTask = _connectCancellationTask;
            if (cancellationTask != null)
            {
                cancellationTask.Cancel();
                _connectCancellationTask = null;
            }

            var readOp = _readOperation;
            if (readOp != null)
            {
                readOp.Dispose();
                _readOperation = null;
            }

            var writeOp = _writeOperation;
            if (writeOp != null)
            {
                writeOp.Dispose();
                _writeOperation = null;
            }
        }

        [Flags]
        protected enum StateFlags
        {
            Open = 1,
            ReadScheduled = 1 << 1,
            WriteScheduled = 1 << 2,
            Active = 1 << 3
            // todo: add input shutdown and read pending here as well?
        }

        internal interface ISocketChannelUnsafe : IChannelUnsafe
        {
            /// <summary>
            ///     Finish connect
            /// </summary>
            void FinishConnect(SocketChannelAsyncOperation operation);

            /// <summary>
            ///     Read from underlying {@link SelectableChannel}
            /// </summary>
            void FinishRead(SocketChannelAsyncOperation operation);

            void FinishWrite(SocketChannelAsyncOperation operation);
        }

        protected abstract class AbstractSocketUnsafe : AbstractUnsafe, ISocketChannelUnsafe
        {
            protected AbstractSocketUnsafe(AbstractSocketChannel channel)
                : base(channel)
            {
            }

            public AbstractSocketChannel Channel
            {
                get { return (AbstractSocketChannel) _channel; }
            }

            public sealed override Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress)
            {
                // todo: handle cancellation
                var ch = Channel;
                if (!ch.IsOpen)
                {
                    return CreateClosedChannelExceptionTask();
                }

                try
                {
                    if (ch._connectPromise != null)
                    {
                        throw new InvalidOperationException("connection attempt already made");
                    }

                    var wasActive = _channel.IsActive;
                    if (ch.DoConnect(remoteAddress, localAddress))
                    {
                        FulfillConnectPromise(wasActive);
                        return TaskEx.Completed;
                    }
                    ch._connectPromise = new TaskCompletionSource(remoteAddress);

                    // Schedule connect timeout.
                    var connectTimeout = ch.Configuration.ConnectTimeout;
                    if (connectTimeout > TimeSpan.Zero)
                    {
                        ch._connectCancellationTask = ch.EventLoop.Schedule(
                            (c, a) =>
                            {
                                // todo: make static / cache delegate?..
                                var self = (AbstractSocketChannel) c;
                                // todo: call Socket.CancelConnectAsync(...)
                                var promise = self._connectPromise;
                                var cause = new ConnectTimeoutException("connection timed out: " + a.ToString());
                                if (promise != null && promise.TrySetException(cause))
                                {
                                    self.CloseAsync();
                                }
                            },
                            _channel,
                            remoteAddress,
                            connectTimeout);
                    }

                    ch._connectPromise.Task.ContinueWith(
                        (t, s) =>
                        {
                            var c = (AbstractSocketChannel) s;
                            if (c._connectCancellationTask != null)
                            {
                                c._connectCancellationTask.Cancel();
                            }
                            c._connectPromise = null;
                            c.CloseAsync();
                        },
                        ch,
                        TaskContinuationOptions.OnlyOnCanceled | TaskContinuationOptions.ExecuteSynchronously);

                    return ch._connectPromise.Task;
                }
                catch (Exception ex)
                {
                    CloseIfClosed();
                    return TaskEx.FromException(AnnotateConnectException(ex, remoteAddress));
                }
            }

            public void FinishConnect(SocketChannelAsyncOperation operation)
            {
                Contract.Assert(_channel.EventLoop.InEventLoop);

                var ch = Channel;
                try
                {
                    var wasActive = ch.IsActive;
                    ch.DoFinishConnect(operation);
                    FulfillConnectPromise(wasActive);
                }
                catch (Exception ex)
                {
                    var promise = ch._connectPromise;
                    var remoteAddress = promise == null ? null : (EndPoint) promise.Task.AsyncState;
                    FulfillConnectPromise(AnnotateConnectException(ex, remoteAddress));
                }
                finally
                {
                    // Check for null as the connectTimeoutFuture is only created if a connectTimeoutMillis > 0 is used
                    // See https://github.com/netty/netty/issues/1770
                    if (ch._connectCancellationTask != null)
                    {
                        ch._connectCancellationTask.Cancel();
                    }
                    ch._connectPromise = null;
                }
            }

            public abstract void FinishRead(SocketChannelAsyncOperation operation);

            public void FinishWrite(SocketChannelAsyncOperation operation)
            {
                bool resetWritePending = this.Channel.ResetState(StateFlags.WriteScheduled);

                Contract.Assert(resetWritePending);
                var input = OutboundBuffer;
                try
                {
                    operation.Validate();
                    var sent = operation.BytesTransferred;
                    Channel.ResetWriteOperation();
                    if (sent > 0)
                    {
                        input.RemoveBytes(sent);
                    }
                }
                catch (Exception ex)
                {
                    input.FailFlushed(ex, true);
                    throw;
                }

                // directly call super.flush0() to force a flush now
                base.Flush0();
            }


            private void FulfillConnectPromise(bool wasActive)
            {
                var promise = Channel._connectPromise;
                if (promise == null)
                {
                    // Closed via cancellation and the promise has been notified already.
                    return;
                }

                // trySuccess() will return false if a user cancelled the connection attempt.
                var promiseSet = promise.TryComplete();

                // Regardless if the connection attempt was cancelled, channelActive() event should be triggered,
                // because what happened is what happened.
                if (!wasActive && _channel.IsActive)
                {
                    _channel.Pipeline.FireChannelActive();
                }

                // If a user cancelled the connection attempt, close the channel, which is followed by channelInactive().
                if (!promiseSet)
                {
                    CloseAsync();
                }
            }

            private void FulfillConnectPromise(Exception cause)
            {
                var promise = Channel._connectPromise;
                if (promise == null)
                {
                    // Closed via cancellation and the promise has been notified already.
                    return;
                }

                // Use tryFailure() instead of setFailure() to avoid the race against cancel().
                promise.TrySetException(cause);
                CloseIfClosed();
            }

            protected sealed override void Flush0()
            {
                // Flush immediately only when there's no pending flush.
                // If there's a pending flush operation, event loop will call FinishWrite() later,
                // and thus there's no need to call it now.
                if (IsFlushPending())
                {
                    return;
                }
                base.Flush0();
            }

            private bool IsFlushPending()
            {
                return Channel.IsInState(StateFlags.WriteScheduled);
            }
        }
    }
}