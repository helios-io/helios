using Helios.Net;
using Helios.Net.Bootstrap;

namespace Helios.Reactor.Bootstrap
{
    /// <summary>
    /// A <see cref="IServerFactory"/> instance that spawns <see cref="IReactor"/> instances with UDP transport enabled
    /// </summary>
    public abstract class ServerFactoryBase : ServerBootstrap, IServerFactory, IConnectionFactory
    {
        protected ServerFactoryBase(ServerBootstrap other)
            : base(other)
        {
        }

        protected abstract ReactorBase NewReactorInternal();

        public IReactor NewReactor()
        {
            var reactor = NewReactorInternal();
            reactor.Configure(Config);

            if (ReceivedData != null)
                reactor.OnReceive += (ReceivedDataCallback)ReceivedData.Clone();
            if (ConnectionEstablishedCallback != null)
                reactor.OnConnection += (ConnectionEstablishedCallback)ConnectionEstablishedCallback.Clone();
            if (ConnectionTerminatedCallback != null)
                reactor.OnDisconnection += (ConnectionTerminatedCallback)ConnectionTerminatedCallback.Clone();

            return reactor;
        }

        public IConnection NewConnection()
        {
            var reactor = (ReactorBase)NewReactor();
            return new ReactorConnectionAdapter(reactor);
        }
    }
}