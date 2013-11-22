using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.ServiceStore.Seeds
{
    /// <summary>
    /// Describes some of the capabilities of the node
    /// </summary>
    public class NodeCapability
    {
        /// <summary>
        /// The port number of this node's capability
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// The name of this capability. I.E. "Thrift", "RDP", "SSH", "FTP", etc...
        /// </summary>
        public string Capability { get; set; }
    }
}
