using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Helios.Exceptions;
using Helios.Topology;

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
        protected UdpClient Client;
        protected EndPoint RemoteEndpoint;

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

        public override bool Blocking
        {
            get { return Client.Client.Blocking; }
            set { Client.Client.Blocking = value; }
        }

        public override bool IsOpen()
        {
            if (Client == null) return false;
            return Client.Client.Connected;
        }

        public override int Available
        {
            get
            {
                if (!IsOpen()) return 0;
                return Client.Available;
            }
        }

        public override async Task<bool> OpenAsync()
        {
            Open();
            return await Task.Run(() => true);
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

            if (Client == null)
                InitClient();

            try
            {
                // ReSharper disable once PossibleNullReferenceException
                Client.Client.Bind(Binding.ToEndPoint());
            }
            catch (SocketException ex)
            {
                throw new HeliosConnectionException(ExceptionType.NotOpen, ex);
            }
        }

        protected override void BeginReceiveInternal()
        {
            Client.Client.BeginReceiveFrom(Buffer, 0, Buffer.Length, SocketFlags.None, ref RemoteEndpoint, ReceiveCallback, Client.Client);
        }

        protected override void ReceiveCallback(IAsyncResult ar)
        {
            var socket = (Socket)ar.AsyncState;
            try
            {
                var buffSize = socket.EndReceiveFrom(ar, ref RemoteEndpoint);
                var receivedData = new byte[buffSize];
                Array.Copy(Buffer, receivedData, buffSize);

                var networkData = NetworkData.Create(NodeBuilder.FromEndpoint((IPEndPoint)RemoteEndpoint),
                    receivedData, buffSize);
                RemoteHost = networkData.RemoteHost;

                //continue receiving in a loop
                if (Receiving)
                {
                    socket.BeginReceiveFrom(Buffer, 0, Buffer.Length, SocketFlags.None, ref RemoteEndpoint, ReceiveCallback, socket);
                }
                InvokeReceiveIfNotNull(networkData);
            }
            catch (SocketException ex) //typically means that the socket is now closed
            {
                Receiving = false;
                InvokeDisconnectIfNotNull(NodeBuilder.FromEndpoint((IPEndPoint)RemoteEndpoint), new HeliosConnectionException(ExceptionType.Closed, ex));
                Dispose();
            }
        }

        public override void Close(Exception reason)
        {
            CheckWasDisposed();

            if (!IsOpen())
                return;

            Client.Close();
            Client = null;
            InvokeDisconnectIfNotNull(RemoteHost, new HeliosConnectionException(ExceptionType.Closed, reason));
        }

        public override void Close()
        {
           Close(null);
        }

        public override void Send(NetworkData payload)
        {
            try
            {
                Client.Send(payload.Buffer, payload.Length, payload.RemoteHost.ToEndPoint());
            }
            catch (SocketException ex) //socket probably closed
            {
                Close(ex);
            }
        }

#if !NET35 && !NET40
        public override async Task SendAsync(NetworkData payload)
        {
            await Client.SendAsync(payload.Buffer, payload.Length, payload.RemoteHost.ToEndPoint());
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
            Client = new UdpClient(){ MulticastLoopback = false };
        }

        protected void InitClient(UdpClient client)
        {
            Client = client;
            var ipAddress = (IPEndPoint)Client.Client.RemoteEndPoint;
            Local = Binding = NodeBuilder.FromEndpoint(ipAddress);
            RemoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
        }

        #endregion

        #region IDisposable members

        protected override void Dispose(bool disposing)
        {
            if (!WasDisposed)
            {
                if (disposing)
                {
                    if (Client != null)
                    {
                        Close();
                        ((IDisposable)Client).Dispose();
                    }
                }
            }
            WasDisposed = true;
        }

        #endregion
    }
}