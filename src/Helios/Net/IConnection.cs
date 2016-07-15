// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Helios.Buffers;
using Helios.Channels;
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
    [Obsolete("Use IChannel instead")]
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

        /// <summary>
        /// Messages that have not yet been delivered to their intended destination
        /// </summary>
        int MessagesInSendQueue { get; }

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
        /// <param name="data">A NetworkData data</param>
        void Send(NetworkData data);

        /// <summary>
        /// Send a data of data from the specified byte array to the 
        /// <see cref="INode"/> specified. 
        /// 
        /// <see cref="destination"/> is not used for TCP and other connection-oriented 
        /// protocols, where the recipient is well-known. It is
        /// 
        /// <see cref="destination"/> is REQUIRED, however, for connectionless protocols like UDP.
        /// 
        /// Not sure what type of connection you're using? Include <see cref="destination"/> by default.
        /// 
        /// All sends are done asynchronously by default.
        /// </summary>
        /// <param name="buffer">The byte array to send over the network</param>
        /// <param name="index">Send bytes starting at this index in the array</param>
        /// <param name="length">Send this many bytes from the array starting at <see cref="index"/>.</param>
        /// <param name="destination">The network address where this information will be sent.</param>
        void Send(byte[] buffer, int index, int length, INode destination);
    }

    /// <summary>
    /// The state object used to process data on an <see cref="IConnection"/> instance
    /// </summary>
    public class NetworkState
    {
        public NetworkState(Socket socket, INode remoteHost, IByteBuf buffer, int rawBufferLength)
        {
            Buffer = buffer;
            RemoteHost = remoteHost;
            Socket = socket;
            RawBuffer = new byte[rawBufferLength];
        }

        /// <summary>
        /// The low-level socket object
        /// </summary>
        public Socket Socket { get; private set; }

        /// <summary>
        /// The remote host on the other end of the connection
        /// </summary>
        public INode RemoteHost { get; set; }

        /// <summary>
        /// The receive buffer used for processing data from this connection
        /// </summary>
        public IByteBuf Buffer { get; private set; }

        /// <summary>
        /// Raw buffer used for receiving data directly from the network
        /// </summary>
        public byte[] RawBuffer { get; private set; }
    }
}