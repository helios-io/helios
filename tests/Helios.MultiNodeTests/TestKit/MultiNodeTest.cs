using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Helios.Buffers;
using Helios.Net;
using Helios.Net.Bootstrap;
using Helios.Ops;
using Helios.Reactor.Bootstrap;
using Helios.Serialization;
using Helios.Topology;
using Helios.Util;
using Helios.Util.Collections;
using NUnit.Framework;

namespace Helios.MultiNodeTests.TestKit
{
    [TestFixture]
    public abstract class MultiNodeTest
    {
        public abstract TransportType TransportType { get; }

        /// <summary>
        /// Disables message capture and resorts to just using counters
        /// </summary>
        public virtual bool HighPerformance { get { return false; } }        

        public virtual int BufferSize { get { return 1024; } }

        public AtomicCounter ClientReceived { get; protected set; }

        public AtomicCounter ServerReceived { get; protected set; }

        public virtual IMessageEncoder Encoder { get { return Encoders.DefaultEncoder; } }

        public virtual IMessageDecoder Decoder { get { return Encoders.DefaultDecoder; } }

        public virtual IByteBufAllocator Allocator { get { return UnpooledByteBufAllocator.Default; } }

        public virtual IConnectionConfig Config { get { return new DefaultConnectionConfig(); } }

        private IConnectionFactory _clientConnectionFactory;

        [SetUp]
        public void SetUp()
        {
            if (!HighPerformance)
            {
                ClientSendBuffer = new ConcurrentCircularBuffer<NetworkData>(BufferSize);
                ClientReceiveBuffer = new ConcurrentCircularBuffer<NetworkData>(BufferSize);
                ServerReceiveBuffer = new ConcurrentCircularBuffer<NetworkData>(BufferSize);
            }
            ClientReceived = new AtomicCounter(0);
            ServerReceived = new AtomicCounter(0);
           

            _clientExecutor = new AssertExecutor();
            _serverExecutor = new AssertExecutor();
            var serverBootstrap = new ServerBootstrap()
                   .WorkerThreads(2)
                   .Executor(_serverExecutor)
                   .SetTransport(TransportType)
                   .SetEncoder(Encoder)
                   .SetDecoder(Decoder)
                   .SetAllocator(Allocator)
                   .SetConfig(Config)
                   .Build();

            _server = serverBootstrap.NewConnection(Node.Loopback());

            _clientConnectionFactory = new ClientBootstrap()
                .Executor(_clientExecutor)
                .SetTransport(TransportType)
                .SetEncoder(Encoder)
                .SetDecoder(Decoder)
                .SetAllocator(Allocator)
                .SetConfig(Config)
                .Build();
        }

        [TearDown]
        public void CleanUp()
        {
            _client.Close();
            _server.Close();
            _client = null;
            _server = null;
        }

        protected void StartServer()
        {
            StartServer((data, channel) =>
            {
                if (!HighPerformance)
                {
                    ServerReceiveBuffer.Add(data);
                }
                ServerReceived.GetAndIncrement();
                channel.Send(new NetworkData() { Buffer = data.Buffer, Length = data.Length, RemoteHost = channel.RemoteHost });
            });
        }

        /// <summary>
        /// Used to start the server with a specific receive data callback
        /// </summary>
        protected void StartServer(ReceivedDataCallback callback)
        {
            _server.Receive += (data, channel) =>
            {
                callback(data, channel);
            };
            _server.OnConnection += (address, channel) =>
            {
                channel.BeginReceive();
            };
            _server.OnError += (exception, connection) => _serverExecutor.Exceptions.Add(exception);
            _server.Open();
        }

        protected void StartClient()
        {
            if (!_server.IsOpen()) throw new HeliosException("Server is not started yet. Cannot start client yet.");
            _client = _clientConnectionFactory.NewConnection(_server.Local);
            _client.Receive += (data, channel) =>
            {
                if (!HighPerformance)
                {
                    ClientReceiveBuffer.Add(data);
                }
                ClientReceived.GetAndIncrement();
            };
            _client.OnConnection += (address, channel) => channel.BeginReceive();
            _client.OnError += (exception, connection) => _clientExecutor.Exceptions.Add(exception);
            _client.Open();
        }

        protected void Send(byte[] data)
        {
            if (_client == null)
                StartClient();
            var networkData = NetworkData.Create(_server.Local, data, data.Length);
            if(!HighPerformance)
                ClientSendBuffer.Add(networkData);
            _client.Send(networkData);
        }

        protected void WaitUntilNMessagesReceived(int count)
        {
            WaitUntilNMessagesReceived(count, TimeSpan.FromSeconds(5));
        }


        protected void WaitUntilNMessagesReceived(int count, TimeSpan timeout)
        {
            SpinWait.SpinUntil(() => ClientReceived.Current >= count, timeout);
        }

        protected Exception[] ClientExceptions { get { return _clientExecutor.Exceptions.ToArray(); } }
        protected Exception[] ServerExceptions { get { return _serverExecutor.Exceptions.ToArray(); } }

        private AssertExecutor _clientExecutor;
        private AssertExecutor _serverExecutor;

        protected ConcurrentCircularBuffer<NetworkData> ClientSendBuffer { get; private set; }

        protected ConcurrentCircularBuffer<NetworkData> ClientReceiveBuffer { get; private set; }

        protected ConcurrentCircularBuffer<NetworkData> ServerReceiveBuffer { get; private set; }

        private IConnection _client;

        private IConnection _server;
    }
}
