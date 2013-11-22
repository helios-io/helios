using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;

namespace Helios.Core.Topology
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
            var ip = IPAddress.Parse(host);
            return Host(n, ip);
        }

        /// <summary>
        /// Add a machine name to an INode instance
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
        /// Add an OS name and verison to an INode instance
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
        /// Add the version # of the service being used to an INode instance
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
        /// Adds a capability to a given INode instance
        /// </summary>
        /// <param name="n">A valid INode instance</param>
        /// <param name="capability">A populated NodeCapability object</param>
        /// <returns>A valid node</returns>
        public static INode WithCapability(this INode n, NodeCapability capability)
        {
            n.Capabilities.Add(capability);
            return n;
        }

        /// <summary>
        /// Adds a capability to a given INode instance
        /// </summary>
        /// <param name="n">A valid INode instance</param>
        /// <param name="portNum">the port number for this capabiltiy</param>
        /// <param name="capabilityName">the name of this capabiltiy</param>
        /// <param name="transportType">the type of network connection used by this capability</param>
        /// <returns>A valid node</returns>
        public static INode WithCapability(this INode n, int portNum, string capabilityName, TransportType transportType)
        {
            n.Capabilities.Add(NodeCapability.Create(portNum, capabilityName, transportType));
            return n;
        }

        /// <summary>
        /// Add a set of capabilities to a node
        /// </summary>
        /// <param name="n">A valid INode instance</param>
        /// <param name="capabilities">A list of populated NodeCapability objects</param>
        /// <returns>A valid node</returns>
        public static INode WithCapabilities(this INode n, IEnumerable<NodeCapability> capabilities)
        {
            foreach (var nodeCapability in capabilities)
            {
                n.Capabilities.Add(nodeCapability);
            }
            return n;
        }

        /// <summary>
        /// Add a JSON blob to a node's metadata
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
        /// Add an object (which will be converted to JSON) to a node's metadata
        /// </summary>
        /// <param name="n">A valid INode instance</param>
        /// <param name="customData">An object which will be converted into a JSON string, containing custom meta data about this node</param>
        /// <returns>A valid node</returns>
        public static INode WithCustomDataObject(this INode n, object customData)
        {
            n.CustomData = JsonConvert.SerializeObject(customData);
            return n;
        }
    }
}
