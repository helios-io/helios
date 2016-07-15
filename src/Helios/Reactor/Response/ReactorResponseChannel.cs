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
using Helios.Serialization;
using Helios.Topology;
using Helios.Util.Collections;
using Helios.Util.Concurrency;

namespace Helios.Reactor.Response
{
    /// <summary>
    ///     Wraps a remote endpoint which connected <see cref="IReactor" /> instance inside a <see cref="IConnection" /> object
    /// </summary>
    public abstract class ReactorResponseChannel : IConnection
    {
        private readonly ReactorBase _reactor;
        internal Socket Socket;

        protected ICircularBuffer<NetworkData> UnreadMessages = new ConcurrentCircularBuffer<NetworkData>(1000);

        protected ReactorResponseChannel(ReactorBase reactor, Socket outboundSocket, NetworkEventLoop eventLoop)
            : this(reactor, outboundSocket, (IPEndPoint) outboundSocket.RemoteEndPoint, eventLoop)
        {
        }

        protected ReactorResponseChannel(ReactorBase reactor, Socket outboundSocket, IPEndPoint endPoint,
            NetworkEventLoop eventLoop)
        {
            _reactor = reactor;
            Socket = outboundSocket;
            Decoder = _reactor.Decoder.Clone();
            Encoder = _reactor.Encoder.Clone();
            Allocator = _reactor.Allocator;
            Local = reactor.LocalEndpoint.ToNode(reactor.Transport);
            RemoteHost = NodeBuilder.FromEndpoint(endPoint);
            NetworkEventLoop = eventLoop;
        }

        public NetworkEventLoop NetworkEventLoop { get; }


        public event ReceivedDataCallback Receive
        {
            add { NetworkEventLoop.Receive = value; }
            // ReSharper disable once ValueParameterNotUsed
            remove { NetworkEventLoop.Receive = null; }
        }

        public event ConnectionEstablishedCallback OnConnection
        {
            add { NetworkEventLoop.Connection = value; }
            // ReSharper disable once ValueParameterNotUsed
            remove { NetworkEventLoop.Connection = null; }
        }

        public event ConnectionTerminatedCallback OnDisconnection
        {
            add { NetworkEventLoop.Disconnection = value; }
            // ReSharper disable once ValueParameterNotUsed
            remove { NetworkEventLoop.Disconnection = null; }
        }

        public event ExceptionCallback OnError
        {
            add { NetworkEventLoop.SetExceptionHandler(value, this); }
            // ReSharper disable once ValueParameterNotUsed
            remove { NetworkEventLoop.SetExceptionHandler(null, this); }
        }

        public IEventLoop EventLoop
        {
            get { return NetworkEventLoop; }
        }

        public IMessageEncoder Encoder { get; }
        public IMessageDecoder Decoder { get; }
        public IByteBufAllocator Allocator { get; }

        public DateTimeOffset Created { get; private set; }
        public INode RemoteHost { get; }
        public INode Local { get; }

        public TimeSpan Timeout
        {
            get { return TimeSpan.FromSeconds(Socket.ReceiveTimeout); }
        }

        public TransportType Transport
        {
            get
            {
                if (Socket.ProtocolType == ProtocolType.Tcp)
                {
                    return TransportType.Tcp;
                }
                return TransportType.Udp;
            }
        }

        public bool Blocking
        {
            get { return Socket.Blocking; }
            set { Socket.Blocking = value; }
        }

        public bool WasDisposed { get; private set; }

        public bool Receiving
        {
            get { return _reactor.IsActive; }
        }

        public bool IsOpen()
        {
            return Socket != null && Socket.Connected;
        }

        public int Available
        {
            get { return Socket == null ? 0 : Socket.Available; }
        }

        public int MessagesInSendQueue
        {
            get { return 0; }
        }

        public Task<bool> OpenAsync()
        {
            Open();
            return TaskRunner.Run(() => true);
        }

        public abstract void Configure(IConnectionConfig config);

        public void Open()
        {
            if (NetworkEventLoop.Connection != null)
            {
                NetworkEventLoop.Connection(RemoteHost, this);
            }
        }

        public void BeginReceive()
        {
            if (NetworkEventLoop.Receive == null) throw new NullReferenceException("Receive cannot be null");

            BeginReceiveInternal();
        }

        public void BeginReceive(ReceivedDataCallback callback)
        {
            Receive += callback;
            foreach (var msg in UnreadMessages.DequeueAll())
            {
                NetworkEventLoop.Receive(msg, this);
            }

            BeginReceiveInternal();
        }

        public void StopReceive()
        {
            StopReceiveInternal();
            NetworkEventLoop.Receive = null;
        }

        public void Close()
        {
            _reactor.CloseConnection(this);

            if (NetworkEventLoop.Disconnection != null)
            {
                NetworkEventLoop.Disconnection(new HeliosConnectionException(ExceptionType.Closed), this);
            }
            EventLoop.Dispose();
        }

        public virtual void Send(NetworkData data)
        {
            _reactor.Send(data);
        }

        public void Send(byte[] buffer, int index, int length, INode destination)
        {
            _reactor.Send(buffer, index, length, destination);
        }

        protected abstract void BeginReceiveInternal();


        /// <summary>
        ///     Method is called directly by the <see cref="ReactorBase" /> implementation to send data to this
        ///     <see cref="IConnection" />.
        ///     Can also be called by the socket itself if this reactor doesn't use <see cref="ReactorProxyResponseChannel" />.
        /// </summary>
        /// <param name="data">The data to pass directly to the recipient</param>
        internal virtual void OnReceive(NetworkData data)
        {
            if (NetworkEventLoop.Receive != null)
            {
                NetworkEventLoop.Receive(data, this);
            }
            else
            {
                UnreadMessages.Enqueue(data);
            }
        }


        protected abstract void StopReceiveInternal();

        public void InvokeReceiveIfNotNull(NetworkData data)
        {
            OnReceive(data);
        }

        protected void InvokeErrorIfNotNull(Exception ex)
        {
            if (NetworkEventLoop.Exception != null)
            {
                NetworkEventLoop.Exception(ex, this);
            }
            else
            {
                throw new HeliosException("Unhandled exception on a connection with no error handler", ex);
            }
        }

        #region IDisposable members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!WasDisposed)
            {
                WasDisposed = true;
                if (disposing)
                {
                    Close();
                    Socket = null;
                    EventLoop.Dispose();
                }
            }
        }

        #endregion
    }
}