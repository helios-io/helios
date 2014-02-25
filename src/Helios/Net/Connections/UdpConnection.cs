using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Helios.Exceptions;
using Helios.Topology;
using Helios.Util.Concurrency;

namespace Helios.Net.Connections
{
    /// <summary>
    /// UDP IConnection implementation.
    /// 
    /// <remarks>N.B. It's worth nothing that <see cref="Node"/> in this IConnection implementation
    /// refers to the local port / address that this UDP socket is bound to, rather than a remote host.</remarks>
    /// </summary>
    public class UdpConnection : UnstreamedConnectionBase
    {
        protected UdpClient _client;

        public UdpConnection(INode binding, TimeSpan timeout)
            : base(binding, timeout)
        {
            InitClient();
        }

        public UdpConnection(INode binding)
            : base(binding)
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

            if (Binding == null || Binding.Host == null)
            {
                throw new HeliosConnectionException(ExceptionType.NotOpen, "Cannot open a connection to a null Node or null Node.Host");
            }

            if (Binding.Port <= 0)
            {
                throw new HeliosConnectionException(ExceptionType.NotOpen, "Cannot open a connection to an invalid port");
            }

            if (_client == null)
                InitClient();

            try
            {
                // ReSharper disable once PossibleNullReferenceException
                _client.Client.Bind(Binding.ToEndPoint());
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
            var remoteHost = Binding.ToEndPoint();
            var bytes = _client.Receive(ref remoteHost);
            return NetworkData.Create(remoteHost.ToNode(Transport), bytes, bytes.Length);
        }

#if !NET35 && !NET40
        public override async Task<NetworkData> RecieveAsync()
        {
            var bytes = await _client.ReceiveAsync();
            return NetworkData.Create(bytes);
        }
#else
        public override Task<NetworkData> RecieveAsync()
        {
            return TaskRunner.Run(() => Receive());
        }
#endif

        public override void Send(NetworkData payload)
        {
            _client.Send(payload.Buffer, payload.Length, payload.RemoteHost.ToEndPoint());
        }

#if !NET35 && !NET40
        public override async Task SendAsync(NetworkData payload)
        {
            await _client.SendAsync(payload.Buffer, payload.Length, payload.RemoteHost.ToEndPoint());
        }
#else
        public override Task SendAsync(NetworkData payload)
        {
            return TaskRunner.Run(() => Send(payload));
        }
#endif

        #endregion

        #region Internal members


        protected void InitClient()
        {
            _client = new UdpClient(){ MulticastLoopback = false };
        }

        protected void InitClient(UdpClient client)
        {
            _client = client;
            var ipAddress = (IPEndPoint)_client.Client.RemoteEndPoint;
            Binding = NodeBuilder.FromEndpoint(ipAddress);
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