using System.Net;

namespace Helios.ServiceStore.Persistence
{
    /// <summary>
    /// A repository interface for keeping track of discoverable services
    /// </summary>
    public interface IServiceStore
    {
        /// <summary>
        /// Creates or updates a new service in the repository
        /// </summary>
        /// <param name="serviceDefinition">An IServiceDefinition with one or more nodes</param>
        /// <returns>An operation result with the status of the save</returns>
        OperationResult SaveService(IServiceDefinition serviceDefinition);

        /// <summary>
        /// Save a node's details for an existing service
        /// </summary>
        /// <param name="serviceName">the name of the service this node belongs to</param>
        /// <param name="node">the node details</param>
        /// <returns>An operation result</returns>
        OperationResult SaveNode(string serviceName, INode node);

        /// <summary>
        /// Remove a node from an existing service
        /// </summary>
        /// <param name="serviceName">the name of the service this node belongs to</param>
        /// <param name="nodeAddress">the ip address of the failed node</param>
        /// <returns>An operation result</returns>
        OperationResult RemoveNode(string serviceName, IPAddress nodeAddress);

        /// <summary>
        /// Get all of the details about an existing service if available
        /// </summary>
        /// <param name="serviceName">The name of the service</param>
        /// <returns>An OperationResult with the revelent ServiceDefinition details</returns>
        OperationResult<ServiceDefinition> GetService(string serviceName);

        /// <summary>
        /// Check to see if an existing service is registered with this repository
        /// </summary>
        /// <param name="serviceName">The name of the service to check</param>
        /// <returns>An OperationResult with a payload of true if exists, false otherwise</returns>
        OperationResult<bool> ServiceExists(string serviceName);

        /// <summary>
        /// Delete's a service from the repository altogether
        /// </summary>
        /// <param name="serviceName">The name of the service to delete</param>
        /// <returns>An OperationResult</returns>
        OperationResult DeleteService(string serviceName);
    }
}
