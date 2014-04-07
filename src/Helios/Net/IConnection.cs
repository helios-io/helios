using System;
using System.Net;
using System.Threading.Tasks;
using Helios.Topology;

namespace Helios.Net
{
    /// <summary>
    /// Interface used to describe an open connection to a client node / capability
    /// </summary>
    public interface IConnection : IDisposable
    {
        DateTimeOffset Created { get; }

        INode Node { get; }

        TimeSpan Timeout { get; }

        TransportType Transport { get; }

        bool WasDisposed { get; }

        bool IsOpen();


        /// <summary>
        /// The total number of bytes written the network that are available to be read
        /// </summary>
        /// <returns>the number of bytes received from the network that are available to be read</returns>
        int Available { get; }

        Task<bool> OpenAsync();

        void Open();

        void Close();

        /// <summary>
        /// Recieve data from a remote host
        /// </summary>
        /// <returns>A NetworkData payload</returns>
        NetworkData Receive();

        Task<NetworkData> RecieveAsync();

        /// <summary>
        /// Send data to a remote host
        /// </summary>
        /// <param name="payload">A NetworkData payload</param>
        void Send(NetworkData payload);

        Task SendAsync(NetworkData payload);
    }
}
