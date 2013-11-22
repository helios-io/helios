using System.Collections.Generic;
using System.Net;

namespace Helios.ServiceStore
{
    public interface INode
    {
        /// <summary>
        /// The IP address of this seed
        /// </summary>
        IPAddress Host { get; set; }

        /// <summary>
        /// The name of this machine
        /// </summary>
        string MachineName { get; set; }

        /// <summary>
        /// OS name and version of this macine
        /// </summary>
        string OS { get; set; }

        /// <summary>
        /// version of the service running on this node
        /// </summary>
        string ServiceVersion { get; set; }

        /// <summary>
        /// All of the exposed capabilities of this node
        /// </summary>
        IList<NodeCapability> Capabilities { get; }

        /// <summary>
        /// A JSON blob representing arbtirary data about this node
        /// </summary>
        string CustomData { get; set; }

        /// <summary>
        /// A DateTime.Ticks representation of when we last heard from this node
        /// </summary>
        long LastPulse { get; set; }
    }
}
