using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Helios.Buffers;
using Helios.Exceptions;
using Helios.Net;
using Helios.Reactor.Response;
using Helios.Serialization;
using Helios.Topology;

namespace Helios.Reactor.Tcp
{
    /// <summary>
    /// High-performance TCP reactor that uses a single buffer and manages all client connections internally.
    /// 
    /// Passes <see cref="ReactorProxyResponseChannel"/> instances to connected clients and allows them to set up their own event loop behavior.
    /// 
    /// All I/O is still handled internally through the proxy reactor.
    /// </summary>
    public class TcpProxyReactor : ProxyReactorBase
    {
        public TcpProxyReactor(IPAddress localAddress, int localPort, NetworkEventLoop eventLoop, IMessageEncoder encoder, IMessageDecoder decoder, IByteBufAllocator allocator, int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE)
            : base(localAddress, localPort, eventLoop, encoder, decoder, allocator, SocketType.Stream, ProtocolType.Tcp, bufferSize)
        {
            LocalEndpoint = new IPEndPoint(localAddress, localPort);
            Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public override bool IsActive { get; protected set; }
        public override void Configure(IConnectionConfig config)
        {
            if (config.HasOption<int>("receiveBufferSize"))
                Listener.ReceiveBufferSize = config.GetOption<int>("receiveBufferSize");
            if (config.HasOption<int>("sendBufferSize"))
                Listener.SendBufferSize = config.GetOption<int>("sendBufferSize");
            if (config.HasOption<bool>("reuseAddress"))
                Listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, config.GetOption<bool>("reuseAddress"));
            if (config.HasOption<int>("backlog"))
                Backlog = config.GetOption<int>("backlog");
            if (config.HasOption<bool>("tcpNoDelay"))
                Listener.NoDelay = config.GetOption<bool>("tcpNoDelay");
            if (config.HasOption<bool>("keepAlive"))
                Listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, config.GetOption<bool>("keepAlive"));
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
            var newSocket = Listener.EndAccept(ar);
            var node = NodeBuilder.FromEndpoint((IPEndPoint)newSocket.RemoteEndPoint);

            var receiveState = CreateNetworkState(newSocket, node);
            var responseChannel = new TcpReactorResponseChannel(this, newSocket, EventLoop.Clone(ProxiesShareFiber));
            SocketMap.Add(node, responseChannel);
            NodeConnected(node, responseChannel);
            try
            {
                newSocket.BeginReceive(receiveState.RawBuffer, 0, receiveState.RawBuffer.Length, SocketFlags.None,
                    ReceiveCallback, receiveState);
            }
            catch (SocketException ex) //error attempting to receive on the socket
            {
                CloseConnection(ex, responseChannel);
            }
            Listener.BeginAccept(AcceptCallback, null); //accept more connections
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            var receiveState = (NetworkState)ar.AsyncState;
            try
            {
                var received = receiveState.Socket.EndReceive(ar);

                if (!receiveState.Socket.Connected || received == 0)
                {
                    var connection = SocketMap[receiveState.RemoteHost];
                    CloseConnection(connection);
                    return;
                }


                receiveState.Buffer.WriteBytes(receiveState.RawBuffer, 0, received);

                var adapter = SocketMap[receiveState.RemoteHost];

                List<IByteBuf> decoded;
                adapter.Decoder.Decode(ConnectionAdapter, receiveState.Buffer, out decoded);

                foreach (var message in decoded)
                {
                    var networkData = NetworkData.Create(receiveState.RemoteHost, message);
                    ReceivedData(networkData, adapter);
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
                if (SocketMap.ContainsKey(receiveState.RemoteHost))
                {
                    var connection = SocketMap[receiveState.RemoteHost];
                    OnErrorIfNotNull(ex, connection);
                }
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
                    var state = CreateNetworkState(clientSocket.Socket, destination, message,0);
                    clientSocket.Socket.BeginSend(message.ToArray(), 0, message.ReadableBytes, SocketFlags.None,
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
            catch(Exception innerEx)
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

        private void SendCallback(IAsyncResult ar)
        {
            var receiveState = (NetworkState)ar.AsyncState;
            try
            {
                if (!receiveState.Socket.Connected)
                {
                    var connection = SocketMap[receiveState.RemoteHost];
                    CloseConnection(connection);
                    return;
                }

                var bytesSent = receiveState.Socket.EndSend(ar);
                receiveState.Buffer.SkipBytes(bytesSent);

                if(receiveState.Buffer.ReadableBytes > 0) //need to send again
                    receiveState.Socket.BeginSend(receiveState.Buffer.ToArray(), 0, receiveState.Buffer.ReadableBytes, SocketFlags.None,
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
        public TcpSingleEventLoopProxyReactor(IPAddress localAddress, int localPort, NetworkEventLoop eventLoop, IMessageEncoder encoder, IMessageDecoder decoder, IByteBufAllocator allocator, int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE)
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
