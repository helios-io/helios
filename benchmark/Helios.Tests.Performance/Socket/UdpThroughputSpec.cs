using System.Net;

namespace Helios.Tests.Performance.Socket
{
    public class UdpThroughputSpec : SocketThroughputSpec
    {
        public override TransportType TransportType
        {
            get { return TransportType.Tcp; }
        }
    }
}