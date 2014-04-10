using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Helios.Net;
using Helios.Reactor.Response;
using Helios.Topology;

namespace Helios.Reactor
{
    /// <summary>
    /// <see cref="IReactor"/> implementation which spawns <see cref="ReactorProxyResponseChannel"/> instances for interacting directly with connected clients
    /// </summary>
    public abstract class ProxyReactorBase<TIdentifier> : ReactorBase
    {
        /// <summary>
        /// shared buffer used by all incoming connections
        /// </summary>
        protected byte[] Buffer;

        protected Dictionary<TIdentifier, INode> NodeMap = new Dictionary<TIdentifier, INode>();
        protected Dictionary<INode, ReactorResponseChannel> SocketMap = new Dictionary<INode, ReactorResponseChannel>();

        protected ProxyReactorBase(IPAddress localAddress, int localPort, NetworkEventLoop eventLoop, SocketType socketType = SocketType.Stream, ProtocolType protocol = ProtocolType.Tcp, int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE) 
            : base(localAddress, localPort, eventLoop, socketType, protocol, bufferSize)
        {
            Buffer = new byte[bufferSize];
        }

        protected override void ReceivedData(NetworkData availableData, ReactorResponseChannel responseChannel)
        {
            responseChannel.OnReceive(availableData);
        }
    }

    /// <summary>
    /// <see cref="IReactor"/> implementation which spawns <see cref="ReactorProxyResponseChannel"/> instances for responding directly with connected clients,
    /// but maintains a single event loop for responding to incoming requests, rather than allowing each <see cref="ReactorProxyResponseChannel"/> to maintain
    /// its own independent event loop.
    /// 
    /// Great for scenarios where you want to be able to set a single event loop for a server
    /// </summary>
    public abstract class SingleReceiveLoopProxyReactor<TIdentifier> : ProxyReactorBase<TIdentifier>
    {
        protected SingleReceiveLoopProxyReactor(IPAddress localAddress, int localPort, NetworkEventLoop eventLoop, SocketType socketType = SocketType.Stream, ProtocolType protocol = ProtocolType.Tcp, int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE) 
            : base(localAddress, localPort, eventLoop, socketType, protocol, bufferSize)
        {
        }

        protected override void ReceivedData(NetworkData availableData, ReactorResponseChannel responseChannel)
        {
            if (EventLoop.Receive != null)
            {
                EventLoop.Receive(availableData, responseChannel);
            }
        }
    }
}