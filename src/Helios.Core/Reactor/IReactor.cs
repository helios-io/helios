using System;
using System.Net;

namespace Helios.Core.Reactor
{
    /// <summary>
    /// Reactive interface for receiving input from
    /// a network connection
    /// </summary>
    public interface IReactor : IDisposable
    {
        bool IsActive { get; }

        bool WasDisposed { get; }

        void Start();

        void Stop();

        IPEndPoint LocalEndpoint { get; }

        /// <summary>
        /// Event that is fired each time an incoming connection is received
        /// </summary>
        event EventHandler<ReactorEventArgs> AcceptConnection;

        void Dispose(bool disposing);
    }
}
