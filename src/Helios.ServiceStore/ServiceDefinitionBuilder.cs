using System.Collections.Generic;

namespace Helios.ServiceStore
{
    /// <summary>
    /// Builder class for creating service definitions
    /// </summary>
    public static class ServiceDefinitionBuilder
    {
        /// <summary>
        /// Creates a new service with a given name
        /// </summary>
        /// <param name="serviceName">The name of this service</param>
        /// <returns>An IServiceDefinition instance</returns>
        public static IServiceDefinition CreateServiceDefinition(string serviceName)
        {
            return new ServiceDefinition() {ServiceName = serviceName};
        }

        public static IServiceDefinition WithHostName(this IServiceDefinition s, string hostName)
        {
            s.HostName = hostName;
            return s;
        }

        public static IServiceDefinition WithSeed(this IServiceDefinition s, INode seed)
        {
            s.Nodes.Add(seed);
            return s;
        }

        public static IServiceDefinition WithSeeds(this IServiceDefinition s, IEnumerable<INode> seeds)
        {
            foreach (var n in seeds)
            {
                s.Nodes.Add(n);
            }
            return s;
        }
    }
}
