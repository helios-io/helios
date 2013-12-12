using System;
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

        public static Uri ToUri(this INode node)
        {
            return new NodeUri(node);
        }

        public static INode ToNode(this Uri uri)
        {
            return NodeUri.GetNodeFromUri(uri);
        }

        public static bool IsEmpty(this INode node)
        {
            return node is EmptyNode;
        }
    }
}
