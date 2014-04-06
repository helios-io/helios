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
        }

        private ChannelOutboundBuffer _buffer = new ChannelOutboundBuffer();

        public IChannelId Id { get; private set; }
        public IEventLoop EventLoop { get; protected set; }
        public IChannelConfig Config { get; private set; }
        public IChannelPipeline Pipeline { get; private set; }
        public IChannel Parent { get; protected set; }
        public IUnsafe Unsafe { get; private set; }
        public bool IsOpen { get; protected set; }
        public bool IsActive { get; protected set; }
        public bool IsRegistered { get; protected set; }
        public INode LocalAddress { get; protected set; }
        public INode RemoteAddress { get; protected set; }
        public Task<bool> CloseTask { get; protected set; }
        public bool IsWritable { get; protected set; }

        public Task<bool> Bind(INode localAddress)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> Bind(INode localAddress, TaskCompletionSource<bool> bindCompletionSource)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> Connect(INode remoteAddress)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> Connect(INode remoteAddress, TaskCompletionSource<bool> connectCompletionSource)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> Connect(INode remoteAddress, INode localAddress)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> Connect(INode remoteAddress, INode localAddress, TaskCompletionSource<bool> connectCompletionSource)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> Disconnect()
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> Disconnect(TaskCompletionSource<bool> disconnectCompletionSource)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> Close(TaskCompletionSource<bool> closeCompletionSource)
        {
            throw new System.NotImplementedException();
        }

        public IChannel Read()
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> Write(NetworkData message)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> Write(NetworkData message, TaskCompletionSource<bool> writeCompletionSource)
        {
            throw new System.NotImplementedException();
        }

        public IChannel Flush()
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> WriteAndFlush(NetworkData message, TaskCompletionSource<bool> writeCompletionSource)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> WriteAndFlush(NetworkData message)
        {
            throw new System.NotImplementedException();
        }

        public VoidChannelPromise VoidPromise { get; private set; }

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
            throw new NotImplementedException();
        }

        #endregion
    }
}