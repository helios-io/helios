using System;
using System.Net;
using System.Threading.Tasks;
using Helios.Topology;

namespace Helios.Net
{
    /// <summary>
    /// Typed delegate used for handling received data
    /// </summary>
    /// <param name="incomingData">a <see cref="NetworkData"/> instance that contains information that's arrived over the network</param>
    public delegate void ReceivedDataCallback(NetworkData incomingData, IConnection responseChannel);

    /// <summary>
    /// Delegate used when a new connection is successfully established
    /// </summary>
    /// <param name="remoteAddress">The remote endpoint on the other end of this connection</param>
    public delegate void ConnectionEstablishedCallback(INode remoteAddress);

    /// <summary>
    /// Delegate used when a connection is closed
    /// </summary>
    /// <param name="remoteAddress">The remote endpoint that terminated the connection</param>
    public delegate void ConnectionTerminatedCallback(INode remoteAddress, HeliosException reason);

    /// <summary>
    /// Interface used to describe an open connection to a client node / capability
    /// </summary>
    public interface IConnection : IDisposable
    {
        ReceivedDataCallback Receive { get; }

        event ConnectionEstablishedCallback OnConnection;

        event ConnectionTerminatedCallback OnDisconnection;

        DateTimeOffset Created { get; }

        INode Node { get; }

        TimeSpan Timeout { get; }

        TransportType Transport { get; }

        bool WasDisposed { get; }

        bool Receiving { get; }

        bool IsOpen();

        /// <summary>
        /// The total number of bytes written the network that are available to be read
        /// </summary>
        /// <returns>the number of bytes received from the network that are available to be read</returns>
        int Available { get; }

        Task<bool> OpenAsync();

        void Open();

        /// <summary>
        /// Call this method to begin receiving data on this connection
        /// </summary>
        /// <param name="callback">A callback for when data is received</param>
        void BeginReceive(ReceivedDataCallback callback);

        /// <summary>
        /// Stop receiving messages, but keep the connection open
        /// </summary>
        void StopReceive();

        void Close();

        /// <summary>
        /// Send data to a remote host
        /// </summary>
        /// <param name="payload">A NetworkData payload</param>
        void Send(NetworkData payload);

        Task SendAsync(NetworkData payload);
    }
}
