using System;
using System.Collections.Generic;
using Helios.Net;
using Helios.Topology;
using Helios.Util.Collections;

namespace Helios.Monitoring
{
    /// <summary>
    /// Class used for tracking the heartbeat
    /// of an INode instance in a given cluster
    /// </summary>
    public class ClusterHeartBeat
    {
        /// <summary>
        /// List of all active nodes in the cluster and their health checks
        /// </summary>
        protected readonly Dictionary<INode, IFixedSizeStack<ConnectivityCheck>> NodeChecks;

        protected readonly IConnectionProvider ConnectionProvider;

        protected readonly Cluster Cluster;

        protected readonly TimeSpan PollingInterval;

        public ClusterHeartBeat(Cluster cluster, IConnectionProvider provider, TimeSpan pollingInterval)
        {
            Cluster = cluster;
            NodeChecks = new Dictionary<INode, IFixedSizeStack<ConnectivityCheck>>();
            ConnectionProvider = provider;
            PollingInterval = pollingInterval;
        }
    }
}
