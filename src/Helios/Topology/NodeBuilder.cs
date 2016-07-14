// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Helios.Topology
{
    /// <summary>
    ///     Static builder class for creating INode instances
    /// </summary>
    public static class NodeBuilder
    {
        /// <summary>
        ///     Creates a new INode instance
        /// </summary>
        /// <returns>A new INode instance</returns>
        public static INode BuildNode()
        {
            var n = new Node {LastPulse = DateTime.UtcNow.Ticks};
            return n;
        }

        /// <summary>
        ///     Add a host to an INode instance
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
        ///     Add a host to an INode instance
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
                parseIp = hostentry.AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
                    // first IPv4 address
                if (parseIp == null)
                {
                    parseIp = hostentry.AddressList.First(x => x.AddressFamily == AddressFamily.InterNetworkV6);
                        // first IPv6 address
                }
            }

            return Host(n, parseIp);
        }

        /// <summary>
        ///     Add a machine name to an INode instance
        /// </summary>
        /// <param name="n">A valid INode instance</param>
        /// <param name="machineName">the name of this machine</param>
        /// <returns>A valid INode instance with the machine name set</returns>
        public static INode MachineName(this INode n, string machineName)
        {
            n.MachineName = machineName;
            return n;
        }

        /// <summary>
        ///     Add an OS name and version to an INode instance
        /// </summary>
        /// <param name="n">A valid INode instance</param>
        /// <param name="osName">The name and version of the host operating system</param>
        /// <returns>A valid INode instance with the OS name and version set</returns>
        public static INode OperatingSystem(this INode n, string osName)
        {
            n.OS = osName;
            return n;
        }

        /// <summary>
        ///     Add the version # of the service being used to an INode instance
        /// </summary>
        /// <param name="n">A valid INode instance</param>
        /// <param name="serviceVersion">The version # of the service this node belongs to</param>
        /// <returns>A valid INode instance with the service version # set</returns>
        public static INode WithVersion(this INode n, string serviceVersion)
        {
            n.ServiceVersion = serviceVersion;
            return n;
        }

        /// <summary>
        ///     Adds a capability to a given INode instance
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
        ///     Adds a capability to a given INode instance
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
        ///     Add a JSON blob to a node's metadata
        /// </summary>
        /// <param name="n">A valid INode instance</param>
        /// <param name="customData">A JSON-string representing a blob of custom data about this node</param>
        /// <returns>A valid node</returns>
        public static INode WithCustomData(this INode n, string customData)
        {
            n.CustomData = customData;
            return n;
        }

        /// <summary>
        ///     Creates an INode instance from an IP endpoint
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