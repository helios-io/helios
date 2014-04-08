using System;
using System.Net;
using System.Threading.Tasks;
using Helios.Net;
using Helios.Topology;

namespace Helios.Reactor
{
    /// <summary>
    /// Wraps the <see cref="IReactor"/> itself inside a <see cref="IConnection"/> object and makes it callable
    /// directly to end users
    /// </summary>
    public class ReactorConnectionAdapter : IConnection
    {
        private ReactorBase _reactor;

        public ReactorConnectionAdapter(ReactorBase reactor)
        {
            _reactor = reactor;
            Local = _reactor.LocalEndpoint.ToNode(_reactor.Transport);
        }

        public ReceivedDataCallback Receive { get; private set; }

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
        public DateTimeOffset Created { get; private set; }
        public INode RemoteHost { get; private set; }
        public INode Local { get; private set; }
        public TimeSpan Timeout { get; private set; }
        public TransportType Transport { get { return _reactor.Transport; } }
        public bool Blocking { get { return _reactor.Blocking; } set { _reactor.Blocking = value; } }
        public bool WasDisposed { get; private set; }
        public bool Receiving { get { return _reactor.IsActive; } }
        public bool IsOpen()
        {
            return _reactor.IsActive;
        }

        public int Available { get{ throw new NotImplementedException("[Available] is not supported on ReactorConnectionAdapter"); } }

        public async Task<bool> OpenAsync()
        {
            await Task.Run(() => _reactor.Start());
            return true;
        }

        public void Open()
        {
            if (_reactor.IsActive) return;
            _reactor.Start();
        }

        public void BeginReceive(ReceivedDataCallback callback)
        {
            Receive = callback;
            _reactor.OnReceive += Receive;
        }

        public void StopReceive()
        {
            _reactor.OnReceive -= Receive;
        }

        public void Close()
        {
            _reactor.Stop();
        }

        public void Send(NetworkData payload)
        {
            _reactor.Send(payload.Buffer, payload.RemoteHost);
        }

        public async Task SendAsync(NetworkData payload)
        {
            await Task.Run(() => Send(payload));
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
                        ((IDisposable)_reactor).Dispose();
                        _reactor = null;
                    }
                }
            }
            WasDisposed = true;
        }

        #endregion
    }
}
