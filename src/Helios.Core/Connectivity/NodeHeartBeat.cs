using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helios.Core.Topology;

namespace Helios.Core.Connectivity
{
    /// <summary>
    /// Class used for tracking the heartbeat
    /// of an INode instance in a given cluster
    /// </summary>
    public class NodeHeartBeat
    {
        public INode Node { get; private set; }

        
    }
}
