using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Helios.Core.Exceptions;
using Helios.Core.Topology;

namespace Helios.Core.Net.Connections
{
    public class UdpConnection : UnstreamedConnectionBase
    {
        protected UdpClient _client;

        public UdpConnection(INode node, TimeSpan timeout)
            : base(node, timeout)
        {
            InitClient();
        }

        public UdpConnection(INode node)
            : base(node)
        {
            InitClient();
        }

        public UdpConnection(UdpClient client)
        {
            InitClient(client);
        }

        #region IConnection Members

        public override TransportType Transport
        {
            get { return TransportType.Udp; }
        }

        public override bool IsOpen()
        {
            if (_client == null) return false;
            return _client.Client.Connected;
        }

        public override int Available
        {
            get
            {
                if (!IsOpen()) return 0;
                return _client.Available;
            }
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

            try
            {
                // ReSharper disable once PossibleNullReferenceException
                _client.Client.Bind(Node.ToEndPoint());
            }
            catch (SocketException ex)
            {
                throw new HeliosConnectionException(ExceptionType.NotOpen, ex);
            }
        }

        public override void Close()
        {
            CheckWasDisposed();

            if (!IsOpen())
                return;

            _client.Close();
            _client = null;
        }

        public override NetworkData Receive()
        {
            var remoteHost = Node.ToEndPoint();
            var bytes = _client.Receive(ref remoteHost);
            return NetworkData.Create(remoteHost.ToNode(Transport), bytes, bytes.Length);
        }

        public override async Task<NetworkData> RecieveAsync()
        {
            var bytes = await _client.ReceiveAsync();
            return NetworkData.Create(bytes);
        }

        public override void Send(NetworkData payload)
        {
            _client.Send(payload.Data, payload.Bytes, payload.RemoteHost.ToEndPoint());
        }

        public override async Task SendAsync(NetworkData payload)
        {
            await _client.SendAsync(payload.Data, payload.Bytes, payload.RemoteHost.ToEndPoint());
        }

        #endregion

        #region Internal members


        private void InitClient()
        {
            _client = new UdpClient();
        }

        private void InitClient(UdpClient client)
        {
            _client = client;
            var ipAddress = (IPEndPoint)_client.Client.RemoteEndPoint;
            Node = NodeBuilder.FromEndpoint(ipAddress);
        }

        #endregion

        #region IDisposable members

        protected override void Dispose(bool disposing)
        {
            if (!WasDisposed)
            {
                if (disposing)
                {
                    if (_client != null)
                    {
                        Close();
                        ((IDisposable)_client).Dispose();
                    }
                }
            }
            WasDisposed = true;
        }

        #endregion
    }
}