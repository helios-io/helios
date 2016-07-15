// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Net;
using Helios.Buffers;
using Helios.Channels;
using Helios.Net;
using Helios.Serialization;
using Helios.Topology;

namespace Helios.Reactor
{
    /// <summary>
    /// Reactive interface for receiving input from
    /// a network connection
    /// </summary>
    [Obsolete("Use IChannel instead")]
    public interface IReactor : IDisposable
    {
        event ConnectionEstablishedCallback OnConnection;

        event ReceivedDataCallback OnReceive;

        event ConnectionTerminatedCallback OnDisconnection;

        event ExceptionCallback OnError;

        IMessageEncoder Encoder { get; }
        IMessageDecoder Decoder { get; }
        IByteBufAllocator Allocator { get; }

        IConnection ConnectionAdapter { get; }

        NetworkEventLoop EventLoop { get; }

        void Send(NetworkData data);

        /// <summary>
        /// Send a payload of data from the specified byte array to the 
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

        /// <summary>
        /// The backlog of pending connections allowed for the underlying transport
        /// </summary>
        int Backlog { get; set; }

        bool IsActive { get; }

        bool WasDisposed { get; }

        void Configure(IConnectionConfig config);

        void Start();

        void Stop();

        IPEndPoint LocalEndpoint { get; }

        TransportType Transport { get; }

        void Dispose(bool disposing);
    }
}