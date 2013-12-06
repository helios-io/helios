using System;
using System.Collections;
using System.Collections.Generic;
using Helios.Exceptions;
using Helios.Topology;
using Helios.Util;

namespace Helios.Net.Clustering
{
    /// <summary>
    /// Cluster manager for managing a single node
    /// </summary>
    public class SingleNodeClusterManager : IClusterManager
    {
        protected INode Node;
        protected bool Errored;

        protected readonly Action<Exception> ErrorCallback;

        public SingleNodeClusterManager(INode node) : this(node, exception => { }) { }

        public SingleNodeClusterManager(INode node, Action<Exception> callback)
        {
            Node = node;
            ErrorCallback = callback;
        }

        #region IClusterManager members

        public bool HasNext()
        {
            return Node != null;
        }

        public INode Next()
        {
            return Node;
        }

        public void ErrorOccurred(INode node, Exception exc = null)
        {
            exc.InitializeIfNull(new Exception("Unknown exception"));
            ErrorCallback.NotNull(a => a(new HeliosNodeException(exc, node)));
        }

        public void Add(INode node)
        {
            Node = node;
        }

        public void Remove(INode node)
        {
            throw new NotSupportedException("SingleNodeClusterManager only supports a single node, which cannot be removed. Use Add method to change the underlying node.");
        }

        public bool Exists(INode node)
        {
            return Node.Equals(node);
        }

        #endregion

        #region IEnumerable members

        public IEnumerator<INode> GetEnumerator()
        {
            return new List<INode>() { Node }.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}