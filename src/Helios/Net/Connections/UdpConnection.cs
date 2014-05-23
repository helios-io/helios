using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Helios.Buffers;
using Helios.Exceptions;
using Helios.Serialization;
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
        protected UdpClient Client;
        protected EndPoint RemoteEndpoint;

        public UdpConnection(NetworkEventLoop eventLoop, INode binding, TimeSpan timeout, IMessageEncoder encoder, IMessageDecoder decoder, IByteBufAllocator allocator)
            : base(eventLoop, binding, timeout, encoder, decoder, allocator)
        {
            InitClient();
        }

        public UdpConnection(NetworkEventLoop eventLoop, INode binding, IMessageEncoder encoder, IMessageDecoder decoder, IByteBufAllocator allocator)
            : base(eventLoop, binding, encoder, decoder, allocator)
        {
            InitClient();
        }

        public UdpConnection(UdpClient client, IMessageEncoder encoder, IMessageDecoder decoder, IByteBufAllocator allocator)
        {
            InitClient(client);
            Encoder = encoder;
            Decoder = decoder;
            Allocator = allocator;
        }

        public UdpConnection(UdpClient client)
            : this(client, Encoders.DefaultEncoder, Encoders.DefaultDecoder, UnpooledByteBufAllocator.Default)
        {
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

#if NET35 || NET40
        public override Task<bool> OpenAsync()
        {
            Open();
            return TaskRunner.Run(() => true);
        }
#else
        public override async Task<bool> OpenAsync()
        {
            Open();
            return await Task.Run(() => true);
        }
#endif

        public override void Configure(IConnectionConfig config)
        {
            if (config.HasOption<int>("receiveBufferSize"))
                Client.Client.ReceiveBufferSize = config.GetOption<int>("receiveBufferSize");
            if (config.HasOption<int>("sendBufferSize"))
                Client.Client.SendBufferSize = config.GetOption<int>("sendBufferSize");
            if (config.HasOption<bool>("reuseAddress"))
                Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, config.GetOption<bool>("reuseAddress"));
            if (config.HasOption<bool>("keepAlive"))
                Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, config.GetOption<bool>("keepAlive"));
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
            var receiveState = CreateNetworkState(Client.Client, RemoteHost);
            Client.Client.BeginReceiveFrom(Buffer, 0, BufferSize, SocketFlags.None, ref RemoteEndpoint, ReceiveCallback, receiveState);
        }

        protected override void ReceiveCallback(IAsyncResult ar)
        {
            var receiveState = (NetworkState)ar.AsyncState;
            try
            {
                var buffSize = receiveState.Socket.EndReceiveFrom(ar, ref RemoteEndpoint);
                receiveState.Buffer.WriteBytes(Buffer, 0, buffSize);
                receiveState.RemoteHost = ((IPEndPoint) RemoteEndpoint).ToNode(TransportType.Udp);

                List<IByteBuf> decoded;
                Decoder.Decode(this, receiveState.Buffer, out decoded);

                foreach (var message in decoded)
                {
                    var networkData = NetworkData.Create(receiveState.RemoteHost, message);
                    InvokeReceiveIfNotNull(networkData);
                }

                //shift the contents of the buffer
                receiveState.Buffer.CompactIfNecessary();

                //continue receiving in a loop
                if (Receiving)
                {
                    receiveState.Socket.BeginReceiveFrom(Buffer, 0, Buffer.Length, SocketFlags.None, ref RemoteEndpoint, ReceiveCallback, receiveState);
                }
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

        public override void Send(byte[] buffer, int index, int length, INode destination)
        {
            try
            {
                if (Client.Client == null || WasDisposed)
                {
                    Close();
                    return;
                }

                var buf = Allocator.Buffer(length);
                buf.WriteBytes(buffer, index, length);
                List<IByteBuf> encodedMessages;
                Encoder.Encode(this, buf, out encodedMessages);
                foreach (var message in encodedMessages)
                {
                    var state = CreateNetworkState(Client.Client, destination, message);
                    state.Socket.BeginSendTo(message.ToArray(), 0, message.ReadableBytes, SocketFlags.None, destination.ToEndPoint(),
                    SendCallback, state);
                }
            }
            catch (SocketException ex)
            {
                Close(ex);
            }
            catch (Exception ex)
            {
                InvokeErrorIfNotNull(ex);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            var receiveState = (NetworkState)ar.AsyncState;
            try
            {
                if (!receiveState.Socket.Connected)
                {
                    Close();
                    return;
                }

                var bytesSent = receiveState.Socket.EndSend(ar);
                receiveState.Buffer.SkipBytes(bytesSent);

                if (receiveState.Buffer.ReadableBytes > 0) //need to send again
                    receiveState.Socket.BeginSendTo(receiveState.Buffer.ToArray(), 0, receiveState.Buffer.ReadableBytes, SocketFlags.None, receiveState.RemoteHost.ToEndPoint(),
                   SendCallback, receiveState);
            }
            catch (SocketException ex)
            {
                Close(ex);
            }
            catch (Exception ex)
            {
                InvokeErrorIfNotNull(ex);
            }
        }

        #endregion

        #region Internal members


        protected void InitClient()
        {
            Client = new UdpClient() { MulticastLoopback = false };
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