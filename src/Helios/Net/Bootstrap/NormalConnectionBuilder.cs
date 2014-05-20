using System;
using System.Net;
using Helios.Exceptions;
using Helios.Net.Connections;
using Helios.Ops;
using Helios.Serialization;
using Helios.Topology;
using Helios.Util;

namespace Helios.Net.Builders
{
    public class NormalConnectionBuilder : IConnectionBuilder
    {
        public NormalConnectionBuilder() : this(NetworkConstants.DefaultConnectivityTimeout) { }

        public NormalConnectionBuilder(TimeSpan timeout)
        {
            Timeout = timeout;
        }

        public TimeSpan Timeout { get; private set; }
        public IConnection BuildConnection(INode node)
        {
            switch (node.TransportType)
            {
                case TransportType.Tcp:
                    return new TcpConnection(EventLoopFactory.CreateNetworkEventLoop(), node, Timeout, Encoders.DefaultEncoder, Encoders.DefaultDecoder);
                case TransportType.Udp:
                    return new UdpConnection(EventLoopFactory.CreateNetworkEventLoop(), node, Timeout, Encoders.DefaultEncoder, Encoders.DefaultDecoder);
                default:
                    throw new HeliosConnectionException(ExceptionType.NotSupported, "No support for non-UDP / TCP connections at this time.");
            }
        }

        public IConnection BuildMulticastConnection(INode bindNode, INode multicastNode)
        {
            if(MulticastHelper.IsValidMulticastAddress(multicastNode.Host))
                return new MulticastUdpConnection(EventLoopFactory.CreateNetworkEventLoop(), bindNode, multicastNode, Encoders.DefaultEncoder, Encoders.DefaultDecoder);

            throw new HeliosConnectionException(ExceptionType.NotSupported, string.Format("{0} is an invalid multicast IP address", multicastNode.Host));
        }
    }
}