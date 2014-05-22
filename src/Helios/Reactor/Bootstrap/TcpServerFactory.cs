using System;
using Helios.Reactor.Tcp;
using Helios.Topology;

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

        protected override ReactorBase NewReactorInternal(INode listenAddress)
        {
            if (UseProxies)
                return new TcpProxyReactor(listenAddress.Host, listenAddress.Port, EventLoop, Encoder, Decoder, Allocator, BufferBytes);
            else
                throw new NotImplementedException("Have not implemented non-TCP proxies");
        }
    }
}