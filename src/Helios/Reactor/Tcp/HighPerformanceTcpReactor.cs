using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Helios.Exceptions;
using Helios.Net;
using Helios.Topology;

namespace Helios.Reactor.Tcp
{
    public abstract class HighPerformanceTcpReactor : ReactorBase
    {
        /// <summary>
        /// shared buffer used by all incoming connections
        /// </summary>
        protected Dictionary<Socket, INode> NodeMap = new Dictionary<Socket, INode>();
        protected Dictionary<INode, Socket> SocketMap = new Dictionary<INode, Socket>();

        protected HighPerformanceTcpReactor(IPAddress localAddress, int localPort, int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE) 
            : base(localAddress, localPort, SocketType.Stream, ProtocolType.Tcp, bufferSize)
        {
            LocalEndpoint = new IPEndPoint(localAddress, localPort);
            Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Buffer = new byte[bufferSize];
        }


        public override bool IsActive { get; protected set; }
        protected override void StartInternal()
        {
            Listener.Listen(Backlog);
            Listener.BeginAccept(AcceptCallback, null);
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            var newSocket = Listener.EndAccept(ar);
            var node = NodeBuilder.FromEndpoint((IPEndPoint) newSocket.RemoteEndPoint);
            NodeMap.Add(newSocket, node);
            SocketMap.Add(node, newSocket);
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
                ReceivedData(networkData);
            }
            catch (SocketException ex) //node disconnected
            {
                var node = NodeMap[socket];
                NodeMap.Remove(socket);
                SocketMap.Remove(node);
                NodeDisconnected(node, new HeliosConnectionException(ExceptionType.Closed, ex));
            }
        }

        protected override void StopInternal()
        {
        }

        public override void Send(byte[] message, INode responseAddress)
        {
            var clientSocket = SocketMap[responseAddress];
            clientSocket.BeginSend(message, 0, message.Length, SocketFlags.None, SendCallback, clientSocket);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            var socket = (Socket) ar.AsyncState;
            socket.EndSend(ar);
        }

        #region IDisposable Members

        public override void Dispose(bool disposing)
        {
            if (!WasDisposed && disposing && Listener != null)
            {
                Stop();
            }
            IsActive = false;
            WasDisposed = true;
        }

        #endregion
    }
}
