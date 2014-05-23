using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Helios.Buffers;
using Helios.Exceptions;
using Helios.Ops;
using Helios.Serialization;
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
    public delegate void ConnectionEstablishedCallback(INode remoteAddress, IConnection responseChannel);

    /// <summary>
    /// Delegate used when a connection is closed
    /// </summary>
    /// <param name="closedChannel">The channel that is now closed</param>
    public delegate void ConnectionTerminatedCallback(HeliosConnectionException reason, IConnection closedChannel);

    /// <summary>
    /// Delegate used when an unexpected error occurs
    /// </summary>
    /// <param name="connection">The connection object responsible for propagating this error</param>
    /// <param name="ex">The exception that occurred</param>
    public delegate void ExceptionCallback(Exception ex, IConnection connection);

    /// <summary>
    /// Interface used to describe an open connection to a client node / capability
    /// </summary>
    public interface IConnection : IDisposable
    {
        event ReceivedDataCallback Receive;

        event ConnectionEstablishedCallback OnConnection;

        event ConnectionTerminatedCallback OnDisconnection;

        event ExceptionCallback OnError;

        IEventLoop EventLoop { get; }

        IMessageEncoder Encoder { get; }
        IMessageDecoder Decoder { get; }

        /// <summary>
        /// Used to allocate reusable buffers for network I/O
        /// </summary>
        IByteBufAllocator Allocator { get; }

        DateTimeOffset Created { get; }

        INode RemoteHost { get; }

        INode Local { get; }

        TimeSpan Timeout { get; }

        TransportType Transport { get; }

        /// <summary>
        /// Determines if the underlying connection uses a blocking transport or not
        /// </summary>
        bool Blocking { get; set; }

        bool WasDisposed { get; }

        bool Receiving { get; }

        bool IsOpen();

        /// <summary>
        /// The total number of bytes written the network that are available to be read
        /// </summary>
        /// <returns>the number of bytes received from the network that are available to be read</returns>
        int Available { get; }

        Task<bool> OpenAsync();

        /// <summary>
        /// Configures this transport using the provided option
        /// </summary>
        /// <param name="config">a <see cref="IConnectionConfig"/> instance with the appropriate configuration options</param>
        void Configure(IConnectionConfig config);

        void Open();

        /// <summary>
        /// Call this method to begin receiving data on this connection.
        /// 
        /// Assumes that <see cref="Receive"/> has already been set.
        /// </summary>
        void BeginReceive();

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

    /// <summary>
    /// The state object used to process data on an <see cref="IConnection"/> instance
    /// </summary>
    public class ReceiveState
    {
        public ReceiveState(Socket socket, INode remoteHost, IByteBuf buffer)
        {
            Buffer = buffer;
            RemoteHost = remoteHost;
            Socket = socket;
        }

        /// <summary>
        /// The low-level socket object
        /// </summary>
        public Socket Socket { get; private set; }

        /// <summary>
        /// The remote host on the other end ofthe connection
        /// </summary>
        public INode RemoteHost { get; private set; }

        /// <summary>
        /// The receive buffer used for processing data from this connection
        /// </summary>
        public IByteBuf Buffer { get; private set; }
    }
}
