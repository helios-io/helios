using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Helios.Exceptions;
using Helios.Net;
using Helios.Ops;
using Helios.Topology;
using Helios.Util.Collections;

namespace Helios.Reactor.Response
{
    /// <summary>
    /// Wraps a remote endpoint which connected <see cref="IReactor"/> instance inside a <see cref="IConnection"/> object
    /// </summary>
    public abstract class ReactorResponseChannel : IConnection
    {
        protected ICircularBuffer<NetworkData> UnreadMessages = new ConcurrentCircularBuffer<NetworkData>(100);
        private readonly ReactorBase _reactor;
        internal readonly Socket Socket;

        protected ReactorResponseChannel(ReactorBase reactor, Socket outboundSocket, IEventLoop eventLoop) : this(reactor, outboundSocket, (IPEndPoint)outboundSocket.RemoteEndPoint, eventLoop)
        {
            
        }

        protected ReactorResponseChannel(ReactorBase reactor, Socket outboundSocket, IPEndPoint endPoint, IEventLoop eventLoop)
        {
            _reactor = reactor;
            Socket = outboundSocket;
            Local = reactor.LocalEndpoint.ToNode(reactor.Transport);
            RemoteHost = NodeBuilder.FromEndpoint(endPoint);
            EventLoop = eventLoop;
        }

        protected IEventLoop EventLoop;

        public ReceivedDataCallback Receive { get; set; }

        public event ConnectionEstablishedCallback OnConnection;
        public event ConnectionTerminatedCallback OnDisconnection;

        public DateTimeOffset Created { get; private set; }
        public INode RemoteHost { get; private set; }
        public INode Local { get; private set; }
        public TimeSpan Timeout { get { return TimeSpan.FromSeconds(Socket.ReceiveTimeout); } }
        public TransportType Transport { get{ if(Socket.ProtocolType == ProtocolType.Tcp){ return TransportType.Tcp; } return TransportType.Udp; } }
        public bool Blocking { get { return Socket.Blocking; } set { Socket.Blocking = value; } }
        public bool WasDisposed { get; private set; }
        public bool Receiving { get { return _reactor.IsActive; } }
        public bool IsOpen()
        {
            return Socket.Connected;
        }

        public int Available { get { return Socket.Available; } }
        public Task<bool> OpenAsync()
        {
            Open();
            return Task.Run(() => true);
        }

        public abstract void Configure(IConnectionConfig config);

        public void Open()
        {
            if (OnConnection != null)
            {
                OnConnection(RemoteHost, this);
            }
        }

        public void BeginReceive()
        {
            if (Receive == null) throw new NullReferenceException("Receive cannot be null");

            BeginReceiveInternal();
        }

        public void BeginReceive(ReceivedDataCallback callback)
        {
            Receive = callback;
            foreach (var msg in UnreadMessages.DequeueAll())
            {
                var msg1 = msg;
                EventLoop.Execute(() => Receive(msg1, this));
            }

            BeginReceiveInternal();
        }

        protected abstract void BeginReceiveInternal();


        /// <summary>
        /// Method is called directly by the <see cref="ReactorBase"/> implementation to send data to this <see cref="IConnection"/>.
        /// 
        /// Can also be called by the socket itself if this reactor doesn't use <see cref="ReactorProxyResponseChannel"/>.
        /// </summary>
        /// <param name="data">The data to pass directly to the recipient</param>

        internal virtual void OnReceive(NetworkData data)
        {
            if (Receive != null)
            {
                EventLoop.Execute(() => Receive(data, this));
            }
            else
            {
               UnreadMessages.Enqueue(data);
            }
        }

        public void StopReceive()
        {
            StopReceiveInternal();
            Receive = null;
        }


        protected abstract void StopReceiveInternal();

        public void Close()
        {
            _reactor.CloseConnection(RemoteHost);

            if (OnDisconnection != null)
            {
                OnDisconnection(RemoteHost, new HeliosConnectionException(ExceptionType.Closed));
            }
        }

        public virtual void Send(NetworkData payload)
        {
            _reactor.Send(payload.Buffer, RemoteHost);
        }

        public virtual async Task SendAsync(NetworkData payload)
        {
            await Task.Run(() => _reactor.Send(payload.Buffer, RemoteHost));
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
                
                if (disposing)
                {
                    Close();
                    if (Socket != null)
                    {
                        ((IDisposable)Socket).Dispose();
                    }
                }
            }
            WasDisposed = true;
        }

        #endregion
    }
}
