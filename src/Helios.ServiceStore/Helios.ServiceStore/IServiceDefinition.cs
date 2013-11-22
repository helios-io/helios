using System;
using System.Collections.Generic;

namespace Helios.ServiceStore
{
    public interface IServiceDefinition : ICloneable
    {
        /// <summary>
        /// The name of the service - used as the primary lookup criteria
        /// by nodes looking to access it
        /// </summary>
        string ServiceName { get; set; }

        /// <summary>
        /// A central hostname for the service, if applicable. Can be null.
        /// </summary>
        string HostName { get; set; }

        /// <summary>
        /// The list of available nodes in this service
        /// </summary>
        ISet<INode> Nodes { get; }
    }
}