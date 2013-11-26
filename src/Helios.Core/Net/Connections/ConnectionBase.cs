using System;
using System.Net;
using Helios.Core.Monitoring;
using Helios.Core.Net.Transports;
using Helios.Core.Topology;

namespace Helios.Core.Net.Connections
{
    public abstract class ConnectionBase : StreamTransport, IConnection
    {
        protected ConnectionBase(INode node, TimeSpan timeout) : base()
        {
            Created = DateTimeOffset.UtcNow;
            Node = node;
            Timeout = timeout;
        }

        protected ConnectionBase(INode node) : this(node, NetworkMonitoringConstants.DefaultConnectivityTimeout) { }

        public DateTimeOffset Created { get; private set; }
        public INode Node { get; private set; }

        public TimeSpan Timeout { get; private set; }
        public abstract TransportType Transport { get; }
        public bool WasDisposed { get; protected set; }

        public abstract bool IsOpen();

        public override bool Peek()
        {
            return IsOpen();
        }

        public abstract void Open();

        public abstract void Close();

        public override string ToString()
        {
            return string.Format("{0}/{1}", Node, Created);
        }

        #region IDisposable Members

        /// <summary>
        /// Prevents disposed connections from being re-used again
        /// </summary>
        protected void CheckWasDisposed()
        {
            if (WasDisposed)
                throw new ObjectDisposedException("connection has been disposed of");
        }

        public virtual void Dispose()
        {
            DisposeStreams(true);
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="IConnection"/> is reclaimed by garbage collection.
        /// </summary>
        ~ConnectionBase()
        {
            Dispose(true);
        }

        #endregion
    }
}