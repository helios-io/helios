using System.Collections.Generic;
using System.Net;

namespace Helios.Core.Topology
{
    /// <summary>
    /// Node belonging to a service
    /// </summary>
    public class Node : INode
    {
        /// <summary>
        /// The IP address of this seed
        /// </summary>
        public IPAddress Host { get; set; }

        /// <summary>
        /// The name of this machine
        /// </summary>
        public string MachineName { get; set; }

        public string OS { get; set; }
        public string ServiceVersion { get; set; }

        /// <summary>
        /// All of the exposed capabilities of this node
        /// </summary>
        public IList<NodeCapability> Capabilities { get; private set; }

        /// <summary>
        /// A JSON blob representing arbtirary data about this node
        /// </summary>
        public string CustomData { get; set; }

        /// <summary>
        /// A DateTime.Ticks representation of when we last heard from this node
        /// </summary>
        public long LastPulse { get; set; }

        public override int GetHashCode()
        {
            var hashCode = Host.GetHashCode();
            return hashCode;
        }

        public Node()
        {
            Capabilities = new List<NodeCapability>();
        }
    }
}
