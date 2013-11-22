using System.Collections.Generic;

namespace Helios.ServiceStore.Definition
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
        public IList<Node> Nodes { get; private set; }

        /// <summary>
        /// A DateTime.Ticks representation of the last time we heard anything
        /// from a node in this service
        /// </summary>
        public long LastPulse { get; set; }

        public ServiceDefinition()
        {
            Nodes = new List<Node>();
        }
    }
}
