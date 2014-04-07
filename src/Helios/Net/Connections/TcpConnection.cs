using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Helios.Exceptions;
using Helios.Topology;

namespace Helios.Net.Connections
{
    public class TcpConnection : UnstreamedConnectionBase
    {
        protected TcpClient _client;

        public TcpConnection(INode node, TimeSpan timeout, int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE)
            : base(node, timeout, bufferSize)
        {
            InitClient();
        }

        public TcpConnection(INode node, int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE)
            : base(node, bufferSize)
        {
            InitClient();
        }

        public TcpConnection(TcpClient client, int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE)
        {
            InitClient(client);
        }

        public override TransportType Transport { get { return TransportType.Tcp; } }

        public bool NoDelay
        {
            get { return _client.NoDelay; }
            set { _client.NoDelay = value; }
        }

        public int Linger
        {
            get { return _client.LingerState.Enabled ? _client.LingerState.LingerTime : 0; }
            set { _client.LingerState = new LingerOption(true, value); }
        }

        public int SendBufferSize
        {
            get { return _client.SendBufferSize; }
            set { _client.SendBufferSize = value; }
        }

        public int ReceiveBufferSize
        {
            get { return _client.ReceiveBufferSize; }
            set { _client.ReceiveBufferSize = value; }
        }

        public bool ReuseAddress
        {
            get { return !_client.ExclusiveAddressUse; }
            set { _client.ExclusiveAddressUse = !value; }
        }

        public bool KeepAlive
        {
            get { return ((int)_client.Client.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive) == 1); }
            set { _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, value ? 1 : 0); }
        }

        public override bool IsOpen()
        {
            if (_client == null) return false;
            return _client.Connected;
        }

        public override int Available
        {
            get
            {
                if (!IsOpen()) return 0;
                return _client.Available;
            }
        }

        public override async Task<bool> OpenAsync()
        {
            CheckWasDisposed();

            if (IsOpen()) return await Task.Run(() => true);

            if (Node == null || Node.Host == null)
            {
                throw new HeliosConnectionException(ExceptionType.NotOpen, "Cannot open a connection to a null Node or null Node.Host");
            }

            if (Node.Port <= 0)
            {
                throw new HeliosConnectionException(ExceptionType.NotOpen, "Cannot open a connection to an invalid port");
            }

            if (_client == null)
                InitClient();

            return await _client.ConnectAsync(Node.Host, Node.Port)
                .ContinueWith(x =>
                {
                    var result = x.IsCompleted && !x.IsFaulted && !x.IsCanceled;
                    if (result)
                    {
                        InvokeConnectIfNotNull(Node);
                    }
                    return result;
                },
                    TaskContinuationOptions.AttachedToParent | TaskContinuationOptions.ExecuteSynchronously);
        }

        public override void Open()
        {
            CheckWasDisposed();

            if (IsOpen()) return;

            if (Node == null || Node.Host == null)
            {
                throw new HeliosConnectionException(ExceptionType.NotOpen, "Cannot open a connection to a null Node or null Node.Host");
            }

            if (Node.Port <= 0)
            {
                throw new HeliosConnectionException(ExceptionType.NotOpen, "Cannot open a connection to an invalid port");
            }

            if (_client == null)
                InitClient();

            var ar = _client.BeginConnect(Node.Host, Node.Port, null, null);
            if (ar.AsyncWaitHandle.WaitOne(Timeout))
            {
                try
                {
                    _client.EndConnect(ar);
                }
                catch (SocketException ex)
                {
                    throw new HeliosConnectionException(ExceptionType.NotOpen, ex);
                }
            }
            else
            {
                _client.Close();
                throw new HeliosConnectionException(ExceptionType.TimedOut, "Timed out on connect");
            }
            InvokeConnectIfNotNull(Node);
        }

        protected override void BeginReceiveInternal()
        {
            _client.Client.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, ReceiveCallback, _client.Client);
        }

        public override void Close()
        {
            CheckWasDisposed();

            if (!IsOpen())
                return;

            _client.Close();
            InvokeDisconnectIfNotNull(Node, new HeliosConnectionException(ExceptionType.Closed));
            _client = null;
        }

        public override void Send(NetworkData payload)
        {
            try
            {
                _client.Client.Send(payload.Buffer, payload.Length, SocketFlags.None);
            }
            catch (SocketException ex) //socket probably closed
            {
                Close();
            }
        }

        public override async Task SendAsync(NetworkData payload)
        {
            await Task.Run(() => Send(payload));
        }

        #region IDisposable Members

        protected override void Dispose(bool disposing)
        {
            if (!WasDisposed)
            {
                if (disposing)
                {
                    if (_client != null)
                    {
                        ((IDisposable)_client).Dispose();
                    }
                }
            }
            WasDisposed = true;
        }

        #endregion

        private void InitClient(TcpClient client)
        {
            _client = client;
            _client.NoDelay = true;
            _client.ReceiveTimeout = Timeout.Seconds;
            _client.SendTimeout = Timeout.Seconds;
            _client.ReceiveBufferSize = Buffer.Length;
            var ipAddress = (IPEndPoint)_client.Client.RemoteEndPoint;
            Binding = NodeBuilder.FromEndpoint(ipAddress);
        }

        private void InitClient()
        {
            _client = new TcpClient()
            {
                ReceiveTimeout = Timeout.Seconds,
                SendTimeout = Timeout.Seconds,
                Client = {NoDelay = true},
                ReceiveBufferSize = Buffer.Length
            };
        }
    }
}