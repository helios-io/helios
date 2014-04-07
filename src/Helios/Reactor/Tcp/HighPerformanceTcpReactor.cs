using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Helios.Concurrency;
using Helios.Exceptions;
using Helios.Net;
using Helios.Topology;

namespace Helios.Reactor.Tcp
{
    public abstract class HighPerformanceTcpReactor : ReactorBase
    {
        protected Socket Listener;

        public const int DEFAULT_BUFFER_SIZE = 1024*32; //32k

        /// <summary>
        /// shared buffer used by all incoming connections
        /// </summary>
        protected byte[] Buffer;
        protected Dictionary<Socket, INode> NodeMap = new Dictionary<Socket, INode>();
        protected Dictionary<INode, Socket> SocketMap = new Dictionary<INode, Socket>();

        protected HighPerformanceTcpReactor(IPAddress localAddress, int localPort, int bufferSize = DEFAULT_BUFFER_SIZE)
        {
            LocalEndpoint = new IPEndPoint(localAddress, localPort);
            Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Buffer = new byte[bufferSize];
        }


        public override bool IsActive { get; protected set; }
        public override void Start()
        {
            //Don't restart
            if (IsActive) return;

            CheckWasDisposed();
            IsActive = true;
            Listener.Bind(LocalEndpoint);
            Listener.Listen(5);
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
                EventLoop(networkData);
            }
            catch (SocketException ex) //node disconnected
            {
                var node = NodeMap[socket];
                NodeMap.Remove(socket);
                SocketMap.Remove(node);
                NodeDisconnected(node);
            }
            
        }

        /// <summary>
        /// Invoked when a new node has connected to this server
        /// </summary>
        /// <param name="node">The <see cref="INode"/> instance that just connected</param>
        protected abstract void NodeConnected(INode node);

        /// <summary>
        /// Invoked when a node's connection to this server has been disconnected
        /// </summary>
        /// <param name="node">The <see cref="INode"/> instance that just disconnected</param>
        protected abstract void NodeDisconnected(INode node);

        /// <summary>
        /// Abstract method to be filled in by a child class - data received from the
        /// network is injected into this method via the <see cref="NetworkData"/> data type.
        /// </summary>
        /// <param name="availableData">Data available from the network, including a response address</param>
        protected abstract void EventLoop(NetworkData availableData);

        public override void Stop()
        {
            CheckWasDisposed();
            Listener.Shutdown(SocketShutdown.Both);
            IsActive = false;
        }

        public void Send(byte[] message, INode responseAddress)
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

        public void CheckWasDisposed()
        {
            if (WasDisposed)
            {
                throw new HeliosConnectionException(ExceptionType.NotOpen, "Already disposed this Reactor");
            }
        }

        #endregion
    }
}
