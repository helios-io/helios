using System;
using System.Collections;
using System.Collections.Generic;

namespace Helios.Core.Topology
{
    /// <summary>
    /// Represents a cluster of connected nodes belonging to a single service
    /// </summary>
    public class NodeCluster : IEnumerable<INode>
    {
        /// <summary>
        /// The list of active nodes in this cluster
        /// </summary>
        public IList<INode> Nodes { get; private set; }

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
