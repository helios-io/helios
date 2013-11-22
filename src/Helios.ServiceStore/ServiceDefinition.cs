using System.Collections.Generic;
using Helios.Core.Topology;
using Helios.Core.Util;

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
        public ISet<INode> Nodes { get; private set; }

        public ServiceDefinition()
        {
            Nodes = new HashSet<INode>();
        }

        public object Clone()
        {
            var newService = new ServiceDefinition();
            newService.Nodes = new HashSet<INode>(Nodes);
            newService.HostName = (string) HostName.NotNull(x => x.Clone());
            newService.ServiceName = (string)ServiceName.NotNull(x => x.Clone());
            return newService;
        }
    }

    /// <summary>
    /// Special case pattern - represents a service definition we were unable to find
    /// in our service store
    /// </summary>
    public class MissingServiceDefinition : ServiceDefinition
    {
    }
}
