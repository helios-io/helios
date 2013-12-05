using System;
using Helios.Core.Exceptions;
using Helios.Core.Topology;
using Helios.Core.Util;

namespace Helios.Core.Net.Providers
{
    public abstract class ConnectionProviderBase : IConnectionProvider
    {
        protected readonly IClusterManager ClusterManager;
        protected readonly IConnectionBuilder ConnectionBuilder;

        protected ConnectionProviderBase(IClusterManager clusterManager, IConnectionBuilder connectionBuilder, ConnectionProviderType providerType)
        {
            ClusterManager = clusterManager;
            ConnectionBuilder = connectionBuilder;
            Type = providerType;
        }

        #region IConnectionProvider members

        public ConnectionProviderType Type { get; private set; }

        public IClusterManager Cluster { get { return ClusterManager; } }
        public IConnection GetConnection()
        {
            var nextNode = GetNodeInternal();
            if(nextNode == null) throw new HeliosConnectionException(ExceptionType.NotSupported, "No available nodes.");
            if (HasConnectionForNode(nextNode)) return GetExistingConnectionForNode(nextNode);
            return ConnectionBuilder.BuildConnection(nextNode);
        }

        public virtual void MarkConnectionAsUnhealthy(IConnection connection, Exception exc = null)
        {
            Cluster.ErrorOccurred(connection.Node, exc);
        }

        #endregion

        protected abstract bool HasConnectionForNode(INode node);

        protected abstract IConnection GetExistingConnectionForNode(INode node);

        protected abstract INode GetNodeInternal();
    }
}