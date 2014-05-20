using Helios.Net.Connections;
using Helios.Serialization;
using Helios.Topology;

namespace Helios.Net.Bootstrap
{
    /// <summary>
    /// <see cref="IConnectionFactory"/> for spawning <see cref="TcpConnection"/> instances
    /// </summary>
    public sealed class TcpConnectionFactory : ClientConnectionFactoryBase
    {
        public TcpConnectionFactory(ClientBootstrap clientBootstrap) : base(clientBootstrap)
        {
        }

        protected override IConnection CreateConnection(INode localEndpoint, INode remoteEndpoint)
        {
            return new TcpConnection(EventLoop, remoteEndpoint, Encoder, Decoder);
        }
    }
}