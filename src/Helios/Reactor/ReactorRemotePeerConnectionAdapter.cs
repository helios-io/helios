using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Helios.Net;
using Helios.Topology;

namespace Helios.Reactor
{
    /// <summary>
    /// Wraps a remote endpoint which connected <see cref="IReactor"/> instance inside a <see cref="IConnection"/> object
    /// </summary>
    public class ReactorRemotePeerConnectionAdapter : IConnection
    {
        private readonly ReactorBase _reactor;
        internal readonly Socket Socket;

        public ReactorRemotePeerConnectionAdapter(ReactorBase reactor, Socket outboundSocket) : this(reactor, outboundSocket, (IPEndPoint)outboundSocket.RemoteEndPoint)
        {
            
        }

        public ReactorRemotePeerConnectionAdapter(ReactorBase reactor, Socket outboundSocket, IPEndPoint endPoint)
        {
            _reactor = reactor;
            Socket = outboundSocket;
            Local = reactor.LocalEndpoint.ToNode(reactor.Transport);
            RemoteHost = NodeBuilder.FromEndpoint(endPoint);
        }

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
            return Task.Run(() => true);
        }

        public void Open()
        {
            //NO-OP
        }

        public void BeginReceive(ReceivedDataCallback callback)
        {
            //NO-OP
        }

        public void StopReceive()
        {
            //NO-OP
        }

        public void Close()
        {
            _reactor.CloseConnection(RemoteHost);
        }

        public void Send(NetworkData payload)
        {
            _reactor.Send(payload.Buffer, RemoteHost);
        }

        public async Task SendAsync(NetworkData payload)
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
