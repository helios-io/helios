using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Helios.Net;
using Helios.Net.Bootstrap;
using Helios.Ops;
using Helios.Reactor.Bootstrap;
using Helios.Util.Collections;
using NUnit.Framework;

namespace Helios.MultiNodeTests.TestKit
{
    [TestFixture]
    public abstract class MultiNodeTest<T>
    {
        public abstract TransportType TransportType { get; }

        [SetUp]
        public void SetUp()
        {
            clientExecutor = new AssertExecutor();
            serverExecutor = new AssertExecutor();
            var serverBootstrap = new ServerBootstrap()
                   .WorkerThreads(2)
                   .Executor(serverExecutor)
                   .SetTransport(TransportType)
                   .Build();

            var clientBootstrap = new ClientBootstrap
        }

        public void CleanUp()
        {
            
        }

        private IExecutor clientExecutor;
        private IExecutor serverExecutor;

        protected ConcurrentCircularBuffer<byte[]> SendBuffer { get; private set; }

        protected ConcurrentCircularBuffer<byte[]> ReceiveBuffer { get; private set; }

        private IConnection Client;

        private IConnection Server;
    }
}
