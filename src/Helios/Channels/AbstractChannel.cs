using System;
using System.Threading.Tasks;
using Helios.Channels.Impl;
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
        protected AbstractChannel(IChannel parent, IEventLoop loop)
        {
            Parent = parent;
            ValidateEventLoop(loop);
            EventLoop = loop;
            Id = DefaultChannelId.NewChannelId();
// ReSharper disable once DoNotCallOverridableMethodsInConstructor
            Unsafe = NewUnsafe();
            Pipeline = new DefaultChannelPipeline(this);
        }
        
        private volatile INode _localAddress;
        private volatile INode _remoteAddress;

        public IChannelId Id { get; private set; }
        public IEventLoop EventLoop { get; protected set; }
        public IChannelConfig Config { get; private set; }
        public IChannelPipeline Pipeline { get; private set; }
        public IChannel Parent { get; protected set; }
        public IUnsafe Unsafe { get; private set; }
        public bool IsOpen { get; protected set; }
        public bool IsActive { get; protected set; }
        public bool IsRegistered { get; protected set; }

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
// ReSharper disable once UnusedVariable
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

        public Task<bool> CloseTask { get; protected set; }
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
            private ChannelOutboundBuffer _buffer = new ChannelOutboundBuffer();
            private bool inFlush;

            public IChannelHandlerInvoker Invoker { get; private set; }
            public INode LocalAddress { get; private set; }
            public INode RemoteAddress { get; private set; }
            public void Register(ChannelPromise<bool> registerPromise)
            {
                throw new NotImplementedException();
            }

            public void Bind(INode localAddress, ChannelPromise<bool> bindCompletionSource)
            {
                throw new NotImplementedException();
            }

            public void Connect(INode localAddress, INode remoteAddress, ChannelPromise<bool> connectCompletionSource)
            {
                throw new NotImplementedException();
            }

            public void Disconnect(ChannelPromise<bool> disconnectCompletionSource)
            {
                throw new NotImplementedException();
            }

            public void Close(ChannelPromise<bool> closeCompletionSource)
            {
                throw new NotImplementedException();
            }

            public void CloseForcibly()
            {
                throw new NotImplementedException();
            }

            public void BeginRead()
            {
                throw new NotImplementedException();
            }

            public void Write(NetworkData msg, ChannelPromise<bool> writeCompletionSource)
            {
                throw new NotImplementedException();
            }

            public void Flush()
            {
                throw new NotImplementedException();
            }

            public ChannelOutboundBuffer OutboundBuffer { get; private set; }
            public VoidChannelPromise VoidPromise()
            {
                return new VoidChannelPromise(null);
            }
        }

        #endregion

    }

}