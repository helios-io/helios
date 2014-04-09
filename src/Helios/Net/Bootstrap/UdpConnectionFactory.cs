using Helios.Net.Connections;

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

        protected override IConnection CreateConnection()
        {
            return new UdpConnection(EventLoop, TargetNode);
        }
    }
}