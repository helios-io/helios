using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Helios.Buffers;
using Helios.Concurrency;
using Helios.Logging;
using Helios.Util.Concurrency;

namespace Helios.Channels
{
    public abstract class AbstractChannel : IChannel
    {
        private static readonly ILogger Logger = LoggingFactory.GetLogger<AbstractChannel>();

        private IMessageSizeEstimatorHandle _estimatorHandle;
        private readonly IChannelUnsafe _channelUnsafe;
        private readonly DefaultChannelPipeline _pipeline;
        private readonly TaskCompletionSource _closeTask = new TaskCompletionSource();

        private volatile EndPoint _localAddress;
        private volatile EndPoint _remoteAddress;
        private volatile PausableChannelEventLoop eventLoop;
        private volatile bool registered;

        /// <summary> Cache for the string representation of this channel /// </summary>
        bool strValActive;

        string strVal;

        protected AbstractChannel(IChannel parent)
        {
            Parent = parent;
            // ReSharper disable once VirtualMemberCallInContructor
            _channelUnsafe = NewUnsafe();
            _pipeline = new DefaultChannelPipeline(this);
        }

        /// <summary>
        /// Create a new <see cref="AbstractUnsafe"/> instance which will be used for the life-time of the <see cref="IChannel"/>
        /// </summary>
        protected abstract IChannelUnsafe NewUnsafe();

        public IByteBufAllocator Allocator => Configuration.Allocator;

        public IEventLoop EventLoop
        {
            get
            {
                var loop = eventLoop;
                if(loop == null)
                    throw new InvalidOperationException("Channel is not registered to an event loop");
                return loop;
            }
        }

        public IChannel Parent { get; }
        public abstract bool DisconnectSupported { get; }
        public abstract bool Open { get; }
        public abstract bool Active { get; }
        public bool Registered => registered;

        public EndPoint LocalAddress
        {
            get
            {
                EndPoint address = _localAddress;
                return address ?? this.CacheLocalAddress();
            }
        }

        public EndPoint RemoteAddress
        {
            get
            {
                EndPoint address = _remoteAddress;
                return address ?? this.CacheRemoteAddress();
            }
        }

        protected abstract EndPoint LocalAddressInternal { get; }

        protected void InvalidateLocalAddress()
        {
            _localAddress = null;
        }

        protected EndPoint CacheLocalAddress()
        {
            try
            {
                return _localAddress = this.LocalAddressInternal;
            }
            catch (Exception)
            {
                // Sometimes fails on a closed socket in Windows.
                return null;
            }
        }

        protected abstract EndPoint RemoteAddressInternal { get; }

        protected void InvalidateRemoteAddress()
        {
            _remoteAddress = null;
        }

        protected EndPoint CacheRemoteAddress()
        {
            try
            {
                return _remoteAddress = this.RemoteAddressInternal;
            }
            catch (Exception)
            {
                // Sometimes fails on a closed socket in Windows.
                return null;
            }
        }

        public bool IsWritable
        {
            get
            {
                var buf = _channelUnsafe.OutboundBuffer;
                return buf != null && buf.IsWritable;
            }
        }

        public IChannelUnsafe Unsafe => _channelUnsafe;
        public IChannelPipeline Pipeline => _pipeline;
        public abstract IChannelConfiguration Configuration { get; }
        public Task CloseCompletion => _closeTask.Task;
        public Task DeregisterAsync()
        {
            throw new NotImplementedException();
        }

        public Task BindAsync(EndPoint localAddress)
        {
            throw new NotImplementedException();
        }

        public Task ConnectAsync(EndPoint remoteAddress)
        {
            throw new NotImplementedException();
        }

        public Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress)
        {
            throw new NotImplementedException();
        }

        public Task DisconnectAsync()
        {
            throw new NotImplementedException();
        }

        public Task CloseAsync()
        {
            throw new NotImplementedException();
        }

        public IChannel Read()
        {
            throw new NotImplementedException();
        }

        public Task WriteAsync(object message)
        {
            throw new NotImplementedException();
        }

        public IChannel Flush()
        {
            throw new NotImplementedException();
        }

        public Task WriteAndFlushAsync(object message)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Return {@code true} if the given {@link EventLoop} is compatible with this instance.
        /// </summary>
        protected abstract bool IsCompatible(IEventLoop eventLoop);

        /// <summary>
        /// Is called after the {@link Channel} is registered with its {@link EventLoop} as part of the register process.
        ///
        /// Sub-classes may override this method
        /// </summary>
        protected virtual void DoRegister()
        {
            // NOOP
        }

        /// <summary>
        /// Bind the {@link Channel} to the {@link EndPoint}
        /// </summary>
        protected abstract void DoBind(EndPoint localAddress);

        /// <summary>
        /// Disconnect this {@link Channel} from its remote peer
        /// </summary>
        protected abstract void DoDisconnect();

        /// <summary>
        /// Close the {@link Channel}
        /// </summary>
        protected abstract void DoClose();

        /// <summary>
        /// Deregister the {@link Channel} from its {@link EventLoop}.
        ///
        /// Sub-classes may override this method
        /// </summary>
        protected virtual void DoDeregister()
        {
            // NOOP
        }

        /// <summary>
        /// ScheduleAsync a read operation.
        /// </summary>
        protected abstract void DoBeginRead();

        /// <summary>
        /// Flush the content of the given buffer to the remote peer.
        /// </summary>
        protected abstract void DoWrite(ChannelOutboundBuffer input);

        /// <summary>
        /// Invoked when a new message is added to a {@link ChannelOutboundBuffer} of this {@link AbstractChannel}, so that
        /// the {@link Channel} implementation converts the message to another. (e.g. heap buffer -> direct buffer)
        /// </summary>
        protected virtual object FilterOutboundMessage(object msg)
        {
            return msg;
        }

        #region AbstractUnsafe

        /// <summary>
        /// <see cref="IChannelUnsafe"/> implementation which sub-classes must extend and use.
        /// </summary>
        protected abstract class AbstractUnsafe : IChannelUnsafe
        {
            protected readonly AbstractChannel channel;
            ChannelOutboundBuffer outboundBuffer;
            IRecvByteBufferAllocatorHandle recvHandle;
            bool inFlush0;

            /// <summary> true if the channel has never been registered, false otherwise /// </summary>
            bool neverRegistered = true;

            public IRecvByteBufferAllocatorHandle RecvBufAllocHandle
            {
                get
                {
                    if (this.recvHandle == null)
                    {
                        this.recvHandle = this.channel.Configuration.RecvByteBufAllocator.NewHandle();
                    }
                    return this.recvHandle;
                }
            }

            //public ChannelHandlerInvoker invoker() {
            //    // return the unwrapped invoker.
            //    return ((PausableChannelEventExecutor) eventLoop().asInvoker()).unwrapInvoker();
            //}

            protected AbstractUnsafe(AbstractChannel channel)
            {
                this.channel = channel;
                this.outboundBuffer = new ChannelOutboundBuffer(channel);
            }

            public ChannelOutboundBuffer OutboundBuffer
            {
                get { return this.outboundBuffer; }
            }

            public Task RegisterAsync(IEventLoop eventLoop)
            {
                Contract.Requires(eventLoop != null);
                if (this.channel.Registered)
                {
                    return TaskEx.FromException(new InvalidOperationException("registered to an event loop already"));
                }
                if (!this.channel.IsCompatible(eventLoop))
                {
                    return TaskEx.FromException(new InvalidOperationException("incompatible event loop type: " + eventLoop.GetType().Name));
                }

                // It's necessary to reuse the wrapped eventloop object. Otherwise the user will end up with multiple
                // objects that do not share a common state.
                if (this.channel.eventLoop == null)
                {
                    this.channel.eventLoop = new PausableChannelEventLoop(this.channel, eventLoop);
                }
                else
                {
                    this.channel.eventLoop.Unwrapped = eventLoop;
                }

                var promise = new TaskCompletionSource();

                if (eventLoop.InEventLoop)
                {
                    this.Register0(promise);
                }
                else
                {
                    try
                    {
                        eventLoop.Execute(() => this.Register0(promise));
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(
                            string.Format("Force-closing a channel whose registration task was not accepted by an event loop: {0}", this.channel),
                            ex);
                        this.CloseForcibly();
                        this.channel.closeFuture.Complete();
                        Util.SafeSetFailure(promise, ex, Logger);
                    }
                }

                return promise.Task;
            }

            void Register0(TaskCompletionSource promise)
            {
                try
                {
                    // check if the channel is still open as it could be closed input the mean time when the register
                    // call was outside of the eventLoop
                    if (!promise.setUncancellable() || !this.EnsureOpen(promise))
                    {
                        Util.SafeSetFailure(promise, ClosedChannelException, Logger);
                        return;
                    }
                    bool firstRegistration = this.neverRegistered;
                    this.channel.DoRegister();
                    this.neverRegistered = false;
                    this.channel.registered = true;
                    this.channel.eventLoop.AcceptNewTasks();
                    Util.SafeSetSuccess(promise, Logger);
                    this.channel.pipeline.FireChannelRegistered();
                    // Only fire a channelActive if the channel has never been registered. This prevents firing
                    // multiple channel actives if the channel is deregistered and re-registered.
                    if (firstRegistration && this.channel.Active)
                    {
                        this.channel.pipeline.FireChannelActive();
                    }
                }
                catch (Exception t)
                {
                    // Close the channel directly to avoid FD leak.
                    CloseForcibly();
                    channel._closeTask.Complete();
                    PromiseUtil.SafeSetFailure(promise, t, Logger);
                }
            }

            public Task BindAsync(EndPoint localAddress)
            {
                // todo: cancellation support
                if ( /*!promise.setUncancellable() || */!channel.Open)
                {
                    return this.CreateClosedChannelExceptionTask();
                }

                //// See: https://github.com/netty/netty/issues/576
                //if (bool.TrueString.Equals(this.channel.Configuration.getOption(ChannelOption.SO_BROADCAST)) &&
                //    localAddress is IPEndPoint &&
                //    !((IPEndPoint)localAddress).Address.getAddress().isAnyLocalAddress() &&
                //    !Environment.OSVersion.Platform == PlatformID.Win32NT && !Environment.isRoot())
                //{
                //    // Warn a user about the fact that a non-root user can't receive a
                //    // broadcast packet on *nix if the socket is bound on non-wildcard address.
                //    logger.Warn(
                //        "A non-root user can't receive a broadcast packet if the socket " +
                //            "is not bound to a wildcard address; binding to a non-wildcard " +
                //            "address (" + localAddress + ") anyway as requested.");
                //}

                bool wasActive = this.channel.Active;
                var promise = new TaskCompletionSource();
                try
                {
                    this.channel.DoBind(localAddress);
                }
                catch (Exception t)
                {
                    PromiseUtil.SafeSetFailure(promise, t, Logger);
                    this.CloseIfClosed();
                    return promise.Task;
                }

                if (!wasActive && channel.Active)
                {
                    InvokeLater(() => this.channel._pipeline.FireChannelActive());
                }

                SafeSetSuccess(promise);

                return promise.Task;
            }

            public abstract Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress);

            static void SafeSetFailure(TaskCompletionSource promise, Exception cause)
            {
                PromiseUtil.SafeSetFailure(promise, cause, Logger);
            }

            public Task DisconnectAsync()
            {
                var promise = new TaskCompletionSource();
                if (!promise.SetUncancellable())
                {
                    return promise.Task;
                }

                bool wasActive = this.channel.Active;
                try
                {
                    this.channel.DoDisconnect();
                }
                catch (Exception t)
                {
                    SafeSetFailure(promise, t);
                    this.CloseIfClosed();
                    return promise.Task;
                }

                if (wasActive && !this.channel.Active)
                {
                    this.InvokeLater(() => this.channel._pipeline.FireChannelInactive());
                }

                this.SafeSetSuccess(promise);
                this.CloseIfClosed(); // doDisconnect() might have closed the channel

                return promise.Task;
            }

            public void SafeSetSuccess(TaskCompletionSource promise)
            {
                PromiseUtil.SafeSetSuccess(promise, Logger);
            }

            public Task CloseAsync() //CancellationToken cancellationToken)
            {
                var promise = new TaskCompletionSource();
                if (!promise.SetUncancellable())
                {
                    return promise.Task;
                }
                //if (cancellationToken.IsCancellationRequested)
                //{
                //    return TaskEx.Cancelled;
                //}

                if (this.outboundBuffer == null)
                {
                    // Only needed if no VoidChannelPromise.
                    if (promise != TaskCompletionSource.Void)
                    {
                        // This means close() was called before so we just register a listener and return
                        return this.channel._closeTask.Task;
                    }
                    return promise.Task;
                }

                if (this.channel._closeTask.Task.IsCompleted)
                {
                    // Closed already.
                    PromiseUtil.SafeSetSuccess(promise, Logger);
                    return promise.Task;
                }

                bool wasActive = this.channel.Active;
                ChannelOutboundBuffer buffer = this.outboundBuffer;
                this.outboundBuffer = null; // Disallow adding any messages and flushes to outboundBuffer.

                try
                {
                    // Close the channel and fail the queued messages input all cases.
                    this.DoClose0(promise);
                }
                finally
                {
                    // Fail all the queued messages.
                    buffer.FailFlushed(ClosedChannelException.Instance, false);
                    buffer.Close(ClosedChannelException.Instance);
                }
                if (this.inFlush0)
                {
                    this.InvokeLater(() => this.FireChannelInactiveAndDeregister(wasActive));
                }
                else
                {
                    this.FireChannelInactiveAndDeregister(wasActive);
                }


                return promise.Task;
            }

            void DoClose0(TaskCompletionSource promise)
            {
                try
                {
                    this.channel.DoClose();
                    this.channel._closeTask.Complete();
                    this.SafeSetSuccess(promise);
                }
                catch (Exception t)
                {
                    this.channel._closeTask.Complete();
                    SafeSetFailure(promise, t);
                }
            }

            void FireChannelInactiveAndDeregister(bool wasActive)
            {
                if (wasActive && !this.channel.Active)
                {
                    this.InvokeLater(() =>
                    {
                        this.channel._pipeline.FireChannelInactive();
                        this.DeregisterAsync();
                    });
                }
                else
                {
                    this.InvokeLater(() => this.DeregisterAsync());
                }
            }

            public void CloseForcibly()
            {
                try
                {
                    this.channel.DoClose();
                }
                catch (Exception e)
                {
                    Logger.Warning("Failed to close a channel. Cause: {0}", e);
                }
            }

            /// <summary>
            /// This method must NEVER be called directly, but be executed as an
            /// extra task with a clean call stack instead. The reason for this
            /// is that this method calls {@link ChannelPipeline#fireChannelUnregistered()}
            /// directly, which might lead to an unfortunate nesting of independent inbound/outbound
            /// events. See the comments input {@link #invokeLater(Runnable)} for more details.
            /// </summary>
            public Task DeregisterAsync()
            {
                //if (!promise.setUncancellable())
                //{
                //    return;
                //}

                if (!this.channel.registered)
                {
                    return TaskEx.Completed;
                }

                try
                {
                    this.channel.DoDeregister();
                }
                catch (Exception t)
                {
                    Logger.Warning("Unexpected exception occurred while deregistering a channel. Cause: {0}", t);
                    return TaskEx.FromException(t);
                }
                finally
                {
                    if (this.channel.registered)
                    {
                        this.channel.registered = false;
                        this.channel._pipeline.FireChannelUnregistered();
                    }
                    else
                    {
                        // Some transports like local and AIO does not allow the deregistration of
                        // an open channel.  Their doDeregister() calls close().  Consequently,
                        // close() calls deregister() again - no need to fire channelUnregistered.
                    }
                }
                return TaskEx.Completed;
            }

            public void BeginRead()
            {
                if (!this.channel.Active)
                {
                    return;
                }

                try
                {
                    this.channel.DoBeginRead();
                }
                catch (Exception e)
                {
                    this.InvokeLater(() => this.channel._pipeline.FireExceptionCaught(e));
                    this.CloseAsync();
                }
            }

            public Task WriteAsync(object msg)
            {
                ChannelOutboundBuffer outboundBuffer = this.outboundBuffer;
                if (outboundBuffer == null)
                {
                    // If the outboundBuffer is null we know the channel was closed and so
                    // need to fail the future right away. If it is not null the handling of the rest
                    // will be done input flush0()
                    // See https://github.com/netty/netty/issues/2362

                    // release message now to prevent resource-leak
                    // TODO: referencing counting
                    return TaskEx.FromException(ClosedChannelException.Instance);
                }

                int size;
                try
                {
                    msg = this.channel.FilterOutboundMessage(msg);
                    size = this.channel.EstimatorHandle.Size(msg);
                    if (size < 0)
                    {
                        size = 0;
                    }
                }
                catch (Exception t)
                {
                    // TODO: reference counting

                    return TaskEx.FromException(t);
                }

                var promise = new TaskCompletionSource();
                outboundBuffer.AddMessage(msg, size, promise);
                return promise.Task;
            }

            public void Flush()
            {
                ChannelOutboundBuffer outboundBuffer = this.outboundBuffer;
                if (outboundBuffer == null)
                {
                    return;
                }

                outboundBuffer.AddFlush();
                Flush0();
            }

            protected virtual void Flush0()
            {
                if (inFlush0)
                {
                    // Avoid re-entrance
                    return;
                }

                ChannelOutboundBuffer outboundBuffer = this.outboundBuffer;
                if (outboundBuffer == null || outboundBuffer.IsEmpty)
                {
                    return;
                }

                this.inFlush0 = true;

                // Mark all pending write requests as failure if the channel is inactive.
                if (!this.channel.Active)
                {
                    try
                    {
                        if (this.channel.Open)
                        {
                            outboundBuffer.FailFlushed(NotYetConnectedException.Instance, true);
                        }
                        else
                        {
                            // Do not trigger channelWritabilityChanged because the channel is closed already.
                            outboundBuffer.FailFlushed(ClosedChannelException.Instance, false);
                        }
                    }
                    finally
                    {
                        this.inFlush0 = false;
                    }
                    return;
                }

                try
                {
                    this.channel.DoWrite(outboundBuffer);
                }
                catch (Exception t)
                {
                    outboundBuffer.FailFlushed(t, true);
                }
                finally
                {
                    this.inFlush0 = false;
                }
            }

            protected bool EnsureOpen(TaskCompletionSource promise)
            {
                if (this.channel.Open)
                {
                    return true;
                }

                PromiseUtil.SafeSetFailure(promise, ClosedChannelException.Instance, Logger);
                return false;
            }

            protected Task CreateClosedChannelExceptionTask()
            {
                return TaskEx.FromException(ClosedChannelException.Instance);
            }

            protected void CloseIfClosed()
            {
                if (this.channel.Open)
                {
                    return;
                }
                this.CloseAsync();
            }

            void InvokeLater(Action task)
            {
                try
                {
                    // This method is used by outbound operation implementations to trigger an inbound event later.
                    // They do not trigger an inbound event immediately because an outbound operation might have been
                    // triggered by another inbound event handler method.  If fired immediately, the call stack
                    // will look like this for example:
                    //
                    //   handlerA.inboundBufferUpdated() - (1) an inbound handler method closes a connection.
                    //   -> handlerA.ctx.close()
                    //      -> channel.unsafe.close()
                    //         -> handlerA.channelInactive() - (2) another inbound handler method called while input (1) yet
                    //
                    // which means the execution of two inbound handler methods of the same handler overlap undesirably.
                    channel.EventLoop.Execute(task);
                }
                catch (RejectedTaskException e)
                {
                    Logger.Warning("Can't invoke task later as EventLoop rejected it; Cause: {0}", e);
                }
            }

            protected Exception AnnotateConnectException(Exception exception, EndPoint remoteAddress)
            {
                if (exception is SocketException)
                {
                    return new ConnectException("LogError connecting to " + remoteAddress, exception);
                }

                return exception;
            }
        }


        #endregion

        #region PausableChannelEventLoop 

        sealed class PausableChannelEventLoop : PausableChannelEventExecutor, IEventLoop
        {
            volatile bool _isAcceptingNewTasks = true;
            public volatile IEventLoop Unwrapped;
            readonly IChannel _channel;

            public PausableChannelEventLoop(IChannel channel, IEventLoop unwrapped)
            {
                _channel = channel;
                Unwrapped = unwrapped;
            }

            public override void RejectNewTasks()
            {
                _isAcceptingNewTasks = false;
            }

            public override void AcceptNewTasks()
            {
                _isAcceptingNewTasks = true;
            }

            public override bool IsAcceptingNewTasks => _isAcceptingNewTasks;

            public override IEventExecutor Unwrap()
            {
                return Unwrapped;
            }

            IEventLoop IEventLoop.Unwrap()
            {
                return Unwrapped;
            }

            public IChannelHandlerInvoker Invoker => Unwrapped.Invoker;

            public Task RegisterAsync(IChannel c)
            {
                return this.Unwrapped.RegisterAsync(c);
            }

            internal override IChannel Channel => _channel;
        }

        #endregion
    }
}
