using System;
using Helios.Net.Providers;

namespace Helios.Net
{
    /// <summary>
    /// Interface used to provide data
    /// </summary>
    public interface IConnectionProvider
    {
        IClusterManager Cluster { get; }

        IConnection GetConnection();

        ConnectionProviderType Type { get; }

        /// <summary>
        /// Report an error with the current connection
        /// </summary>
        /// <param name="connection">An IConnection object</param>
        /// <param name="exc">OPTIONAL. An exception that occurred on the network</param>
        void MarkConnectionAsUnhealthy(IConnection connection, Exception exc = null);
    }
}
