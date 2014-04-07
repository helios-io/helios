using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Helios.Exceptions;
using Helios.Net;
using Helios.Topology;

namespace Helios.Reactor
{
    public abstract class ReactorBase : IReactor
    {
        protected Socket Listener;

        /// <summary>
        /// shared buffer used by all incoming connections
        /// </summary>
        protected byte[] Buffer;

        protected ReactorBase(IPAddress localAddress, int localPort, SocketType socketType = SocketType.Stream, ProtocolType protocol = ProtocolType.Tcp, int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE)
        {
            LocalEndpoint = new IPEndPoint(localAddress, localPort);
            Listener = new Socket(AddressFamily.InterNetwork, socketType, protocol);
            Buffer = new byte[bufferSize];
            Backlog = NetworkConstants.DefaultBacklog;
        }

        public event ConnectionEstablishedCallback OnConnection;
        public event ReceivedDataCallback OnReceive;
        public event ConnectionTerminatedCallback OnDisconnection;
        

        public abstract bool IsActive { get; protected set; }
        public bool WasDisposed { get; protected set; }

        public void Start()
        {
            //Don't restart
            if (IsActive) return;

            CheckWasDisposed();
            IsActive = true;
            Listener.Bind(LocalEndpoint);
            StartInternal();
        }

        protected abstract void StartInternal();

        public void Stop()
        {
            CheckWasDisposed();
            Listener.Shutdown(SocketShutdown.Both);
            IsActive = false;
            StopInternal();
        }

        protected abstract void StopInternal();

        /// <summary>
        /// Invoked when a new node has connected to this server
        /// </summary>
        /// <param name="node">The <see cref="INode"/> instance that just connected</param>
        protected void NodeConnected(INode node)
        {
            if (OnConnection != null)
            {
                OnConnection(node);
            }
        }

        /// <summary>
        /// Invoked when a node's connection to this server has been disconnected
        /// </summary>
        /// <param name="node">The <see cref="INode"/> instance that just disconnected</param>
        /// <param name="reason">The reason why this node disconnected</param>
        protected void NodeDisconnected(INode node, HeliosConnectionException reason)
        {
            if (OnDisconnection != null)
            {
                OnDisconnection(node, reason);
            }
        }

        /// <summary>
        /// Abstract method to be filled in by a child class - data received from the
        /// network is injected into this method via the <see cref="NetworkData"/> data type.
        /// </summary>
        /// <param name="availableData">Data available from the network, including a response address</param>
        protected void ReceivedData(NetworkData availableData)
        {
            if (OnReceive != null)
            {
                OnReceive(availableData)
            }
        }

        public abstract void Send(byte[] message, INode responseAddress);

        public int Backlog { get; set; }

        public IPEndPoint LocalEndpoint { get; protected set; }
       

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public abstract void Dispose(bool disposing);

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