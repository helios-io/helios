using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Helios.Buffers;
using Helios.Exceptions;
using Helios.Net;
using Helios.Ops;
using Helios.Reactor.Response;
using Helios.Serialization;
using Helios.Topology;


namespace Helios.Reactor.Udp
{
    public class UdpProxyReactor : ProxyReactorBase
    {
        protected EndPoint RemoteEndPoint;

        public UdpProxyReactor(IPAddress localAddress, int localPort, NetworkEventLoop eventLoop, IMessageEncoder encoder, IMessageDecoder decoder, IByteBufAllocator allocator, int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE)
            : base(localAddress, localPort, eventLoop, encoder, decoder, allocator, SocketType.Dgram, ProtocolType.Udp, bufferSize)
        {
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
                Listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, config.GetOption<bool>("reuseAddress"));
            if (config.HasOption<bool>("keepAlive"))
                Listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, config.GetOption<bool>("keepAlive"));
        }

        protected override void StartInternal()
        {
            IsActive = true;
            var receiveState = CreateNetworkState(Listener, Node.Empty());
            Listener.BeginReceiveFrom(Buffer, 0, Buffer.Length, SocketFlags.None, ref RemoteEndPoint, ReceiveCallback, receiveState);
        }

        private void ReceiveCallback(IAsyncResult ar)
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

                var received = receiveState.Socket.EndReceiveFrom(ar, ref RemoteEndPoint);
                var remoteAddress = (IPEndPoint)RemoteEndPoint;

                if (receiveState.RemoteHost.IsEmpty())
                    receiveState.RemoteHost = remoteAddress.ToNode(TransportType.Udp);

                ReactorResponseChannel adapter;
                if (SocketMap.ContainsKey(receiveState.RemoteHost))
                {
                    adapter = SocketMap[receiveState.RemoteHost];
                }
                else
                {
                    adapter = new ReactorProxyResponseChannel(this, receiveState.Socket, remoteAddress, EventLoop);;
                    SocketMap.Add(adapter.RemoteHost, adapter);
                    NodeConnected(adapter.RemoteHost, adapter);
                }

                receiveState.Buffer.WriteBytes(Buffer, 0, received);

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
                
                receiveState.Socket.BeginReceiveFrom(Buffer, 0, Buffer.Length, SocketFlags.None, ref RemoteEndPoint, ReceiveCallback, receiveState); //receive more messages
            }
            catch (SocketException ex) //node disconnected
            {
                var connection = SocketMap[receiveState.RemoteHost];
                CloseConnection(ex, connection);
            }
            catch (Exception ex)
            {
                var connection = SocketMap[receiveState.RemoteHost];
                OnErrorIfNotNull(ex, connection);
            }
        }

        public override void Send(NetworkData data)
        {
            List<NetworkData> encoded;
            Encoder.Encode(data, out encoded);
            foreach (var message in encoded)
                Listener.BeginSendTo(message.Buffer, 0, message.Length, SocketFlags.None, data.RemoteHost.ToEndPoint(), SendCallback, Listener);
        }

        public override void Send(byte[] buffer, int index, int length, INode destination)
        {
            throw new NotImplementedException();
        }

        private void SendCallback(IAsyncResult ar)
        {
            var socket = (Socket)ar.AsyncState;
            try
            {
                socket.EndSendTo(ar);
            }
            catch (SocketException ex) //node disconnected
            {
                var connection = SocketMap[node];
                CloseConnection(ex, connection);
            }
            catch (Exception ex)
            {
                var connection = SocketMap[node];
                OnErrorIfNotNull(ex, connection);
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
            finally
            {
                if(SocketMap.ContainsKey(remoteConnection.RemoteHost))
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
            }
            IsActive = false;
            WasDisposed = true;
        }

        #endregion


    }
}
