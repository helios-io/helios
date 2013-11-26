using System;
using System.Net;
using Helios.Core.Net.Transports;
using Helios.Core.Topology;

namespace Helios.Core.Net
{
    /// <summary>
    /// Interface used to describe an open connection to a client node / capability
    /// </summary>
    public interface IConnection : IStreamTransport, IDisposable
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
