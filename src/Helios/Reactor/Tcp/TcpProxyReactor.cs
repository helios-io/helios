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
using Helios.Tracing;

namespace Helios.Reactor.Tcp
{
    /// <summary>
    ///     High-performance TCP reactor that uses a single buffer and manages all client connections internally.
    ///     Passes <see cref="ReactorProxyResponseChannel" /> instances to connected clients and allows them to set up their
    ///     own event loop behavior.
    ///     All I/O is still handled internally through the proxy reactor.
    /// </summary>
    public class TcpProxyReactor : ProxyReactorBase
    {
        public TcpProxyReactor(IPAddress localAddress, int localPort, NetworkEventLoop eventLoop,
            IMessageEncoder encoder, IMessageDecoder decoder, IByteBufAllocator allocator,
            int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE)
            : base(
                localAddress, localPort, eventLoop, encoder, decoder, allocator, SocketType.Stream, ProtocolType.Tcp,
                bufferSize)
        {
            LocalEndpoint = new IPEndPoint(localAddress, localPort);
            Listener = new Socket(LocalEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
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
            if (config.HasOption<int>("backlog"))
                Backlog = config.GetOption<int>("backlog");
            if (config.HasOption<bool>("tcpNoDelay"))
                Listener.NoDelay = config.GetOption<bool>("tcpNoDelay");
            if (config.HasOption<bool>("keepAlive"))
                Listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive,
                    config.GetOption<bool>("keepAlive"));
            if (config.HasOption<bool>("linger") && config.GetOption<bool>("linger"))
                Listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, new LingerOption(true, 10));
            else
                Listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            if (config.HasOption<bool>("proxiesShareFiber"))
                ProxiesShareFiber = config.GetOption<bool>("proxiesShareFiber");
            else
                ProxiesShareFiber = true;
        }

        protected override void StartInternal()
        {
            IsActive = true;
            Listener.Listen(Backlog);
            Listener.BeginAccept(AcceptCallback, null);
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                var newSocket = Listener.EndAccept(ar);
                var node = NodeBuilder.FromEndpoint((IPEndPoint) newSocket.RemoteEndPoint);

                HeliosTrace.Instance.TcpInboundAcceptSuccess();

                var receiveState = CreateNetworkState(newSocket, node);
                var responseChannel = new TcpReactorResponseChannel(this, newSocket, EventLoop.Clone(ProxiesShareFiber));
                SocketMap.Add(node, responseChannel);
                NodeConnected(node, responseChannel);
                newSocket.BeginReceive(receiveState.RawBuffer, 0, receiveState.RawBuffer.Length, SocketFlags.None,
                    ReceiveCallback, receiveState);
                Listener.BeginAccept(AcceptCallback, null); //accept more connections
            }
            catch (Exception ex)
            {
                HeliosTrace.Instance.TcpInboundAcceptFailure(ex.Message);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            var receiveState = (NetworkState) ar.AsyncState;
            try
            {
                var received = receiveState.Socket.EndReceive(ar);

                if (!receiveState.Socket.Connected || received == 0)
                {
                    HeliosTrace.Instance.TcpInboundReceiveFailure();
                    var connection = SocketMap[receiveState.RemoteHost];
                    CloseConnection(connection);
                    return;
                }


                receiveState.Buffer.WriteBytes(receiveState.RawBuffer, 0, received);
                HeliosTrace.Instance.TcpInboundReceive(received);
                var adapter = SocketMap[receiveState.RemoteHost];

                List<IByteBuf> decoded;
                adapter.Decoder.Decode(ConnectionAdapter, receiveState.Buffer, out decoded);

                foreach (var message in decoded)
                {
                    var networkData = NetworkData.Create(receiveState.RemoteHost, message);
                    ReceivedData(networkData, adapter);
                    HeliosTrace.Instance.TcpInboundReceiveSuccess();
                }

                //reuse the buffer
                if (receiveState.Buffer.ReadableBytes == 0)
                    receiveState.Buffer.SetIndex(0, 0);
                else
                {
                    receiveState.Buffer.CompactIfNecessary();
                    var postCompact = receiveState.Buffer;
                }


                //continue receiving in a loop
                receiveState.Socket.BeginReceive(receiveState.RawBuffer, 0, receiveState.RawBuffer.Length,
                    SocketFlags.None, ReceiveCallback, receiveState);
            }
            catch (SocketException ex) //node disconnected
            {
                if (SocketMap.ContainsKey(receiveState.RemoteHost))
                {
                    var connection = SocketMap[receiveState.RemoteHost];
                    CloseConnection(ex, connection);
                }
                HeliosTrace.Instance.TcpInboundReceiveFailure();
            }
            catch (ObjectDisposedException ex)
            {
                if (SocketMap.ContainsKey(receiveState.RemoteHost))
                {
                    var connection = SocketMap[receiveState.RemoteHost];
                    CloseConnection(ex, connection);
                }
                HeliosTrace.Instance.TcpInboundReceiveFailure();
            }
            catch (Exception ex)
            {
                if (SocketMap.ContainsKey(receiveState.RemoteHost))
                {
                    var connection = SocketMap[receiveState.RemoteHost];
                    OnErrorIfNotNull(ex, connection);
                }
                HeliosTrace.Instance.TcpInboundReceiveFailure();
            }
        }

        protected override void StopInternal()
        {
        }

        public override void Send(byte[] buffer, int index, int length, INode destination)
        {
            var clientSocket = SocketMap[destination];
            try
            {
                if (clientSocket.WasDisposed || !clientSocket.Socket.Connected)
                {
                    CloseConnection(clientSocket);
                    return;
                }

                var buf = Allocator.Buffer(length);
                buf.WriteBytes(buffer, index, length);
                List<IByteBuf> encodedMessages;
                clientSocket.Encoder.Encode(ConnectionAdapter, buf, out encodedMessages);
                foreach (var message in encodedMessages)
                {
                    var bytesToSend = message.ToArray();
                    var bytesSent = 0;
                    while (bytesSent < bytesToSend.Length)
                    {
                        bytesSent += clientSocket.Socket.Send(bytesToSend, bytesSent, bytesToSend.Length - bytesSent,
                            SocketFlags.None);
                    }
                    HeliosTrace.Instance.TcpInboundClientSend(bytesSent);
                    HeliosTrace.Instance.TcpInboundSendSuccess();
                }
            }
            catch (SocketException ex)
            {
                HeliosTrace.Instance.TcpClientSendFailure();
                CloseConnection(ex, clientSocket);
            }
            catch (Exception ex)
            {
                HeliosTrace.Instance.TcpClientSendFailure();
                OnErrorIfNotNull(ex, clientSocket);
            }
        }

        internal override void CloseConnection(Exception ex, IConnection remoteHost)
        {
            if (!SocketMap.ContainsKey(remoteHost.RemoteHost)) return; //already been removed

            var clientSocket = SocketMap[remoteHost.RemoteHost];

            try
            {
                if (clientSocket.Socket.Connected)
                {
                    clientSocket.Socket.Close();
                }
            }
            catch (Exception innerEx)
            {
                OnErrorIfNotNull(innerEx, remoteHost);
            }
            finally
            {
                NodeDisconnected(new HeliosConnectionException(ExceptionType.Closed, ex), remoteHost);

                if (SocketMap.ContainsKey(remoteHost.RemoteHost))
                    SocketMap.Remove(remoteHost.RemoteHost);

                if (!clientSocket.WasDisposed)
                    clientSocket.Dispose();
            }
        }

        internal override void CloseConnection(IConnection remoteHost)
        {
            CloseConnection(null, remoteHost);
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

    public class TcpSingleEventLoopProxyReactor : TcpProxyReactor
    {
        public TcpSingleEventLoopProxyReactor(IPAddress localAddress, int localPort, NetworkEventLoop eventLoop,
            IMessageEncoder encoder, IMessageDecoder decoder, IByteBufAllocator allocator,
            int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE)
            : base(localAddress, localPort, eventLoop, encoder, decoder, allocator, bufferSize)
        {
        }

        protected override void ReceivedData(NetworkData availableData, ReactorResponseChannel responseChannel)
        {
            if (EventLoop.Receive != null)
            {
                EventLoop.Receive(availableData, responseChannel);
            }
        }
    }
}