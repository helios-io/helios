using System;
using System.Configuration;
using System.Diagnostics;
using Helios.Channels.Extensions;
using Helios.Exceptions;
using Helios.Net;
using Helios.Ops;
using Helios.Topology;

namespace Helios.Channels
{
    /// <summary>
    /// Abstract base class for <see cref="IChannel"/> implementations
    /// </summary>
    public abstract class AbstractChannel : IChannel
    {
        static readonly HeliosConnectionException ClosedChannelException = new HeliosConnectionException(ExceptionType.Closed, "Channel is currently closed");
        static readonly HeliosConnectionException NotYetConnectedException = new HeliosConnectionException(ExceptionType.NotOpen, "Channel is not yet open");

        protected AbstractChannel(IChannel parent, IEventLoop loop)
        {
            Parent = parent;
            ValidateEventLoop(loop);
            EventLoop = loop;
            Id = DefaultChannelId.NewChannelId();
// ReSharper disable once DoNotCallOverridableMethodsInConstructor
            Unsafe = NewUnsafe();
            Pipeline = new DefaultChannelPipeline(this);
            _closeFuture = new CloseFuture(this);
        }
        
        private volatile INode _localAddress;
        private volatile INode _remoteAddress;
// ReSharper disable once InconsistentNaming
        internal readonly CloseFuture _closeFuture;

        public IChannelId Id { get; private set; }
        public IEventLoop EventLoop { get; protected set; }
        public IChannelConfig Config { get; internal set; }
        public IChannelPipeline Pipeline { get; private set; }
        public IChannel Parent { get; protected set; }
        public IUnsafe Unsafe { get; private set; }
        public virtual bool IsOpen { get; protected set; }
        public virtual bool IsActive { get; protected set; }
        public virtual bool IsRegistered { get; protected set; }

        public INode LocalAddress
        {
            get
            {
                var local = _localAddress;
                if (_localAddress == null)
                {
                    try
                    {
                        _localAddress = local = Unsafe.LocalAddress;
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }
                }

                return local;
            }
        }

        protected void InvalidateLocalAddress()
        {
            _localAddress = null;
        }

        public INode RemoteAddress
        {
            get
            {
                var remote = _remoteAddress;
                if (remote == null)
                {
                    try
                    {
                        _remoteAddress = remote = Unsafe.RemoteAddress;
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }
                }
                return remote;
            }
        }

        protected void InvalidateRemoteAddress()
        {
            _remoteAddress = null;
        }

        public ChannelFuture CloseTask { get { return _closeFuture.Task; } }
        public bool IsWritable { get; protected set; }

        public ChannelFuture<bool> Bind(INode localAddress)
        {
            return Pipeline.Bind(localAddress);
        }

        public ChannelFuture<bool> Bind(INode localAddress, ChannelPromise<bool> bindCompletionSource)
        {
            return Pipeline.Bind(localAddress, bindCompletionSource);
        }

        public ChannelFuture<bool> Connect(INode remoteAddress)
        {
            return Pipeline.Connect(remoteAddress);
        }

        public ChannelFuture<bool> Connect(INode remoteAddress, ChannelPromise<bool> connectCompletionSource)
        {
            return Pipeline.Connect(remoteAddress, connectCompletionSource);
        }

        public ChannelFuture<bool> Connect(INode remoteAddress, INode localAddress)
        {
            return Pipeline.Connect(remoteAddress, localAddress);
        }

        public ChannelFuture<bool> Connect(INode remoteAddress, INode localAddress, ChannelPromise<bool> connectCompletionSource)
        {
            return Pipeline.Connect(remoteAddress, localAddress, connectCompletionSource);
        }

        public ChannelFuture<bool> Disconnect()
        {
            return Pipeline.Disconnect();
        }

        public ChannelFuture<bool> Disconnect(ChannelPromise<bool> disconnectCompletionSource)
        {
            return Pipeline.Disconnect(disconnectCompletionSource);
        }

        public ChannelFuture<bool> Close(ChannelPromise<bool> closeCompletionSource)
        {
            return Pipeline.Close(closeCompletionSource);
        }

        public IChannel Read()
        {
            Pipeline.Read();
            return this;
        }

        public ChannelFuture<bool> Write(NetworkData message)
        {
            return Pipeline.Write(message);
        }

        public ChannelFuture<bool> Write(NetworkData message, ChannelPromise<bool> writeCompletionSource)
        {
            return Pipeline.Write(message, writeCompletionSource);
        }

        public IChannel Flush()
        {
            Pipeline.Flush();
            return this;
        }

        public ChannelFuture<bool> WriteAndFlush(NetworkData message, ChannelPromise<bool> writeCompletionSource)
        {
            return Pipeline.WriteAndFlush(message, writeCompletionSource);
        }

        public ChannelFuture<bool> WriteAndFlush(NetworkData message)
        {
            return Pipeline.WriteAndFlush(message);
        }

        public ChannelPromise<bool> NewPromise()
        {
            return new ChannelPromise<bool>(this);
        }

        public ChannelFuture<bool> NewFailedFuture(Exception cause)
        {
            var promise = NewPromise();
            promise.TrySetException(cause);
            return promise.Task;
        }

        public ChannelFuture<bool> NewSucceededFuture()
        {
            var promise = NewPromise();
            promise.TrySetResult(true);
            return promise.Task;
        }

        public VoidChannelPromise VoidPromise()
        {
            return new VoidChannelPromise(this);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        #region Internal methods

        internal void ValidateEventLoop(IEventLoop loop)
        {
            if(loop == null)
                throw new InvalidOperationException("Cannot use a null IEventLoop");
        }

        #endregion

        #region IComparable<IChannel> methods

        public int CompareTo(IChannel other)
        {
            return Id.CompareTo(other.Id);
        }

        #endregion


        #region AbstractUnsafe implementation

        protected abstract AbstractUnsafe NewUnsafe();

        protected abstract class AbstractUnsafe : IUnsafe
        {
            protected AbstractUnsafe(AbstractChannel channel)
            {
                Channel = channel;
                EventLoop = channel.EventLoop;
                ValidateEventLoop(EventLoop);
                Invoker = EventLoop.AsInvoker();
                OutboundBuffer = NewOutboundBuffer();
            }

            protected AbstractChannel Channel;
            protected IEventLoop EventLoop;

            private bool inFlush;

            public IChannelHandlerInvoker Invoker { get; private set; }
            public INode LocalAddress { get { return LocalAddressInternal(); }  }
            public INode RemoteAddress { get { return RemoteAddressInternal(); }  }
            public void Register(ChannelPromise<bool> registerPromise)
            {
                if (EventLoop.IsInEventLoop())
                {
                    RegisterInternal(registerPromise);
                }
                else
                {
                    try
                    {
                        EventLoop.Execute(() => RegisterInternal(registerPromise));
                    }
                    catch (Exception ex)
                    {
                        Debug.Write(string.Format("Force-closing a channel whose registration task was not accepted by an event loop: {0} {1}", Channel, ex));
                        CloseForcibly();
                        Channel._closeFuture.SetClosed();
                        SafeSetFailure(registerPromise, ex);
                    }
                }
            }

            protected virtual void RegisterInternal(ChannelPromise<bool> registerPromise)
            {
                try
                {
                    if (registerPromise.Task.IsCanceled || !EnsureOpen(registerPromise))
                    {
                        return;
                    }
                    DoRegister();
                    Channel.IsRegistered = true;
                    SafeSetSuccess(registerPromise);
                    Channel.Pipeline.FireChannelRegistered();
                    if (Channel.IsActive)
                    {
                        Channel.Pipeline.FireChannelActive();
                    }
                }
                catch (Exception ex)
                {
                    CloseForcibly();
                    Channel._closeFuture.SetClosed();
                    SafeSetFailure(registerPromise, ex);
                }
            }

            public virtual void Bind(INode localAddress, ChannelPromise<bool> bindCompletionSource)
            {
                if (bindCompletionSource.Task.IsCanceled || !EnsureOpen(bindCompletionSource))
                {
                    return;
                }

                var wasActive = Channel.IsActive;

                try
                {
                    DoBind(localAddress);
                }
                catch (Exception ex)
                {
                    SafeSetFailure(bindCompletionSource, ex);
                    CloseIfClosed();
                    return;
                }

                if (!wasActive && Channel.IsActive)
                {
                    EventLoop.Execute(() => Channel.Pipeline.FireChannelActive());
                }
                SafeSetSuccess(bindCompletionSource);
            }

            public abstract void Connect(INode remoteAddress, INode localAddress,
                ChannelPromise<bool> connectCompletionSource);

            public virtual void Disconnect(ChannelPromise<bool> disconnectCompletionSource)
            {
                if (disconnectCompletionSource.Task.IsCanceled) return;

                var wasActive = Channel.IsActive;
                try
                {
                    DoDisconnect();
                }
                catch (Exception ex)
                {
                    SafeSetFailure(disconnectCompletionSource, ex);
                    CloseIfClosed();
                    return;
                }

                if (wasActive && !Channel.IsActive)
                {
                    EventLoop.Execute(() => Channel.Pipeline.FireChannelInactive());
                }
                SafeSetSuccess(disconnectCompletionSource);
                CloseIfClosed();
            }

            public virtual void Close(ChannelPromise<bool> closeCompletionSource)
            {
                var channel = Channel;
                if (closeCompletionSource.Task.IsCanceled) return;

                if (inFlush)
                {
                    EventLoop.Execute(() => Close(closeCompletionSource));
                    return;
                }

                if (Channel.CloseTask.IsCompleted)
                {
                    //Closed already
                    SafeSetSuccess(closeCompletionSource);
                    return;
                }

                var wasActive = Channel.IsActive;
                var outboundBuffer = OutboundBuffer;
                OutboundBuffer = null; //disallow adding any messages and flushes to outbound buffer

                try
                {
                    DoClose();
                    Channel._closeFuture.SetClosed();
                    SafeSetSuccess(closeCompletionSource);
                }
                catch (Exception ex)
                {
                    Channel._closeFuture.SetClosed();
                    SafeSetFailure(closeCompletionSource, ex);
                }

                //Fail all the queued messages
                try
                {
                    outboundBuffer.FailFlushed(ClosedChannelException);
                    outboundBuffer.Close(ClosedChannelException);
                }
                finally
                {
                    if (wasActive && !channel.IsActive)
                    {
                        EventLoop.Execute(() => channel.Pipeline.FireChannelInactive());
                    }
                    Deregister();
                }
            }

            public void CloseForcibly()
            {
                try
                {
                    DoClose();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(String.Format("Failed to close a channel.", ex));
                }
            }

            private void Deregister()
            {
                if (!Channel.IsRegistered)
                {
                    return;
                }

                try
                {
                    DoDeregister();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(String.Format("Unexpected exception while deregistering a channel.", ex));
                }
                finally
                {
                    if (Channel.IsRegistered)
                    {
                        Channel.IsRegistered = false;
                    }
                }
            }

            public void BeginRead()
            {
                if (!Channel.IsActive)
                {
                    return;
                }

                try
                {
                    DoBeginRead();
                }
                catch (Exception ex)
                {
                    EventLoop.Execute(() => Channel.Pipeline.FireExceptionCaught(ex));
                    Close(VoidPromise());
                }
            }

            public virtual void Write(NetworkData msg, ChannelPromise<bool> writeCompletionSource)
            {
                var outboundBuffer = OutboundBuffer;
                if (outboundBuffer == null)
                {
                    //If the outboundbuffer is closed, we know hte channel was closed so
                    //we need to fail the future right away. If it is not null, handling of the
                    //rest will be done in FlushInternal
                    SafeSetFailure(writeCompletionSource, ClosedChannelException);
                    return;
                }
                outboundBuffer.AddMessage(msg, writeCompletionSource);
            }

            public virtual void Flush()
            {
                var outboundBuffer = OutboundBuffer;
                if (outboundBuffer == null)
                {
                    return;
                }
                outboundBuffer.AddFlush();
                FlushInternal();
            }

            protected void FlushInternal()
            {
                if (inFlush)
                {
                    //Avoid re-entrance
                    return;
                }

                var outboundBuffer = OutboundBuffer;
                if (outboundBuffer == null || outboundBuffer.IsEmpty) return;

                inFlush = true;

                //Mark all pending write requests as failure if the channel is inactive
                if (!Channel.IsActive)
                {
                    try
                    {
                        if (Channel.IsOpen)
                        {
                            outboundBuffer.FailFlushed(NotYetConnectedException);
                        }
                        else
                        {
                            outboundBuffer.FailFlushed(ClosedChannelException);
                        }
                    }
                    finally
                    {
                        inFlush = false;
                    }
                    return;
                }

                try
                {
                    DoWrite(outboundBuffer);
                }
                catch (Exception ex)
                {
                    outboundBuffer.FailFlushed(ex);
                }
                finally
                {
                    inFlush = false;
                }
            }

            public ChannelOutboundBuffer OutboundBuffer { get; private set; }
            public VoidChannelPromise VoidPromise()
            {
                return new VoidChannelPromise(null);
            }

            protected void ValidateEventLoop(IEventLoop loop)
            {
                if(loop == null) throw new ArgumentNullException("loop");

                if(!IsCompatible(loop)) throw new ArgumentException("EventLoop implementation was not of the expected type.");
            }

            protected abstract bool IsCompatible(IEventLoop loop);

            #region Abstract methods

            protected abstract INode LocalAddressInternal();

            protected abstract INode RemoteAddressInternal();

            protected abstract void DoRegister();

            protected abstract void DoBind(INode localAddress);

            protected abstract void DoDisconnect();

            protected abstract void DoClose();

            protected abstract void DoDeregister();

            protected abstract void DoBeginRead();

            /// <summary>
            /// Flush the content of the given buffer to the remote peer
            /// </summary>
            protected abstract void DoWrite(ChannelOutboundBuffer buff);

            #endregion

            #region Internal methods

            protected ChannelOutboundBuffer NewOutboundBuffer()
            {
                return ChannelOutboundBuffer.NewBuffer(Channel);
            }

            protected bool EnsureOpen(ChannelPromise<bool> promise)
            {
                if (Channel.IsOpen) return true;

                SafeSetFailure(promise, ClosedChannelException);
                return false;
            }

            protected void CloseIfClosed()
            {
                if (Channel.IsOpen) return;

                Close(VoidPromise());
            }

            /// <summary>
            /// Marks <see cref="promise"/> as success.
            /// </summary>
            protected void SafeSetSuccess(ChannelPromise<bool> promise)
            {
                if (!(promise is VoidChannelPromise) && !promise.TrySetResult(true))
                {
                    //add logging here
                }
            }

            /// <summary>
            /// Marks <see cref="promise"/> as a failure with cause <see cref="cause"/>
            /// </summary>
            protected void SafeSetFailure(ChannelPromise<bool> promise, Exception cause)
            {
                if (!(promise is VoidChannelPromise) && !promise.TrySetException(cause))
                {
                    //add logging here
                }
            }

            #endregion
        }

        #endregion

        /// <summary>
        /// Special <see cref="ChannelPromise{T}"/> implementation used for letting listeners know that the linked
        /// <see cref="IChannel"/> has closed.
        /// </summary>
        internal sealed class CloseFuture : ChannelPromise<bool>
        {
            public CloseFuture(AbstractChannel channel) : base(channel)
            {
            }

            public override void SetResult(bool result)
            {
                throw new InvalidOperationException();
            }

            public override void SetException(Exception ex)
            {
                throw new InvalidOperationException();
            }

            public override bool TrySetException(Exception ex)
            {
                throw new InvalidOperationException();
            }

            public override bool TrySetResult(bool result)
            {
                throw new InvalidOperationException();
            }

            public bool SetClosed()
            {
                return base.TrySetResult(true);
            }
        }

    }

}