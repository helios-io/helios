using System;

namespace Helios.Monitoring.Timeouts
{
    /// <summary>
    /// Interface used to describe timeout policies and rules
    /// </summary>
    public interface INodeHealthPolicy
    {
        /// <summary>
        /// The permitted timeout window in seconds
        /// </summary>
        TimeSpan ConnectionTimeout { get; }

        /// <summary>
        /// The maximum number of timeouts on a node before
        /// it has to be put on the blacklist
        /// </summary>
        int MaxTimeouts { get; }

        /// <summary>
        /// <c>MaxTimeouts</c> has to occur in this period before
        /// the node is black-listed
        /// </summary>
        TimeSpan TimeoutPeriod { get; }

        /// <summary>
        /// The number of times a node can be marked as "blacked out"
        /// before it is considered dead and automatically removed
        /// from the cluster
        /// </summary>
        int MaxBlackouts { get; }
    }
}
