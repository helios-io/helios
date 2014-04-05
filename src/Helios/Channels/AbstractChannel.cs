using System.Threading.Tasks;
using Helios.Ops;
using Helios.Topology;

namespace Helios.Channels
{
    /// <summary>
    /// Abstract base class for <see cref="IChannel"/> implementations
    /// </summary>
    public abstract class AbstractChannel : IChannel
    {
        public IChannelId Id { get; private set; }
        public IEventLoop EventLoop { get; private set; }
        public IChannel Parent { get; private set; }
        public bool IsOpen { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsRegistered { get; private set; }
        public INode LocalAddress { get; private set; }
        public INode RemoteAddress { get; private set; }
        public Task<bool> CloseTask { get; private set; }
        public bool IsWriteable { get; private set; }
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
    }
}