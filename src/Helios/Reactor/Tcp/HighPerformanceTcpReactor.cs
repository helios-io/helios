using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Helios.Exceptions;
using Helios.Net;
using Helios.Topology;

namespace Helios.Reactor.Tcp
{
    public class HighPerformanceTcpReactor : ReactorBase
    {
        protected Dictionary<Socket, INode> NodeMap = new Dictionary<Socket, INode>();
        protected Dictionary<INode, ReactorConnectionAdapter> SocketMap = new Dictionary<INode, ReactorConnectionAdapter>();

        public HighPerformanceTcpReactor(IPAddress localAddress, int localPort, int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE) 
            : base(localAddress, localPort, SocketType.Stream, ProtocolType.Tcp, bufferSize)
        {
            LocalEndpoint = new IPEndPoint(localAddress, localPort);
            Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Buffer = new byte[bufferSize];
        }

        public override bool IsActive { get; protected set; }
        protected override void StartInternal()
        {
            IsActive = true;
            Listener.Listen(Backlog);
            Listener.BeginAccept(AcceptCallback, null);
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            var newSocket = Listener.EndAccept(ar);
            var node = NodeBuilder.FromEndpoint((IPEndPoint) newSocket.RemoteEndPoint);
            NodeMap.Add(newSocket, node);
            SocketMap.Add(node, new ReactorConnectionAdapter(this,newSocket));
            NodeConnected(node);
            newSocket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, ReceiveCallback, newSocket);
            Listener.BeginAccept(AcceptCallback, null); //accept more connections
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            var socket = (Socket)ar.AsyncState;
            try
            {
                var received = socket.EndReceive(ar);
                var dataBuff = new byte[received];
                Array.Copy(Buffer, dataBuff, received);
                var networkData = new NetworkData() { Buffer = dataBuff, Length = received, RemoteHost = NodeMap[socket] };
                socket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, ReceiveCallback, socket); //receive more messages
                var adapter = SocketMap[NodeMap[socket]];
                ReceivedData(networkData, adapter);
            }
            catch (SocketException ex) //node disconnected
            {
                var node = NodeMap[socket];
                CloseConnection(node, ex);
            }
        }

        protected override void StopInternal()
        {
        }

        public override void Send(byte[] message, INode responseAddress)
        {
            var clientSocket = SocketMap[responseAddress];
            clientSocket.Socket.BeginSend(message, 0, message.Length, SocketFlags.None, SendCallback, clientSocket);
        }

        internal override void CloseConnection(INode remoteHost, Exception ex)
        {
            var clientSocket = SocketMap[remoteHost];

            try
            {
                if (clientSocket.Socket.Connected)
                {
                    clientSocket.Socket.Close();
                }
                NodeDisconnected(remoteHost, new HeliosConnectionException(ExceptionType.Closed, ex));
            }
            finally
            {
                NodeMap.Remove(clientSocket.Socket);
                SocketMap.Remove(remoteHost);
            }
        }

        internal override void CloseConnection(INode remoteHost)
        {
           CloseConnection(remoteHost, null);
        }

        private void SendCallback(IAsyncResult ar)
        {
            var socket = (Socket) ar.AsyncState;
            try
            {
                socket.EndSend(ar);
            }
            catch (SocketException ex) //node disconnected
            {
                var node = NodeMap[socket];
                CloseConnection(node, ex);
            }
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
