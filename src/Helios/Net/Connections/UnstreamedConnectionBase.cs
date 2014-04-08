using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Helios.Exceptions;
using Helios.Topology;

namespace Helios.Net.Connections
{
    public abstract class UnstreamedConnectionBase : IConnection
    {
        protected byte[] Buffer;

        protected UnstreamedConnectionBase(int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE) : this(null, bufferSize) { }

        protected UnstreamedConnectionBase(INode binding, TimeSpan timeout, int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE)
            : base()
        {
            Created = DateTimeOffset.UtcNow;
            Binding = binding;
            Timeout = timeout;
            Buffer = new byte[bufferSize];
        }

        protected UnstreamedConnectionBase(INode binding, int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE) : this(binding, NetworkConstants.DefaultConnectivityTimeout, bufferSize) { }

        public ReceivedDataCallback Receive { get; private set; }

        public event ConnectionEstablishedCallback OnConnection;
        public event ConnectionTerminatedCallback OnDisconnection;

        public DateTimeOffset Created { get; private set; }
        public INode RemoteHost { get; protected set; }
        public INode Local { get; protected set; }
        public INode Binding { get; protected set; }
        public TimeSpan Timeout { get; private set; }
        public abstract TransportType Transport { get; }
        public abstract bool Blocking { get; set; }
        public bool WasDisposed { get; protected set; }
        public bool Receiving { get; private set; }

        public abstract bool IsOpen();
        public abstract int Available { get; }
        public abstract Task<bool> OpenAsync();
        public abstract void Open();

        public void BeginReceive(ReceivedDataCallback callback)
        {
            if (callback == null) throw new ArgumentNullException("callback");
            if(Receiving) return;

            Receive = callback;
            Receiving = true;
            BeginReceiveInternal();
        }

        protected void ReceiveCallback(IAsyncResult ar)
        {
            var socket = (Socket) ar.AsyncState;
            try
            {
                var buffSize = socket.EndReceive(ar);
                var receivedData = new byte[buffSize];
                Array.Copy(Buffer, receivedData, buffSize);

                var networkData = NetworkData.Create(NodeBuilder.FromEndpoint((IPEndPoint) socket.RemoteEndPoint),
                    receivedData, buffSize);
                RemoteHost = networkData.RemoteHost;

                //continue receiving in a loop
                if (Receiving)
                {
                    socket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, ReceiveCallback, socket);
                }
                InvokeReceiveIfNotNull(networkData);
            }
            catch (SocketException ex) //typically means that the socket is now closed
            {
                Receiving = false;
                InvokeDisconnectIfNotNull(NodeBuilder.FromEndpoint((IPEndPoint)socket.RemoteEndPoint), new HeliosConnectionException(ExceptionType.Closed, ex));
                Dispose();
            }
        }

        protected void InvokeReceiveIfNotNull(NetworkData data)
        {
            if (Receive != null)
            {
                Receive(data, this);
            }
        }

        protected void InvokeConnectIfNotNull(INode remoteHost)
        {
            if (OnConnection != null)
            {
                OnConnection(remoteHost);
            }
        }

        protected void InvokeDisconnectIfNotNull(INode remoteHost, HeliosConnectionException ex)
        {
            if (OnDisconnection != null)
            {
                OnDisconnection(remoteHost, ex);
            }
        }

        protected abstract void BeginReceiveInternal();

        public virtual void StopReceive()
        {
            Receiving = false;
        }

        public abstract void Close(Exception reason);

        public abstract void Close();

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