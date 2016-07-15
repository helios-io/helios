// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Helios.Buffers;
using Helios.Channels;
using Helios.Exceptions;
using Helios.Net;
using Helios.Reactor.Response;
using Helios.Serialization;
using Helios.Topology;

namespace Helios.Reactor.Udp
{
    public class UdpProxyReactor : ProxyReactorBase
    {
        protected EndPoint RemoteEndPoint;

        public UdpProxyReactor(IPAddress localAddress, int localPort, NetworkEventLoop eventLoop,
            IMessageEncoder encoder, IMessageDecoder decoder, IByteBufAllocator allocator,
            int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE)
            : base(
                localAddress, localPort, eventLoop, encoder, decoder, allocator, SocketType.Dgram, ProtocolType.Udp,
                bufferSize)
        {
            LocalEndpoint = new IPEndPoint(localAddress, localPort);
            RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
        }

        public override bool IsActive { get; protected set; }

        public override void Configure(IConnectionConfig config)
        {
            if (config.HasOption<int>("receiveBufferSize"))
                Listener.ReceiveBufferSize = config.GetOption<int>("receiveBufferSize");
            if (config.HasOption<int>("sendBufferSize"))
                Listener.SendBufferSize = config.GetOption<int>("sendBufferSize");
            if (config.HasOption<bool>("reuseAddress"))
                Listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress,
                    config.GetOption<bool>("reuseAddress"));
            if (config.HasOption<bool>("keepAlive"))
                Listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive,
                    config.GetOption<bool>("keepAlive"));
            if (config.HasOption<bool>("proxiesShareFiber"))
                ProxiesShareFiber = config.GetOption<bool>("proxiesShareFiber");
            else
                ProxiesShareFiber = true;
        }

        protected override void StartInternal()
        {
            IsActive = true;
            var receiveState = CreateNetworkState(Listener, Node.Empty());
            Listener.BeginReceiveFrom(receiveState.RawBuffer, 0, receiveState.RawBuffer.Length, SocketFlags.None,
                ref RemoteEndPoint, ReceiveCallback, receiveState);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            var receiveState = (NetworkState) ar.AsyncState;
            try
            {
                var received = receiveState.Socket.EndReceiveFrom(ar, ref RemoteEndPoint);
                if (received == 0)
                {
                    if (SocketMap.ContainsKey(receiveState.RemoteHost))
                    {
                        var connection = SocketMap[receiveState.RemoteHost];
                        CloseConnection(connection);
                    }
                    return;
                }

                var remoteAddress = (IPEndPoint) RemoteEndPoint;

                if (receiveState.RemoteHost.IsEmpty())
                    receiveState.RemoteHost = remoteAddress.ToNode(TransportType.Udp);

                ReactorResponseChannel adapter;
                if (SocketMap.ContainsKey(receiveState.RemoteHost))
                {
                    adapter = SocketMap[receiveState.RemoteHost];
                }
                else
                {
                    adapter = new ReactorProxyResponseChannel(this, receiveState.Socket, remoteAddress,
                        EventLoop.Clone(ProxiesShareFiber));
                    ;
                    SocketMap.Add(adapter.RemoteHost, adapter);
                    NodeConnected(adapter.RemoteHost, adapter);
                }

                receiveState.Buffer.WriteBytes(receiveState.RawBuffer, 0, received);

                List<IByteBuf> decoded;
                Decoder.Decode(ConnectionAdapter, receiveState.Buffer, out decoded);

                foreach (var message in decoded)
                {
                    var networkData = NetworkData.Create(receiveState.RemoteHost, message);
                    ReceivedData(networkData, adapter);
                }

                //reuse the buffer
                if (receiveState.Buffer.ReadableBytes == 0)
                    receiveState.Buffer.SetIndex(0, 0);
                else
                    receiveState.Buffer.CompactIfNecessary();

                receiveState.Socket.BeginReceiveFrom(receiveState.RawBuffer, 0, receiveState.RawBuffer.Length,
                    SocketFlags.None, ref RemoteEndPoint, ReceiveCallback, receiveState); //receive more messages
            }
            catch (SocketException ex) //node disconnected
            {
                var connection = SocketMap[receiveState.RemoteHost];
                CloseConnection(ex, connection);
            }
            catch (ObjectDisposedException ex)
            {
                if (SocketMap.ContainsKey(receiveState.RemoteHost))
                {
                    var connection = SocketMap[receiveState.RemoteHost];
                    CloseConnection(ex, connection);
                }
            }
            catch (Exception ex)
            {
                var connection = SocketMap[receiveState.RemoteHost];
                OnErrorIfNotNull(ex, connection);
            }
        }

        public override void Send(byte[] buffer, int index, int length, INode destination)
        {
            var clientSocket = SocketMap[destination];
            try
            {
                if (clientSocket.WasDisposed)
                {
                    CloseConnection(clientSocket);
                    return;
                }

                var buf = Allocator.Buffer(length);
                buf.WriteBytes(buffer, index, length);
                List<IByteBuf> encodedMessages;
                Encoder.Encode(ConnectionAdapter, buf, out encodedMessages);
                foreach (var message in encodedMessages)
                {
                    var state = CreateNetworkState(clientSocket.Socket, destination, message, 0);
                    clientSocket.Socket.BeginSendTo(message.ToArray(), 0, message.ReadableBytes, SocketFlags.None,
                        destination.ToEndPoint(),
                        SendCallback, state);
                }
            }
            catch (SocketException ex)
            {
                CloseConnection(ex, clientSocket);
            }
            catch (Exception ex)
            {
                OnErrorIfNotNull(ex, clientSocket);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            var receiveState = (NetworkState) ar.AsyncState;
            try
            {
                var bytesSent = receiveState.Socket.EndSend(ar);
                receiveState.Buffer.SkipBytes(bytesSent);

                if (receiveState.Buffer.ReadableBytes > 0) //need to send again
                    receiveState.Socket.BeginSendTo(receiveState.Buffer.ToArray(), 0, receiveState.Buffer.ReadableBytes,
                        SocketFlags.None, receiveState.RemoteHost.ToEndPoint(),
                        SendCallback, receiveState);
            }
            catch (SocketException ex) //node disconnected
            {
                if (SocketMap.ContainsKey(receiveState.RemoteHost))
                {
                    var connection = SocketMap[receiveState.RemoteHost];
                    CloseConnection(ex, connection);
                }
            }
            catch (Exception ex)
            {
                if (SocketMap.ContainsKey(receiveState.RemoteHost))
                {
                    var connection = SocketMap[receiveState.RemoteHost];
                    OnErrorIfNotNull(ex, connection);
                }
            }
        }

        internal override void CloseConnection(IConnection remoteHost)
        {
            CloseConnection(null, remoteHost);
        }

        internal override void CloseConnection(Exception reason, IConnection remoteConnection)
        {
            //NO-OP (no connections in UDP)
            try
            {
                NodeDisconnected(new HeliosConnectionException(ExceptionType.Closed, reason), remoteConnection);
            }
            catch (Exception innerEx)
            {
                OnErrorIfNotNull(innerEx, remoteConnection);
            }
            finally
            {
                if (SocketMap.ContainsKey(remoteConnection.RemoteHost))
                    SocketMap.Remove(remoteConnection.RemoteHost);
            }
        }

        protected override void StopInternal()
        {
            //NO-OP
        }

        #region IDisposable Members

        public override void Dispose(bool disposing)
        {
            if (!WasDisposed && disposing && Listener != null)
            {
                Stop();
                Listener.Dispose();
                EventLoop.Dispose();
            }
            IsActive = false;
            WasDisposed = true;
        }

        #endregion
    }
}