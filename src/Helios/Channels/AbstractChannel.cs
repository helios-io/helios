using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Helios.Channels.Impl;
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

        public IChannelId Id { get; private set; }
        public IEventLoop EventLoop { get; protected set; }
        public IChannel Parent { get; protected set; }
        public bool IsOpen { get; protected set; }
        public bool IsActive { get; protected set; }
        public bool IsRegistered { get; protected set; }
        public INode LocalAddress { get; protected set; }
        public INode RemoteAddress { get; protected set; }
        public Task<bool> CloseTask { get; protected set; }
        public bool IsWriteable { get; protected set; }

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

        public Task<bool> Write(object message)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> Write(object message, TaskCompletionSource<bool> writeCompletionSource)
        {
            throw new System.NotImplementedException();
        }

        public IChannel Flush()
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> WriteAndFlush(object message, TaskCompletionSource<bool> writeCompletionSource)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> WriteAndFlush(object message)
        {
            throw new System.NotImplementedException();
        }

        #region Internal methods

        internal void ValidateEventLoop(IEventLoop loop)
        {
            if(loop == null)
                throw new InvalidOperationException("Cannot use a null IEventLoop");
        }

        #endregion
    }
}