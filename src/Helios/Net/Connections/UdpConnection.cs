// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Helios.Buffers;
using Helios.Channels;
using Helios.Exceptions;
using Helios.Serialization;
using Helios.Topology;
using Helios.Tracing;

namespace Helios.Net.Connections
{
    /// <summary>
    ///     UDP IConnection implementation.
    ///     <remarks>
    ///         N.B. It's worth nothing that <see cref="Node" /> in this IConnection implementation
    ///         refers to the local port / address that this UDP socket is bound to, rather than a remote host.
    ///     </remarks>
    /// </summary>
    public class UdpConnection : UnstreamedConnectionBase
    {
        protected Socket Client;
        protected EndPoint RemoteEndpoint;

        public UdpConnection(NetworkEventLoop eventLoop, INode binding, TimeSpan timeout, IMessageEncoder encoder,
            IMessageDecoder decoder, IByteBufAllocator allocator)
            : base(eventLoop, binding, timeout, encoder, decoder, allocator)
        {
            InitClient();
        }

        public UdpConnection(NetworkEventLoop eventLoop, INode binding, IMessageEncoder encoder, IMessageDecoder decoder,
            IByteBufAllocator allocator)
            : base(eventLoop, binding, encoder, decoder, allocator)
        {
            InitClient();
        }

        public UdpConnection(Socket client, IMessageEncoder encoder, IMessageDecoder decoder,
            IByteBufAllocator allocator)
        {
            InitClient(client);
            Encoder = encoder;
            Decoder = decoder;
            Allocator = allocator;
        }

        public UdpConnection(Socket client)
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
            get { return Client.Blocking; }
            set { Client.Blocking = value; }
        }

        public override bool IsOpen()
        {
            return Local != null;
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
                Client.ReceiveBufferSize = config.GetOption<int>("receiveBufferSize");
            if (config.HasOption<int>("sendBufferSize"))
                Client.SendBufferSize = config.GetOption<int>("sendBufferSize");
            if (config.HasOption<bool>("reuseAddress"))
                Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress,
                    config.GetOption<bool>("reuseAddress"));
            if (config.HasOption<bool>("keepAlive"))
                Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive,
                    config.GetOption<bool>("keepAlive"));
        }

        public override void Open()
        {
            CheckWasDisposed();

            if (IsOpen()) return;

            if (Binding == null || Binding.Host == null)
            {
                throw new HeliosConnectionException(ExceptionType.NotOpen,
                    "Cannot open a connection to a null Node or null Node.Host");
            }

            if (Binding.Port < 0)
            {
                throw new HeliosConnectionException(ExceptionType.NotOpen, "Cannot open a connection to an invalid port");
            }

            if (Client == null)
                InitClient();

            try
            {
                // ReSharper disable once PossibleNullReferenceException
                Client.Bind(Binding.ToEndPoint());
                Local = ((IPEndPoint) Client.LocalEndPoint).ToNode(TransportType.Udp);
                if (NetworkEventLoop.Receive != null) //automatically start receiving
                {
                    BeginReceive();
                }
            }
            catch (SocketException ex)
            {
                throw new HeliosConnectionException(ExceptionType.NotOpen, ex);
            }
        }

        protected override void BeginReceiveInternal()
        {
            var receiveState = CreateNetworkState(Client, RemoteHost);
            Client.BeginReceiveFrom(receiveState.RawBuffer, 0, receiveState.RawBuffer.Length, SocketFlags.None,
                ref RemoteEndpoint, ReceiveCallback, receiveState);
        }

        protected override void ReceiveCallback(IAsyncResult ar)
        {
            var receiveState = (NetworkState) ar.AsyncState;
            try
            {
                var buffSize = receiveState.Socket.EndReceiveFrom(ar, ref RemoteEndpoint);
                receiveState.Buffer.WriteBytes(receiveState.RawBuffer, 0, buffSize);
                receiveState.RemoteHost = ((IPEndPoint) RemoteEndpoint).ToNode(TransportType.Udp);

                HeliosTrace.Instance.UdpClientReceive(buffSize);

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
                    receiveState.Socket.BeginReceiveFrom(receiveState.RawBuffer, 0, receiveState.RawBuffer.Length,
                        SocketFlags.None, ref RemoteEndpoint, ReceiveCallback, receiveState);
                }

                HeliosTrace.Instance.UdpClientReceiveSuccess();
            }
            catch (SocketException ex) //typically means that the socket is now closed
            {
                HeliosTrace.Instance.UdpClientReceiveFailure();
                Receiving = false;
                InvokeDisconnectIfNotNull(NodeBuilder.FromEndpoint((IPEndPoint) RemoteEndpoint),
                    new HeliosConnectionException(ExceptionType.Closed, ex));
                Dispose();
            }
            catch (ObjectDisposedException ex)
            {
                HeliosTrace.Instance.UdpClientReceiveFailure();
                Receiving = false;
                InvokeDisconnectIfNotNull(NodeBuilder.FromEndpoint((IPEndPoint) RemoteEndpoint),
                    new HeliosConnectionException(ExceptionType.Closed, ex));
            }
            catch (Exception ex)
            {
                HeliosTrace.Instance.UdpClientReceiveFailure();
                InvokeErrorIfNotNull(ex);
            }
        }

        public override void Close(Exception reason)
        {
            InvokeDisconnectIfNotNull(RemoteHost, new HeliosConnectionException(ExceptionType.Closed, reason));
            CheckWasDisposed();

            if (!IsOpen())
                return;

            Client.Close();
            Client = null;
            EventLoop.Shutdown(TimeSpan.FromSeconds(2));
        }

        public override void Close()
        {
            Close(null);
        }

        protected override void SendInternal(byte[] buffer, int index, int length, INode destination)
        {
            try
            {
                if (Client == null || WasDisposed)
                {
                    HeliosTrace.Instance.UdpClientSendFailure();
                    Close();
                    return;
                }

                var buf = Allocator.Buffer(length);
                buf.WriteBytes(buffer, index, length);
                List<IByteBuf> encodedMessages;
                Encoder.Encode(this, buf, out encodedMessages);
                foreach (var message in encodedMessages)
                {
                    var bytesToSend = message.ToArray();
                    var bytesSent = 0;
                    while (bytesSent < bytesToSend.Length)
                    {
                        bytesSent += Client.SendTo(bytesToSend, bytesSent, bytesToSend.Length - bytesSent,
                            SocketFlags.None, destination.ToEndPoint());
                    }
                    HeliosTrace.Instance.UdpClientSend(bytesSent);
                    HeliosTrace.Instance.UdpClientSendSuccess();
                }
            }
            catch (SocketException ex)
            {
                HeliosTrace.Instance.UdpClientSendFailure();
                Close(ex);
            }
            catch (Exception ex)
            {
                HeliosTrace.Instance.UdpClientSendFailure();
                InvokeErrorIfNotNull(ex);
            }
        }

        #endregion

        #region Internal members

        protected void InitClient()
        {
            Client = new Socket(Binding.Host.AddressFamily, SocketType.Dgram, ProtocolType.Udp)
            {
                MulticastLoopback = false
            };
            RemoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
        }

        protected void InitClient(Socket client)
        {
            Client = client;
            var ipAddress = (IPEndPoint) Client.RemoteEndPoint;
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
                        ((IDisposable) Client).Dispose();
                        EventLoop.Dispose();
                    }
                }
            }
            WasDisposed = true;
        }

        #endregion
    }
}