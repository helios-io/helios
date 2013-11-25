using System.Collections;
using System.Collections.Generic;

namespace Helios.Core.Topology
{
    /// <summary>
    /// Represents a cluster of connected nodes belonging to a single service
    /// </summary>
    public class Cluster : IEnumerable<INode>
    {
        public Cluster()
        {
            Nodes = new HashSet<INode>();
        }

        /// <summary>
        /// All nodes in this cluster
        /// </summary>
        protected HashSet<INode> Nodes { get; private set; }

        public virtual void AddNode(INode node)
        {
            Nodes.Add(node);
        }

        public virtual void RemoveNode(INode node)
        {
            Nodes.Remove(node);
        }

        #region IEnumerable<INode> members

        public IEnumerator<INode> GetEnumerator()
        {
            return Nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
