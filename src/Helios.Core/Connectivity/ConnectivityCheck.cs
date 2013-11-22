using System;
using System.Net;
using Helios.Core.Topology;

namespace Helios.Core.Connectivity
{
    /// <summary>
    /// Entity for keeping track of the connectivity state
    /// for a given node
    /// </summary>
    public sealed class ConnectivityCheck
    {
        public IPAddress NodeAddress { get; private set; }

        /// <summary>
        /// The capability we're evaluating for a given node
        /// </summary>
        public NodeCapability Capability { get; private set; }

        /// <summary>
        /// The latency in milliseconds. If the connection timed out, this value will be -1
        /// </summary>
        public int Latency { get; private set; }

        /// <summary>
        /// The UTC DateTime this node was last checked
        /// </summary>
        public DateTimeOffset TimeChecked { get; private set; }

        /// <summary>
        /// A flag indicating whether or not we have a connectivity timeout
        /// </summary>
        public bool TimedOut { get; private set; }

        private ConnectivityCheck()
        {
            TimeChecked = DateTimeOffset.UtcNow;
        }

        public static ConnectivityCheck Create(IPAddress ipAddress, NodeCapability capability, int latency,
            bool timedOut)
        {
            return new ConnectivityCheck()
            {
                NodeAddress = ipAddress,
                Capability = capability,
                Latency = latency,
                TimedOut = timedOut
            };
        }
    }
}
