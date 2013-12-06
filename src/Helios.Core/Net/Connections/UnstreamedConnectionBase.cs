using System;
using System.Net;
using System.Threading.Tasks;
using Helios.Core.Topology;

namespace Helios.Core.Net.Connections
{
    public abstract class UnstreamedConnectionBase : IConnection
    {
        protected UnstreamedConnectionBase() : this(null) { }

        protected UnstreamedConnectionBase(INode binding, TimeSpan timeout)
            : base()
        {
            Created = DateTimeOffset.UtcNow;
            Binding = binding;
            Timeout = timeout;
        }

        protected UnstreamedConnectionBase(INode binding) : this(binding, NetworkConstants.DefaultConnectivityTimeout) { }

        public DateTimeOffset Created { get; private set; }
        public INode Node { get { return Binding; } }
        public INode Binding { get; protected set; }
        public TimeSpan Timeout { get; private set; }
        public abstract TransportType Transport { get; }
        public bool WasDisposed { get; protected set; }

        public abstract bool IsOpen();
        public abstract int Available { get; }
        public abstract void Open();

        public abstract void Close();

        public abstract NetworkData Receive();

        public abstract Task<NetworkData> RecieveAsync();

        public abstract void Send(NetworkData payload);

        public abstract Task SendAsync(NetworkData payload);

        public override string ToString()
        {
            return string.Format("{0}/{1}", Binding, Created);
        }

        #region IDisposable members

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
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);

        #endregion
    }
}