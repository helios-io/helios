using System;
using System.Net;

namespace Helios.Core.Connections
{
    /// <summary>
    /// Interface used to describe an open connection to a client node / capability
    /// </summary>
    public interface IConnection : IDisposable
    {
        DateTimeOffset Created { get; }

        IPAddress Host { get; }

        int Port { get; }

        int Timeout { get; }

        Topology.TransportType Transport { get; }

        bool IsOpen();

        void Open();

        void Close();
    }
}
