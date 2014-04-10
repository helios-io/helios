using System;
using System.Net;
using Helios.Net;
using Helios.Topology;

namespace Helios.Reactor
{
    /// <summary>
    /// Reactive interface for receiving input from
    /// a network connection
    /// </summary>
    public interface IReactor : IDisposable
    {
        event ConnectionEstablishedCallback OnConnection;

        event ReceivedDataCallback OnReceive;

        event ConnectionTerminatedCallback OnDisconnection;

        event ExceptionCallback OnError;

        IConnection ConnectionAdapter { get; }

        NetworkEventLoop EventLoop { get; }

        void Send(byte[] message, INode responseAddress);

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
