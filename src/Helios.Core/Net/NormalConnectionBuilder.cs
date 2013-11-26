using System;
using System.Net;
using Helios.Core.Exceptions;
using Helios.Core.Net.Connections;
using Helios.Core.Topology;

namespace Helios.Core.Net
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
                    return new TcpConnection(node, Timeout);
                default:
                    throw new HeliosConnectionException(ExceptionType.NotSupported, "No support for UDP connections at this time.");
            }
        }
    }
}