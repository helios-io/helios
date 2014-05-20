using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Helios.Net;
using Helios.Reactor.Response;
using Helios.Serialization;
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

        protected ProxyReactorBase(IPAddress localAddress, int localPort, NetworkEventLoop eventLoop, IMessageEncoder encoder, IMessageDecoder decoder, SocketType socketType = SocketType.Stream, ProtocolType protocol = ProtocolType.Tcp, int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE) 
            : base(localAddress, localPort, eventLoop, encoder, decoder, socketType, protocol, bufferSize)
        {
            Buffer = new byte[bufferSize];
        }

        protected override void ReceivedData(NetworkData availableData, ReactorResponseChannel responseChannel)
        {
            List<NetworkData> decoded;
            Decoder.Decode(responseChannel, availableData, out decoded);
            foreach(var message in decoded)
                responseChannel.OnReceive(message);
        }
    }
}