using System;
using Helios.Core.Topology;

namespace Helios.Core.Connectivity
{
    /// <summary>
    /// Entity for keeping track of the connectivity state
    /// for a given node
    /// </summary>
    public class ConnectivityState
    {
        /// <summary>
        /// The capability we're evaluating for a given node
        /// </summary>
        public NodeCapability Capability { get; private set; }

        /// <summary>
        /// The latency in milliseconds
        /// </summary>
        public int Latency { get; private set; }

        /// <summary>
        /// The UTC DateTime this node was last checked
        /// </summary>
        public DateTimeOffset LastCheck { get; private set; }
    }
}
