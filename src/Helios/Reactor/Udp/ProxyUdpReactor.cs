using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Helios.Exceptions;
using Helios.Net;
using Helios.Ops;
using Helios.Reactor.Response;
using Helios.Topology;


namespace Helios.Reactor.Udp
{
    public class ProxyUdpReactor : ReactorBase
    {
        protected Dictionary<IPEndPoint, INode> NodeMap = new Dictionary<IPEndPoint, INode>();
        protected Dictionary<INode, ReactorResponseChannel> SocketMap = new Dictionary<INode, ReactorResponseChannel>();

        public ProxyUdpReactor(IPAddress localAddress, int localPort, IEventLoop eventLoop, int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE) 
            : base(localAddress, localPort, eventLoop, SocketType.Dgram, ProtocolType.Udp, bufferSize)
        {
        }

        public override bool IsActive { get; protected set; }

        protected override void StartInternal()
        {
            IsActive = true;
            Listener.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, ReceiveCallback, Listener);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            var socket = (Socket) ar.AsyncState;
            try
            {
                var received = socket.EndReceive(ar);
                var dataBuff = new byte[received];
                Array.Copy(Buffer, dataBuff, received);

                var remoteAddress = (IPEndPoint)socket.RemoteEndPoint;
                ReactorResponseChannel adapter;
                if (NodeMap.ContainsKey(remoteAddress))
                {
                    adapter = SocketMap[NodeMap[remoteAddress]];
                }
                else
                {
                    adapter = new ReactorProxyResponseChannel(this, socket, remoteAddress, EventLoop);
                    NodeMap.Add(remoteAddress, adapter.RemoteHost);
                    SocketMap.Add(adapter.RemoteHost, adapter);
                    NodeConnected(adapter.RemoteHost);
                }

                var networkData = new NetworkData() { Buffer = dataBuff, Length = received, RemoteHost = adapter.RemoteHost };
                socket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, ReceiveCallback, socket); //receive more messages
                ReceivedData(networkData, adapter);
            }
            catch (SocketException ex)
            {
                var node =  NodeBuilder.FromEndpoint((IPEndPoint)socket.RemoteEndPoint);
                CloseConnection(node, ex);
            }
        }

        public override void Send(byte[] message, INode responseAddress)
        {
            Listener.BeginSendTo(message, 0, message.Length, SocketFlags.None, responseAddress.ToEndPoint(), SendCallback, Listener);
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
                var node = NodeMap[(IPEndPoint)socket.RemoteEndPoint];
                CloseConnection(node, ex);
            }
        }

        internal override void CloseConnection(INode remoteHost)
        {
            CloseConnection(remoteHost, null);
        }

        internal override void CloseConnection(INode remoteHost, Exception reason)
        {
            //NO-OP (no connections in UDP)
            try
            {
                NodeDisconnected(remoteHost, new HeliosConnectionException(ExceptionType.Closed, reason));
            }
            finally
            {
                NodeMap.Remove(remoteHost.ToEndPoint());
                SocketMap.Remove(remoteHost);
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
