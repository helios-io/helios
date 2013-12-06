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

        void Dispose(bool disposing);
    }
}
