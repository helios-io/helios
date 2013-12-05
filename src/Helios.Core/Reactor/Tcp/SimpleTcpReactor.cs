using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Helios.Core.Concurrency;
using Helios.Core.Exceptions;

namespace Helios.Core.Reactor.Tcp
{
    public class SimpleTcpReactor : ReactorBase
    {
        protected TcpListener Listener;
        protected ManualResetEventSlim ResetEvent;
        protected IFiber Fiber;

        public SimpleTcpReactor(IPAddress localAddress, int localPort) : this(localAddress, localPort, FiberFactory.CreateFiber(FiberMode.MaximumConcurrency)) { }

        public SimpleTcpReactor(IPAddress localAddress, int localPort, IFiber fiber)
        {
            ResetEvent = new ManualResetEventSlim();
            this.LocalEndpoint = new IPEndPoint(localAddress, localPort);
            Listener = new TcpListener(this.LocalEndpoint);
            Fiber = fiber;
        }

        public override bool IsActive { get; protected set; }
        public override void Start()
        {
            //Don't restart
            if (IsActive) return;

            CheckWasDisposed();
            IsActive = true;
            Listener.Start();
            EventLoop();
        }

        public override void Stop()
        {
            CheckWasDisposed();
            Listener.Stop();
            IsActive = false;
            ResetEvent.Set();
        }

        public virtual void EventLoop()
        {
            try
            {
                while (!ResetEvent.IsSet)
                {
                    var client = Listener.AcceptTcpClient();
                    Fiber.Add(() => InvokeAcceptConnection(client));
                }
            }
            catch (SocketException e)
            {

            }
        }

        public override event EventHandler<ReactorEventArgs> AcceptConnection = delegate { };

        protected virtual void InvokeAcceptConnection(TcpClient tcpClient)
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
                Fiber.Dispose();
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
