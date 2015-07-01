using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Helios.Exceptions;
using Helios.Topology;
using Helios.Util;

namespace Helios.Net.Clustering
{
    /// <summary>
    /// Round-robin cluster manager - more appropriate for connection-oriented transports like TCP than it is
    /// for connectionless ones
    /// </summary>
    public class RoundRobinClusterManager : IClusterManager
    {
        protected object _lock = new object();
        protected Cluster Servers;
        protected Cluster Blacklisted;
        protected Queue<INode> ServerQueue;
        protected TimeSpan ServerRecoveryInterval;

        protected Timer ServerRecoveryTimer;

        protected readonly Action<Exception> ErrorCallback;

        public RoundRobinClusterManager() : this(new Cluster()) { }

        public RoundRobinClusterManager(Cluster servers) : this(servers, NetworkConstants.DefaultNodeRecoveryInterval, exception => { }) { }

        public RoundRobinClusterManager(Cluster servers, TimeSpan recoveryInterval, Action<Exception> errorCallback)
        {
            Servers = servers;
            ErrorCallback = errorCallback;
            Blacklisted = new Cluster();
            ServerQueue = new Queue<INode>(Servers);
            ServerRecoveryInterval = recoveryInterval;
            ServerRecoveryTimer = new Timer(ServerRecover, null, TimeSpan.Zero, ServerRecoveryInterval);
        }

        #region IClusterManager members

        public bool HasNext()
        {
            lock (_lock)
            {
                return ServerQueue.Count > 0;
            }
        }

        public INode Next()
        {
            INode server = null;

            lock (_lock)
            {
                if (ServerQueue.Count > 0)
                {
                    server = ServerQueue.Dequeue();
                    ServerQueue.Enqueue(server);
                }
            }

            return server;
        }

        public void ErrorOccurred(INode node, Exception exc = null)
        {
            lock (_lock)
            {
                Blacklisted.AddNode(node);
                RefreshServerQueue();
                exc.InitializeIfNull(new Exception("Unknown exception"));
                ErrorCallback.NotNull(a => a(new HeliosNodeException(exc, node)));
            }
        }

        public void Add(INode node)
        {
            lock (_lock)
            {
                Servers.AddNode(node);
                ServerQueue.Enqueue(node);
            }
        }

        public void Remove(INode node)
        {
            lock (_lock)
            {
                Servers.RemoveNode(node);
                Blacklisted.RemoveNode(node);
                RefreshServerQueue();
            }
        }

        public bool Exists(INode node)
        {
            return Servers.Contains(node);
        }

        #endregion

        #region Internal members

        private void ServerRecover(object state)
        {
            try
            {
                if (Blacklisted.Any())
                {
                    Cluster clonedBlackList = null;

                    lock (_lock)
                        clonedBlackList = new Cluster(Blacklisted.ToArray());

                    foreach (var node in clonedBlackList)
                    {
                        var connection = node.GetConnection();
                        try
                        {
                            connection.Open();
                            lock (_lock)
                            {
                                Servers.AddNode(node);
                                Blacklisted.RemoveNode(node);
                            }
                        }
                        catch
                        {
                        }
                        finally
                        {
                            connection.Close();
                            connection.Dispose();
                        }
                    }
                    clonedBlackList.Clear();
                }
            }
            finally
            {
            }
        }

        private void RefreshServerQueue()
        {
            ServerQueue.Clear();
            foreach (var s in Servers)
            {
                if (!Blacklisted.Contains(s))
                    ServerQueue.Enqueue(s);
            }
        }

        #endregion

        #region IEnumerable members

        public IEnumerator<INode> GetEnumerator()
        {
            return Servers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}