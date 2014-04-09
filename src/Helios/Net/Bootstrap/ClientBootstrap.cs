using System;
using System.Net;
using Helios.Topology;

namespace Helios.Net.Bootstrap
{
    /// <summary>
    /// Client bootstrap for outbound connections
    /// </summary>
    public class ClientBootstrap : AbstractBootstrap
    {
        public ClientBootstrap() : base() { }

        public ClientBootstrap(AbstractBootstrap other) : base(other) { }

        public TransportType Type { get; private set; }

        public ClientBootstrap SetTransport(TransportType type)
        {
            Type = type;
            return this;
        }

        public new ClientBootstrap LocalAddress(INode node)
        {
            base.LocalAddress(node);
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
            if(LocalNode == null) throw new NullReferenceException("LocalNode must be set");
            if(Type == TransportType.All) throw new ArgumentException("Type must be set");
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