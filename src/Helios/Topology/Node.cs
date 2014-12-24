using System;
using System.Net;
using Helios.Net;
using Helios.Util;

namespace Helios.Topology
{
    /// <summary>
    /// Node belonging to a service
    /// </summary>
    public class Node : INode
    {
        public Node()
        {
            TransportType = TransportType.Tcp;
        }

        /// <summary>
        /// The IP address of this seed
        /// </summary>
        public IPAddress Host { get; set; }

        /// <summary>
        /// The port number of this node
        /// </summary>
        public int Port { get; set; }


        public TransportType TransportType { get; set; }

        /// <summary>
        /// A DateTime.Ticks representation of when we last heard from this node
        /// </summary>
        public long LastPulse { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            return GetHashCode() == obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 17;
                hashCode += 23 * Host.GetHashCode();
                hashCode += 23* Port.GetHashCode();
                return hashCode;
            }
           
        }

        private IPEndPoint _endPoint;
        public IPEndPoint ToEndPoint()
        {
            return _endPoint ?? (_endPoint = new IPEndPoint(Host, Port));
        }

        public object Clone()
        {
            return new Node()
            {
                Host = new IPAddress(Host.GetAddressBytes()),
                Port = Port,
                TransportType = TransportType
            };
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Host, Port);
        }

        #region Static methods

        public static INode Loopback(int port = NetworkConstants.InMemoryPort)
        {
            return NodeBuilder.BuildNode().Host(IPAddress.Loopback).WithPort(port);
        }

        private static INode empty = new EmptyNode();
        public static INode Empty()
        {
            return empty;
        }

        public static INode Any(int port = 0)
        {
            return NodeBuilder.BuildNode().Host(IPAddress.Any).WithPort(port);
        }

#if !NET35 && !NET40
        public static INode FromString(string nodeUri)
        {
            var uri = new Uri(nodeUri);
            return uri.ToNode();
        }
#endif

        #endregion
    }
}
