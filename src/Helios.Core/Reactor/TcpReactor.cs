using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Helios.Core.Exceptions;
using Helios.Core.Util.Concurrency;

namespace Helios.Core.Reactor
{
    public class TcpReactor : ReactorBase
    {
        protected TcpListener Listener;
        protected ManualResetEventSlim ResetEvent;


        public TcpReactor(IPAddress localAddress, int localPort)
        {
            ResetEvent = new ManualResetEventSlim();
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
            EventLoop();
        }

        public override void Stop()
        {
            CheckWasDisposed();
            Listener.Stop();
            ResetEvent.Set();
        }

        public void EventLoop()
        {
            try
            {
                while (!ResetEvent.IsSet)
                {
                    var client = Listener.AcceptTcpClient();
                    TaskRunner.Run(() => InvokeAcceptConnection(client));
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
