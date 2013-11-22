using System.Collections.Generic;

namespace Helios.ServiceStore
{
    /// <summary>
    /// Entity which represents a service that can be discovered
    /// </summary>
    public class ServiceDefinition : IServiceDefinition
    {
        /// <summary>
        /// The name of the service - used as the primary lookup criteria
        /// by nodes looking to access it
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// A central hostname for the service, if applicable. Can be null.
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// The list of available nodes in this service
        /// </summary>
        public IList<INode> Nodes { get; private set; }

        public ServiceDefinition()
        {
            Nodes = new List<INode>();
        }
    }
}
