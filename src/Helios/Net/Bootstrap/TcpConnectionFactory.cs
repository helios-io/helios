using Helios.Net.Connections;

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

        protected override IConnection CreateConnection()
        {
            return new TcpConnection(LocalNode);
        }
    }
}