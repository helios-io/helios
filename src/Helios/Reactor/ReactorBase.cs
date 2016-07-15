// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Helios.Buffers;
using Helios.Channels;
using Helios.Exceptions;
using Helios.Net;
using Helios.Ops;
using Helios.Reactor.Response;
using Helios.Serialization;
using Helios.Topology;
using Helios.Util.Concurrency;

namespace Helios.Reactor
{
    public abstract class ReactorBase : IReactor
    {
        protected Socket Listener;

        protected ReactorBase(IPAddress localAddress, int localPort, NetworkEventLoop eventLoop, IMessageEncoder encoder,
            IMessageDecoder decoder, IByteBufAllocator allocator, SocketType socketType = SocketType.Stream,
            ProtocolType protocol = ProtocolType.Tcp, int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE)
        {
            Decoder = decoder;
            Encoder = encoder;
            Allocator = allocator;
            LocalEndpoint = new IPEndPoint(localAddress, localPort);
            Listener = new Socket(LocalEndpoint.AddressFamily, socketType, protocol);
            if (protocol == ProtocolType.Tcp)
            {
                Transport = TransportType.Tcp;
            }
            else if (protocol == ProtocolType.Udp)
            {
                Transport = TransportType.Udp;
            }
            Backlog = NetworkConstants.DefaultBacklog;
            EventLoop = eventLoop;
            ConnectionAdapter = new ReactorConnectionAdapter(this);
            BufferSize = bufferSize;
        }

        protected int BufferSize { get; set; }

        public bool Blocking
        {
            get { return Listener.Blocking; }
            set { Listener.Blocking = value; }
        }

        public event ReceivedDataCallback OnReceive
        {
            add { EventLoop.Receive = value; }
            // ReSharper disable once ValueParameterNotUsed
            remove { EventLoop.Receive = null; }
        }

        public event ConnectionEstablishedCallback OnConnection
        {
            add { EventLoop.Connection = value; }
            // ReSharper disable once ValueParameterNotUsed
            remove { EventLoop.Connection = null; }
        }

        public event ConnectionTerminatedCallback OnDisconnection
        {
            add { EventLoop.Disconnection = value; }
            // ReSharper disable once ValueParameterNotUsed
            remove { EventLoop.Disconnection = null; }
        }

        public event ExceptionCallback OnError
        {
            add { EventLoop.SetExceptionHandler(value, ConnectionAdapter); }
            // ReSharper disable once ValueParameterNotUsed
            remove { EventLoop.SetExceptionHandler(null, ConnectionAdapter); }
        }

        public IMessageEncoder Encoder { get; }
        public IMessageDecoder Decoder { get; }
        public IByteBufAllocator Allocator { get; }

        public IConnection ConnectionAdapter { get; }
        public NetworkEventLoop EventLoop { get; }

        public abstract bool IsActive { get; protected set; }
        public bool WasDisposed { get; protected set; }

        public abstract void Configure(IConnectionConfig config);

        public void Start()
        {
            //Don't restart
            if (IsActive) return;

            CheckWasDisposed();
            IsActive = true;
            Listener.Bind(LocalEndpoint);
            LocalEndpoint = (IPEndPoint) Listener.LocalEndPoint;
            StartInternal();
        }

        public void Stop()
        {
            CheckWasDisposed();
            try
            {
                Listener.Shutdown(SocketShutdown.Both);
                EventLoop.Shutdown(TimeSpan.FromSeconds(2));
            }
            catch
            {
            }
            IsActive = false;
            StopInternal();
        }

        public void Send(NetworkData data)
        {
            Send(data.Buffer, 0, data.Length, data.RemoteHost);
        }

        public abstract void Send(byte[] buffer, int index, int length, INode destination);

        public int Backlog { get; set; }

        public IPEndPoint LocalEndpoint { get; protected set; }
        public TransportType Transport { get; }

        protected abstract void StartInternal();

        protected abstract void StopInternal();

        /// <summary>
        ///     Invoked when a new node has connected to this server
        /// </summary>
        /// <param name="node">The <see cref="INode" /> instance that just connected</param>
        /// <param name="responseChannel">The channel that the server can respond to</param>
        protected void NodeConnected(INode node, IConnection responseChannel)
        {
            if (EventLoop.Connection != null)
            {
                EventLoop.Connection(node, responseChannel);
            }
        }

        /// <summary>
        ///     Invoked when a node's connection to this server has been disconnected
        /// </summary>
        /// <param name="closedChannel">The <see cref="IConnection" /> instance that just closed</param>
        /// <param name="reason">The reason why this node disconnected</param>
        protected void NodeDisconnected(HeliosConnectionException reason, IConnection closedChannel)
        {
            if (EventLoop.Disconnection != null)
            {
                EventLoop.Disconnection(reason, closedChannel);
            }
        }

        /// <summary>
        ///     Abstract method to be filled in by a child class - data received from the
        ///     network is injected into this method via the <see cref="NetworkData" /> data type.
        /// </summary>
        /// <param name="availableData">Data available from the network, including a response address</param>
        /// <param name="responseChannel">Available channel for handling network response</param>
        protected virtual void ReceivedData(NetworkData availableData, ReactorResponseChannel responseChannel)
        {
            if (EventLoop.Receive != null)
            {
                EventLoop.Receive(availableData, responseChannel);
            }
        }

        protected void OnErrorIfNotNull(Exception reason, IConnection connection)
        {
            if (EventLoop.Exception != null)
            {
                EventLoop.Exception(reason, connection);
            }
        }

        protected NetworkState CreateNetworkState(Socket socket, INode remotehost)
        {
            return CreateNetworkState(socket, remotehost, Allocator.Buffer(), BufferSize);
        }

        protected NetworkState CreateNetworkState(Socket socket, INode remotehost, IByteBuf buffer, int bufferSize)
        {
            return new NetworkState(socket, remotehost, buffer, bufferSize);
        }

        /// <summary>
        ///     Closes a connection to a remote host (without shutting down the server.)
        /// </summary>
        /// <param name="remoteHost">The remote host to terminate</param>
        internal abstract void CloseConnection(IConnection remoteHost);

        internal abstract void CloseConnection(Exception reason, IConnection remoteHost);

        #region ReactorConnectionAdapter

        /// <summary>
        ///     Wraps the <see cref="IReactor" /> itself inside a <see cref="IConnection" /> object and makes it callable
        ///     directly to end users
        /// </summary>
        protected class ReactorConnectionAdapter : IConnection
        {
            private ReactorBase _reactor;

            public ReactorConnectionAdapter(ReactorBase reactor)
            {
                _reactor = reactor;
            }

            public event ReceivedDataCallback Receive
            {
                add { _reactor.OnReceive += value; }
                remove { _reactor.OnReceive -= value; }
            }

            public event ConnectionEstablishedCallback OnConnection
            {
                add { _reactor.OnConnection += value; }
                remove { _reactor.OnConnection -= value; }
            }

            public event ConnectionTerminatedCallback OnDisconnection
            {
                add { _reactor.OnDisconnection += value; }
                remove { _reactor.OnDisconnection -= value; }
            }

            public event ExceptionCallback OnError
            {
                add { _reactor.OnError += value; }
                remove { _reactor.OnError -= value; }
            }

            public IEventLoop EventLoop
            {
                get { return _reactor.EventLoop; }
            }

            public IMessageEncoder Encoder
            {
                get { return _reactor.Encoder; }
            }

            public IMessageDecoder Decoder
            {
                get { return _reactor.Decoder; }
            }

            public IByteBufAllocator Allocator
            {
                get { return _reactor.Allocator; }
            }

            public DateTimeOffset Created { get; private set; }
            public INode RemoteHost { get; private set; }

            public INode Local
            {
                get { return _reactor.LocalEndpoint.ToNode(_reactor.Transport); }
            }

            public TimeSpan Timeout { get; private set; }

            public TransportType Transport
            {
                get { return _reactor.Transport; }
            }

            public bool Blocking
            {
                get { return _reactor.Blocking; }
                set { _reactor.Blocking = value; }
            }

            public bool WasDisposed { get; private set; }

            public bool Receiving
            {
                get { return _reactor.IsActive; }
            }

            public bool IsOpen()
            {
                return _reactor.IsActive;
            }

            public int Available
            {
                get { throw new NotSupportedException("[Available] is not supported on ReactorConnectionAdapter"); }
            }

            public int MessagesInSendQueue
            {
                get { return 0; }
            }

            public Task<bool> OpenAsync()
            {
                TaskRunner.Run(() => _reactor.Start());
                return TaskRunner.Run(() => true);
            }

            public void Configure(IConnectionConfig config)
            {
                _reactor.Configure(config);
            }

            public void Open()
            {
                if (_reactor.IsActive) return;
                _reactor.Start();
            }

            public void BeginReceive()
            {
                Open();
            }

            public void BeginReceive(ReceivedDataCallback callback)
            {
                Receive += callback;
            }

            public void StopReceive()
            {
                Receive += (data, channel) => { };
            }

            public void Close()
            {
                _reactor.Stop();
            }

            public void Send(NetworkData data)
            {
                _reactor.Send(data);
            }

            public void Send(byte[] buffer, int index, int length, INode destination)
            {
                _reactor.Send(buffer, index, length, destination);
            }

            public Task SendAsync(NetworkData payload)
            {
                return TaskRunner.Run(() => Send(payload));
            }

            #region IDisposable methods

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected void Dispose(bool disposing)
            {
                if (!WasDisposed)
                {
                    if (disposing)
                    {
                        Close();
                        if (_reactor != null)
                        {
                            ((IDisposable) _reactor).Dispose();
                            _reactor = null;
                        }
                    }
                }
                WasDisposed = true;
            }

            #endregion
        }

        #endregion

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