using Helios.Net.Connections;
using Helios.Topology;

namespace Helios.Net.Bootstrap
{
    /// <summary>
    /// <see cref="IConnectionFactory"/> for spawning <see cref="UdpConnection"/> instances
    /// </summary>
    public sealed class UdpConnectionFactory : ClientConnectionFactoryBase
    {
        public UdpConnectionFactory(ClientBootstrap clientBootstrap) : base(clientBootstrap)
        {
        }

        protected override IConnection CreateConnection(INode localEndpoint, INode remoteEndpoint)
        {
            return new UdpConnection(EventLoop, localEndpoint, Encoder, Decoder);
        }
    }
}