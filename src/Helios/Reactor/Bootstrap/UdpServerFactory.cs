using Helios.Reactor.Udp;
using Helios.Topology;

namespace Helios.Reactor.Bootstrap
{
    /// <summary>
    /// <see cref="IServerFactory"/> instance for spawning UDP-based <see cref="IReactor"/> instances
    /// </summary>
    public sealed class UdpServerFactory : ServerFactoryBase
    {
        public UdpServerFactory(ServerBootstrap other) : base(other)
        {
        }

        protected override ReactorBase NewReactorInternal(INode listenAddress)
        {
            return new UdpProxyReactor(listenAddress.Host, listenAddress.Port, EventLoop, BufferBytes);
        }
    }
}