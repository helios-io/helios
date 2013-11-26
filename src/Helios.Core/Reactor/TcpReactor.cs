using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Helios.Core.Net.Exceptions;
using Helios.Core.Util.Concurrency;

namespace Helios.Core.Reactor
{
    public class TcpReactor : ReactorBase
    {
        protected TcpListener Listener;
        protected CancellationTokenSource TokenSource;
        protected Task EventLoopThread;

        public TcpReactor(IPAddress localAddress, int localPort)
        {
            TokenSource = new CancellationTokenSource();
            this.LocalEndpoint = new IPEndPoint(localAddress, localPort);
           Listener = new TcpListener(this.LocalEndpoint);
        }

        public override bool IsActive { get; protected set; }
        public override void Start()
        {
            //Don't restart
            if(IsActive) return;

            CheckWasDisposed();
            Listener.Start();
            EventLoopThread = TaskRunner.Run(EventLoop, TokenSource.Token);
        }

        public override void Stop()
        {
            CheckWasDisposed();
            try
            {
                TokenSource.Cancel();
                EventLoopThread = null;
            }
            catch (AggregateException ex)
            {
                Debug.Write(ex.Flatten());
            }
            Listener.Stop();
        }

        public void EventLoop()
        {
            try
            {
                while (true)
                {
                    var client = Listener.AcceptTcpClient();
                    InvokeAcceptConnection(client);
                }
            }
            catch (SocketException e)
            {
                
            }
        }

        public override event EventHandler<ReactorEventArgs> AcceptConnection = delegate {};

        private void InvokeAcceptConnection(TcpClient tcpClient)
        {
            var h = AcceptConnection;
            if (h == null) return;
            h(this, ReactorEventArgs.Create(tcpClient));
        }

        #region IDisposable Members

        public override void Dispose(bool disposing)
        {
            if (!WasDisposed && disposing && Listener != null)
            {
                Listener.Stop();
                AcceptConnection = delegate { };
            }

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
