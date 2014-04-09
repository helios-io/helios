using System;
using Helios.Reactor.Tcp;

namespace Helios.Reactor.Bootstrap
{
    /// <summary>
    /// <see cref="IServerFactory"/> instance for spawning TCP-based <see cref="IReactor"/> instances
    /// </summary>
    public sealed class TcpServerFactory : ServerFactoryBase
    {
        public TcpServerFactory(ServerBootstrap other) : base(other)
        {
        }

        protected override ReactorBase NewReactorInternal()
        {
            if (UseProxies)
                return new TcpProxyReactor(TargetNode.Host, TargetNode.Port, EventLoop, BufferBytes);
            else
                throw new NotImplementedException("Have not implemented non-TCP proxies");
        }
    }
}