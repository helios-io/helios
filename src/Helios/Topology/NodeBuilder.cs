using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Helios.Topology
{
    /// <summary>
    /// Static builder class for creating INode instances
    /// </summary>
    public static class NodeBuilder
    {
        /// <summary>
        /// Creates a new INode instance
        /// </summary>
        /// <returns>A new INode instance</returns>
        public static INode BuildNode()
        {
            var n = new Node {LastPulse = DateTime.UtcNow.Ticks};
            return n;
        }

        /// <summary>
        /// Add a host to an INode instance
        /// </summary>
        /// <param name="n">A valid INode instance</param>
        /// <param name="host">A System.Net IpAddress object representing a valid host for a given service</param>
        /// <returns>A valid INode instance with the host set</returns>
        public static INode Host(this INode n, IPAddress host)
        {
            n.Host = host;
            return n;
        }

        /// <summary>
        /// Add a host to an INode instance
        /// </summary>
        /// <param name="n">A valid INode instance</param>
        /// <param name="host">A string representation of an IP address</param>
        /// <returns>A valid INode instance with the host set</returns>
        public static INode Host(this INode n, string host)
        {
            IPAddress parseIp;
            if (!IPAddress.TryParse(host, out parseIp))
            {
                var hostentry = Dns.GetHostEntry(host);
                parseIp = hostentry.AddressList.First(x => x.AddressFamily == AddressFamily.InterNetwork); //first IPv4 address
            }
           
            return Host(n, parseIp);
        }

        /// <summary>
        /// Adds a capability to a given INode instance
        /// </summary>
        /// <param name="n">A valid INode instance</param>
        /// <param name="portNum">the port number for this node</param>
        /// <returns>A valid node</returns>
        public static INode WithPort(this INode n, int portNum)
        {
            n.Port = portNum;
            return n;
        }

        /// <summary>
        /// Adds a capability to a given INode instance
        /// </summary>
        /// <param name="n">A valid INode instance</param>
        /// <param name="transportType">the type of network connection used by this node</param>
        /// <returns>A valid node</returns>
        public static INode WithTransportType(this INode n, TransportType transportType)
        {
            n.TransportType = transportType;
            return n;
        }

        /// <summary>
        /// Creates an INode instance from an IP endpoint
        /// </summary>
        /// <param name="endPoint">A System.NET.IPEndpoint argument, usually from an incoming socket connection</param>
        /// <returns>An active INode instance</returns>
        public static INode FromEndpoint(IPEndPoint endPoint)
        {
            var n = new Node {Host = endPoint.Address, Port = endPoint.Port};
            return n;
        }
    }
}
