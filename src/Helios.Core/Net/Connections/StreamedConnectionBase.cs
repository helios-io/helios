using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Helios.Core.Net.Transports;
using Helios.Core.Ops;
using Helios.Core.Topology;

namespace Helios.Core.Net.Connections
{
    /// <summary>
    /// Base class for streamed connections, like TCP
    /// </summary>
    public abstract class StreamedConnectionBase : StreamTransport, IConnection
    {
        protected StreamedConnectionBase() : this(null) { }

        protected StreamedConnectionBase(INode node, TimeSpan timeout) : base()
        {
            Created = DateTimeOffset.UtcNow;
            Node = node;
            Timeout = timeout;
        }

        protected StreamedConnectionBase(INode node) : this(node, NetworkConstants.DefaultConnectivityTimeout) { }

        public DateTimeOffset Created { get; private set; }
        public INode Node { get; protected set; }

        public TimeSpan Timeout { get; private set; }
        public abstract TransportType Transport { get; }
        public bool WasDisposed { get; protected set; }

        public abstract bool IsOpen();
        public abstract int Available { get; }

        public override bool Peek()
        {
            return IsOpen();
        }

        public abstract void Open();

        public abstract void Close();
        public NetworkData Receive()
        {
            var memoryStream = new MemoryStream(1024);
            var buffer = Read(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
            return NetworkData.Create(Node, memoryStream.GetBuffer(), (int)memoryStream.Capacity);
        }

        public async Task<NetworkData> RecieveAsync()
        {
            var memoryStream = new MemoryStream(1024);
            var buffer = await ReadAsync(memoryStream.GetBuffer(), 0, (int)memoryStream.Capacity);
            return NetworkData.Create(Node, memoryStream.GetBuffer(),(int)memoryStream.Length);
        }

        public void Send(NetworkData payload)
        {
            Write(payload.Data, 0, payload.Bytes);
        }

        public async Task SendAsync(NetworkData payload)
        {
            await WriteAsync(payload.Data, 0, payload.Bytes);
        }

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
        ~StreamedConnectionBase()
        {
            Dispose(true);
        }

        #endregion
    }
}