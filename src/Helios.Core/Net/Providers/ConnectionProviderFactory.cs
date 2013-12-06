using System;
using Helios.Core.Net.Builders;
using Helios.Core.Net.Clustering;
using Helios.Core.Topology;

namespace Helios.Core.Net.Providers
{
    /// <summary>
    /// Factory class for creating IConnectionProvider instances
    /// </summary>
    public static class ConnectionProviderFactory
    {
        public static IConnectionProvider GetPooledConnectionProvider(Cluster cluster, Action<Exception> errorHandler)
        {
            return new PooledKeyedConnectionProvider(ClusterManagerFactory.CreateClusterManager(cluster, errorHandler), new NormalConnectionBuilder());
        }

        public static IConnectionProvider GetPooledConnectionProvider(Cluster cluster)
        {
            return new PooledKeyedConnectionProvider(ClusterManagerFactory.CreateClusterManager(cluster), new NormalConnectionBuilder());
        }
    }
}
