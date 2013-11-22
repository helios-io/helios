using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Helios.Core.Ops;
using Helios.Core.Topology;

namespace Helios.ServiceStore.Persistence.InMemory
{
    public class InMemoryServiceStore : IServiceStore
    {
        /// <summary>
        /// Maintain a single instance of the in-memory store across all instances of this class,
        /// </summary>
        protected static readonly IDictionary<string, IServiceDefinition> Services = new ConcurrentDictionary<string, IServiceDefinition>();

        public OperationResult SaveService(IServiceDefinition serviceDefinition)
        {
            if (!Services.ContainsKey(serviceDefinition.ServiceName))
            {
                Services.Add(serviceDefinition.ServiceName, serviceDefinition);
            }
            else
            {
                Services[serviceDefinition.ServiceName] = serviceDefinition;
            }

            return OperationResult.Create(200, true, "updated");
        }

        public OperationResult SaveNode(string serviceName, INode node)
        {
            if (ServiceExists(serviceName).Payload)
            {
                var service = Services[serviceName];
                service.Nodes.Add(node); //hashset eliminates duplicates

                return OperationResult.Create(200, true, "saved");
            }

            return OperationResult.Create(404, false, String.Format("unable to find service with name {0}", serviceName));
        }

        public OperationResult RemoveNode(string serviceName, IPAddress nodeAddress)
        {
            if (ServiceExists(serviceName).Payload)
            {
                var service = Services[serviceName];
                if(!service.Nodes.Any(x => x.Host.ToString() == nodeAddress.ToString()))
                    return OperationResult.Create(404, false, String.Format("unable to find node for service {0} with ip {1}", serviceName, nodeAddress));

                var node = service.Nodes.First(x => x.Host.ToString() == nodeAddress.ToString());
                service.Nodes.Remove(node);

                return OperationResult.Create(204, true, "saved");
            }

            return OperationResult.Create(404, false, String.Format("unable to find service with name {0}", serviceName));
        }

        public OperationResult<ServiceDefinition> GetService(string serviceName)
        {
            if (ServiceExists(serviceName).Payload)
            {
                return OperationResult.Create(200, true, "ok", (ServiceDefinition)Services[serviceName].Clone());
            }

            return OperationResult.Create(404, false, String.Format("unable to find service with name {0}", serviceName), (ServiceDefinition)(new MissingServiceDefinition()));
        }

        public OperationResult<bool> ServiceExists(string serviceName)
        {
            return OperationResult.Create(200, true, "ok", Services.ContainsKey(serviceName));
        }

        public OperationResult DeleteService(string serviceName)
        {
            if (ServiceExists(serviceName).Payload)
            {
                var removed = Services.Remove(serviceName);
                return OperationResult.Create(removed ? 204 : 500, removed, string.Format("removed message {0}", serviceName));
            }

            return OperationResult.Create(404, false, String.Format("unable to find service with name {0}", serviceName), (ServiceDefinition)(new MissingServiceDefinition()));
        }
    }
}