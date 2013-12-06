using System.Net;

namespace Helios.Topology
{
    /// <summary>
    /// Extension methods to make it easier to work with INode implementations
    /// </summary>
    public static class NodeExtensions
    {
        public static IPEndPoint ToEndPoint(this INode node)
        {
            return new IPEndPoint(node.Host, node.Port);
        }

        public static INode ToNode(this IPEndPoint endPoint, TransportType transportType)
        {
            return new Node() {Host = endPoint.Address, Port = endPoint.Port, TransportType = transportType};
        }
    }
}
