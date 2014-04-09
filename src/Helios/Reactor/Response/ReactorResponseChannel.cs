using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Helios.Exceptions;
using Helios.Net;
using Helios.Ops;
using Helios.Topology;

namespace Helios.Reactor.Response
{
    /// <summary>
    /// Wraps a remote endpoint which connected <see cref="IReactor"/> instance inside a <see cref="IConnection"/> object
    /// </summary>
    public abstract class ReactorResponseChannel : IConnection
    {
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

        public ReceivedDataCallback Receive { get; private set; }

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

        public void Open()
        {
            if (OnConnection != null)
            {
                OnConnection(RemoteHost);
            }
        }

        public void BeginReceive(ReceivedDataCallback callback)
        {
            Receive = callback;
            BeginReceiveInternal();
        }

        protected abstract void BeginReceiveInternal();

        /// <summary>
        /// Received data from the network
        /// </summary>
        protected void OnReceive(NetworkData data)
        {
            if (Receive != null)
            {
                EventLoop.Execute(() => Receive(data, this));
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

    public class TcpReactorResponseChannel : ReactorResponseChannel
    {
        /// <summary>
        /// shared buffer used by all incoming connections
        /// </summary>
        protected byte[] Buffer;

        public TcpReactorResponseChannel(ReactorBase reactor, Socket outboundSocket, IEventLoop eventLoop, int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE) : this(reactor, outboundSocket, (IPEndPoint)outboundSocket.RemoteEndPoint, eventLoop, bufferSize)
        {
        }

        public TcpReactorResponseChannel(ReactorBase reactor, Socket outboundSocket, IPEndPoint endPoint, IEventLoop eventLoop, int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE)
            : base(reactor, outboundSocket, endPoint, eventLoop)
        {
        }

        protected override void BeginReceiveInternal()
        {
            //Socket.BeginReceive()
        }

        protected override void StopReceiveInternal()
        {
            throw new NotImplementedException();
        }

        public override void Send(NetworkData payload)
        {
            base.Send(payload);
        }

        public override Task SendAsync(NetworkData payload)
        {
            return base.SendAsync(payload);
        }
    }
}
