using System;
using Helios.Topology;

namespace Helios.Net.Clustering
{
    /// <summary>
    /// Static factory class for creating IClusterManager instances
    /// </summary>
    public static class ClusterManagerFactory
    {
        public static IClusterManager CreateClusterManager(Cluster cluster)
        {
            return CreateClusterManager(cluster, NetworkConstants.DefaultNodeRecoveryInterval);
        }

        public static IClusterManager CreateClusterManager(Cluster cluster, TimeSpan recoveryInterval)
        {
            return CreateClusterManager(cluster, recoveryInterval, exception => { });
        }

        public static IClusterManager CreateClusterManager(Cluster cluster,
            Action<Exception> errorCallback)
        {
            return CreateClusterManager(cluster, NetworkConstants.DefaultNodeRecoveryInterval, errorCallback);
        }

        public static IClusterManager CreateClusterManager(Cluster cluster, TimeSpan recoveryInterval,
            Action<Exception> errorCallback)
        {
            return new RoundRobinClusterManager(cluster, recoveryInterval, errorCallback);
        }

        public static IClusterManager CreateClusterManager(INode node)
        {
            return CreateClusterManager(node, exception => { });
        }

        public static IClusterManager CreateClusterManager(INode node, Action<Exception> errorCallback)
        {
            return new SingleNodeClusterManager(node, errorCallback);
        }
    }
}
