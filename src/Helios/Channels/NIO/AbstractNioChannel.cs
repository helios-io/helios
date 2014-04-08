using System;
using System.Threading;
using System.Threading.Tasks;
using Helios.Net;
using Helios.Ops;
using Helios.Topology;

namespace Helios.Channels.NIO
{
    /// <summary>
    /// Abstract base class for <see cref="IChannel"/> implementations which use async / await for
    /// all I/O operations (non-blocking)
    /// </summary>
    public abstract class AbstractNioChannel : AbstractChannel
    {
        private volatile bool _inputShutdown;

        protected CancellationTokenSource ConnectTimeoutCancellationTokenSource;
        protected readonly IConnection Connection;
        protected ChannelPromise<bool> ConnectPromise;
        private Task _connectTimeoutTask;
        private INode _requestedRemoteAddress;

        protected AbstractNioChannel(IChannel parent, IEventLoop loop, IConnection connection)
            : base(parent, loop)
        {
            Connection = connection;
        }

        public override bool IsOpen
        {
            get { return Connection.IsOpen(); }
        }

        public bool IsInputShutdown
        {
            get { return _inputShutdown; }
            set { _inputShutdown = value; }
        }

        /// <summary>
        /// Connect to the remote peer
        /// </summary>
        protected abstract bool DoConnect(INode remoteAddress, INode localAddress);

        /// <summary>
        /// Finish the connection to the remote peer
        /// </summary>
        protected abstract void DoFinishConnect();

        public new INioUnsafe Unsafe { get { return (INioUnsafe) base.Unsafe; } }

        #region AbstractNioUnsafe implementation

        protected abstract class AbstractNioUnsafe : AbstractUnsafe, INioUnsafe
        {
            protected bool readPending;

            protected AbstractNioUnsafe(AbstractNioChannel channel)
                : base(channel)
            {
                Connection = channel.Connection;
                ReadCallback = ((NioEventLoop) EventLoop).Receive;
            }

            public new AbstractNioChannel Channel { get { return (AbstractNioChannel) base.Channel; } }

            public IConnection Connection { get; private set; }
            public ReceivedDataCallback ReadCallback { get; protected set; }

            public abstract void Read();

            public new void BeginRead()
            {
                // IChannel.Read or IChannelHandlerContext.Read was called
                readPending = true;
                base.BeginRead();
            }

            public override void Connect(INode remoteAddress, INode localAddress, ChannelPromise<bool> connectCompletionSource)
            {
                if (connectCompletionSource.Task.IsCanceled || !EnsureOpen(connectCompletionSource)) return;

                try
                {
                    if (Channel.ConnectPromise != null)
                    {
                        throw new InvalidOperationException("connection attempt was already made.");
                    }

                    var wasActive = Channel.IsActive;
                    if (Channel.DoConnect(remoteAddress, localAddress))
                    {
                        FulfillConnectPromise(connectCompletionSource, wasActive);
                    }
                    else
                    {
                        Channel.ConnectPromise = connectCompletionSource;
                        Channel._requestedRemoteAddress = remoteAddress;

                        //Schedule connect timeout
                        var connectTimeout = Channel.Config.ConnectTimeout;
                        Channel.ConnectTimeoutCancellationTokenSource = new CancellationTokenSource();
                        if (connectTimeout > TimeSpan.Zero)
                        {
                            Channel.ConnectTimeoutCancellationTokenSource.CancelAfter(connectTimeout);
                            Channel._connectTimeoutTask = new Task(() =>
                            {
                                var promise = Channel.ConnectPromise;
                                var cause = new TimeoutException("connection timed out: " + remoteAddress);
                                if (promise != null && !promise.TrySetException(cause))
                                {
                                    Close(VoidPromise());
                                }
                            }, Channel.ConnectTimeoutCancellationTokenSource.Token);
                            EventLoop.Execute(Channel._connectTimeoutTask);
                        }

                        connectCompletionSource.Task.Task.ContinueWith(x =>
                        {
                            if (!x.IsCanceled) return;
                            Channel._connectTimeoutTask = null;
                            Channel.ConnectPromise = null;
                            Close(VoidPromise());
                        });
                    }
                }
                catch (Exception ex)
                {
                    connectCompletionSource.TrySetException(ex);
                    CloseIfClosed();
                }
            }

            private void FulfillConnectPromise(ChannelPromise<bool> promise, bool wasActive)
            {
                if (promise == null || promise.Task.IsCanceled) return; //closed via cancellation and the promise has been notified already

                //TrySetResult will return false if a user cancelled the connection attempt
                var promsiseSet = promise.TrySetResult(true);

                //Regardless if the connection attempt was cancelled, ChannelActive event should be triggered
                if (!wasActive && Channel.IsActive)
                {
                    Channel.Pipeline.FireChannelActive();
                }

                //If a user cancelled the connection attempt, close the channel, which is followed by ChannelInactive
                if (!promsiseSet)
                {
                    Close(VoidPromise());
                }
            }

            private void FulfillConnectPromise(ChannelPromise<bool> promise, Exception ex)
            {
                if (promise == null || promise.Task.IsCanceled) return; //closed via cancellation and the promise has been notified already

                promise.TrySetException(ex);
                CloseIfClosed();
            }

            protected override void DoRegister()
            {
                Connection.BeginReceive(((NioEventLoop)EventLoop).Receive);
            }

            protected override void DoDeregister()
            {
                Connection.StopReceive();
            }

            protected override void DoBeginRead()
            {
                if (Channel.IsInputShutdown || Connection.Receiving) return;

                DoRegister();
            }

            protected override bool IsCompatible(IEventLoop loop)
            {
                return loop is NioEventLoop;
            }
        }

        #endregion
    }

    /// <summary>
    /// Special <see cref="IUnsafe"/> sub-type which allows access to the underlying <see cref="IConnection"/>
    /// </summary>
    public interface INioUnsafe : IUnsafe
    {
        IConnection Connection { get; }

        /// <summary>
        /// Callback method which asynchronously receives read events
        /// </summary>
        ReceivedDataCallback ReadCallback { get; }

        /// <summary>
        /// Read from the underlying <see cref="IConnection"/>
        /// </summary>
        void Read();
    }
}
