using System;
using System.Net;

namespace Helios.Topology
{
    public interface INode : ICloneable
    {
        /// <summary>
        /// The IP address of this seed
        /// </summary>
        IPAddress Host { get; set; }

        /// <summary>
        /// The port number of this node's capability
        /// </summary>
        int Port { get; set; }

        /// <summary>
        /// The connection type used by this node
        /// </summary>
        TransportType TransportType { get; set; }

        /// <summary>
        /// Converts the node to an <see cref="IPEndPoint"/>
        /// </summary>
        IPEndPoint ToEndPoint();
    }
}
