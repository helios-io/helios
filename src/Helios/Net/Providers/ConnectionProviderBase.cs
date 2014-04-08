using System;
using Helios.Exceptions;
using Helios.Topology;

namespace Helios.Net.Providers
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

        public void AddConnection(IConnection connection)
        {
            //Add this node to the cluster, in case it doesn't exist already
            ClusterManager.Add(connection.RemoteHost);
            AddConnectionInternal(connection);
        }
        public virtual void MarkConnectionAsUnhealthy(IConnection connection, Exception exc = null)
        {
            Cluster.ErrorOccurred(connection.RemoteHost, exc);
        }

        #endregion

        protected abstract bool HasConnectionForNode(INode node);

        protected abstract IConnection GetExistingConnectionForNode(INode node);

        protected abstract INode GetNodeInternal();

        protected abstract void AddConnectionInternal(IConnection connection);
    }
}