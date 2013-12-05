using Helios.Core.Topology;

namespace Helios.Core.Net.Providers
{
    /// <summary>
    /// Connection provider implementation that uses INode as the key type
    /// </summary>
    public class PooledKeyedConnectionProvider : KeyedConnectionProviderBase<INode>
    {
        public PooledKeyedConnectionProvider(IClusterManager clusterManager, IConnectionBuilder connectionBuilder) : base(clusterManager, connectionBuilder)
        {
        }

        protected override bool HasConnectionForNode(INode node)
        {
            return HasConnection(node);
        }

        protected override IConnection GetExistingConnectionForNode(INode node)
        {
            return GetConnection(node);
        }

        protected override INode GetNodeInternal()
        {
            return Cluster.Next();
        }
    }
}