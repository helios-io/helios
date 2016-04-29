using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Helios.Buffers;
using Helios.Concurrency;
using Helios.Logging;

namespace Helios.Channels.Sockets
{
    public abstract class AbstractSocketChannel : ISocketChannel
    {
        private static readonly ILogger Logger = LoggingFactory.GetLogger<AbstractSocketChannel>();

        [Flags]
        protected enum StateFlags
        {
            Open = 1,
            ReadScheduled = 1 << 1,
            WriteScheduled = 1 << 2,
            Active = 1 << 3
            // todo: add input shutdown and read pending here as well?
        }

        protected readonly Socket Socket;
        private SocketChannelAsyncOperation _readOperation;
        private SocketChannelAsyncOperation _writeOperation;
        private volatile bool _inputShutdown;
        private volatile bool _readPending;
        private volatile StateFlags _state;

        TaskCompletionSource connectPromise;

        public IChannelId Id { get; }
        public IByteBufAllocator Allocator { get; }
        public IEventLoop EventLoop { get; }
        public IServerSocketChannel Parent { get; }
        public ISocketChannelConfiguration Configuration { get; }

        IChannel IChannel.Parent
        {
            get { return Parent; }
        }

        public bool DisconnectSupported { get; }
        public bool IsOpen { get; }
        public bool IsActive { get; }
        public bool Registered { get; }
        public EndPoint LocalAddress { get; }
        public EndPoint RemoteAddress { get; }
        public bool IsWritable { get; }
        public IChannelUnsafe Unsafe { get; }
        public IChannelPipeline Pipeline { get; }
        IChannelConfiguration IChannel.Configuration
        {
            get { return Configuration; }
        }

        public Task CloseCompletion { get; }
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
    }
}
