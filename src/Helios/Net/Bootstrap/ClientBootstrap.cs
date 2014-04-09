using System;
using System.Net;
using Helios.Ops;
using Helios.Ops.Executors;
using Helios.Topology;

namespace Helios.Net.Bootstrap
{
    /// <summary>
    /// Client bootstrap for outbound connections
    /// </summary>
    public class ClientBootstrap : AbstractBootstrap
    {
        public ClientBootstrap() : base()
        {
            Workers = 2;
            InternalExecutor = new BasicExecutor();
        }

        public ClientBootstrap(ClientBootstrap other) : base(other)
        {
            Workers = other.Workers;
            InternalExecutor = other.InternalExecutor;
        }

        public TransportType Type { get; private set; }

        protected IExecutor InternalExecutor { get; set; }

        protected NetworkEventLoop EventLoop
        {
            get
            {
                return EventLoopFactory.CreateNetworkEventLoop(Workers, InternalExecutor);
            }
        }

        protected int Workers { get; set; }

        public ClientBootstrap WorkerThreads(int workerThreadCount)
        {
            if (workerThreadCount < 1) throw new ArgumentException("Can't be below 1", "workerThreadCount");
            Workers = workerThreadCount;
            return this;
        }

        public ClientBootstrap Executor(IExecutor executor)
        {
            if (executor == null) throw new ArgumentNullException("executor");
            InternalExecutor = executor;
            return this;
        }

        public ClientBootstrap SetTransport(TransportType type)
        {
            Type = type;
            return this;
        }

        public ClientBootstrap RemoteAddress(INode node)
        {
            return this;
        }

        public new ClientBootstrap OnConnect(ConnectionEstablishedCallback connectionEstablishedCallback)
        {
            base.OnConnect(connectionEstablishedCallback);
            return this;
        }

        public new ClientBootstrap OnDisconnect(ConnectionTerminatedCallback connectionTerminatedCallback)
        {
            base.OnDisconnect(connectionTerminatedCallback);
            return this;
        }

        public new ClientBootstrap OnReceive(ReceivedDataCallback receivedDataCallback)
        {
            base.OnReceive(receivedDataCallback);
            return this;
        }

        public new ClientBootstrap SetOption(string optionKey, object optionValue)
        {
            base.SetOption(optionKey, optionValue);
            return this;
        }

        public override void Validate()
        {
            if(Type == TransportType.All) throw new ArgumentException("Type must be set");
            if (Workers < 1) throw new ArgumentException("Workers must be at least 1");
        }

        protected override IConnectionFactory BuildInternal()
        {
            switch (Type)
            {
                case TransportType.Tcp:
                    return new TcpConnectionFactory(this);
                case TransportType.Udp:
                    return new UdpConnectionFactory(this);
                default:
                    throw new InvalidOperationException("This shouldn't happen");
            }
        }
    }
}