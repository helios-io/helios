using Helios.Reactor.Udp;

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

        protected override ReactorBase NewReactorInternal()
        {
            return new UdpProxyReactor(TargetNode.Host, TargetNode.Port, EventLoop, BufferBytes);
        }
    }
}