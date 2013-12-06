using System;
using System.Net;
using System.Threading;
using Helios.Core.Concurrency;
using Helios.Core.Exceptions;
using Helios.Core.Net;
using Helios.Core.Net.Connections;
using Helios.Core.Topology;

namespace Helios.Core.Reactor.Udp
{
    public class SimpleUdpReactor : ReactorBase, IConnectionlessReactor
    {
        protected ManualResetEventSlim ResetEvent;
        protected IFiber Fiber;
        protected IConnection Connection;

        public SimpleUdpReactor(IPAddress localAddress, int localPort) : 
            this(
            new UdpConnection(new Node() { Host = localAddress, Port = localPort, TransportType = TransportType.Udp}), 
            FiberFactory.CreateFiber(FiberMode.MaximumConcurrency)) { }

        public SimpleUdpReactor(IConnection udpConnection, IFiber fiber)
        {
            ResetEvent = new ManualResetEventSlim();
            Connection = udpConnection;
            this.LocalEndpoint = udpConnection.Node.ToEndPoint();
            Fiber = fiber;
        }
        public override bool IsActive { get; protected set; }
        public override void Start()
        {
            //Don't restart
            if (IsActive) return;

            CheckWasDisposed();
            IsActive = true;
            Connection.Open();
            EventLoop();
        }

        public override void Stop()
        {
            Connection.Close();
        }

        public virtual async void EventLoop()
        {
            var data = await Connection.RecieveAsync();
            Fiber.Add(() => InvokeDataAvailable(data));
        }

        public event EventHandler<ReactorReceivedDataEventArgs> DataAvailable = delegate { };

        protected virtual void InvokeDataAvailable(NetworkData data)
        {
            var h = DataAvailable;
            if (h == null) return;
            h(this, ReactorReceivedDataEventArgs.Create(data, Connection));
        }

        #region IDisposable Members

        public override void Dispose(bool disposing)
        {
            if (!WasDisposed && disposing && Connection != null)
            {
                Connection.Dispose();
                DataAvailable = delegate { };
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
