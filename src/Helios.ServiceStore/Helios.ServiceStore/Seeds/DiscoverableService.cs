using System.Collections.Generic;

namespace Helios.ServiceStore.Seeds
{
    /// <summary>
    /// Entity which represents a service that can be discovered
    /// </summary>
    public class DiscoverableService
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
        public IList<SeedNode> Nodes { get; private set; }

        /// <summary>
        /// A DateTime.Ticks representation of the last time we heard anything
        /// from a node in this service
        /// </summary>
        public long LastPulse { get; set; }

        public DiscoverableService()
        {
            Nodes = new List<SeedNode>();
        }
    }
}
