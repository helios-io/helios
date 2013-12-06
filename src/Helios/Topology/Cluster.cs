using System.Collections;
using System.Collections.Generic;

namespace Helios.Topology
{
    /// <summary>
    /// Represents a cluster of connected nodes belonging to a single service
    /// </summary>
    public class Cluster : IEnumerable<INode>
    {
        public Cluster(IEnumerable<INode> nodes)
        {
            Nodes = new HashSet<INode>(nodes);
        }

        public Cluster()
            : this(new List<INode>())
        {
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

        public void Clear()
        {
            Nodes.Clear();
        }

        public int Count
        {
            get { return Nodes.Count; }
        }
    }
}
