using Helios.Net;
using Helios.Topology;

namespace Helios.Reactor.Bootstrap
{
    /// <summary>
    /// A <see cref="IServerFactory"/> instance that spawns <see cref="IReactor"/> instances with UDP transport enabled
    /// </summary>
    public abstract class ServerFactoryBase : ServerBootstrap, IServerFactory
    {
        protected ServerFactoryBase(ServerBootstrap other)
            : base(other)
        {
        }

        protected abstract ReactorBase NewReactorInternal(INode listenAddress);

        public IReactor NewReactor(INode listenAddress)
        {
            var reactor = NewReactorInternal(listenAddress);
            reactor.Configure(Config);

            if (ReceivedData != null)
                reactor.OnReceive += (ReceivedDataCallback)ReceivedData.Clone();
            if (ConnectionEstablishedCallback != null)
                reactor.OnConnection += (ConnectionEstablishedCallback)ConnectionEstablishedCallback.Clone();
            if (ConnectionTerminatedCallback != null)
                reactor.OnDisconnection += (ConnectionTerminatedCallback)ConnectionTerminatedCallback.Clone();

            return reactor;
        }

        public IConnection NewConnection(INode localEndpoint, INode remoteEndpoint)
        {
            var reactor = (ReactorBase)NewReactor(localEndpoint);
            return new ReactorConnectionAdapter(reactor);
        }
    }
}