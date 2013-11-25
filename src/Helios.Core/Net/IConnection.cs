using System;
using System.Net;
using Helios.Core.Topology;

namespace Helios.Core.Net
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

        void Open();

        void Close();
    }
}
