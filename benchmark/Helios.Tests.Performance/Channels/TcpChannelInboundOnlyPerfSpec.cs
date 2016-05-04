// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Helios.Buffers;
using Helios.Channels;
using Helios.Channels.Bootstrap;
using Helios.Channels.Sockets;
using Helios.Codecs;
using Helios.Logging;
using NBench;

namespace Helios.Tests.Performance.Channels
{
    public class TcpChannelInboundOnlyPerfSpec
    {
        private const string InboundThroughputCounterName = "inbound ops";

        private const string OutboundThroughputCounterName = "outbound ops";

        public const int IterationCount = 5;
        public const int WriteCount = 1000000;
        public const int MessagesPerMinute = 1000000;

        private static readonly IPEndPoint TEST_ADDRESS = new IPEndPoint(IPAddress.IPv6Loopback, 0);
        protected readonly ManualResetEventSlim ResetEvent = new ManualResetEventSlim(false);
        private Counter _inboundThroughputCounter;
        private Counter _outboundThroughputCounter;

        private IChannel _serverChannel;

        private IReadFinishedSignal _signal;

        protected System.Net.Sockets.Socket ClientSocket;

        private byte[] message;

        private IByteBuf[] messages = new IByteBuf[WriteCount];

        protected ServerBootstrap ServerBoostrap;
        protected IEventLoopGroup ServerGroup;
        protected NetworkStream Stream;
        public TimeSpan Timeout = TimeSpan.FromMinutes((double) WriteCount/MessagesPerMinute);
        protected IEventLoopGroup WorkerGroup;

        static TcpChannelInboundOnlyPerfSpec()
        {
            // Disable the logging factory
            LoggingFactory.DefaultFactory = new NoOpLoggerFactory();
        }

        protected virtual IChannelHandler GetEncoder()
        {
            return new LengthFieldPrepender(4, false);
        }

        protected virtual IChannelHandler GetDecoder()
        {
            return new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4);
        }

        [PerfSetup]
        public void SetUp(BenchmarkContext context)
        {
            ServerGroup = new MultithreadEventLoopGroup(1);
            WorkerGroup = new MultithreadEventLoopGroup();


            var iso = Encoding.GetEncoding("ISO-8859-1");
            message = Unpooled.Buffer().WriteInt(3).WriteBytes(iso.GetBytes("ABC")).ToArray();


            _inboundThroughputCounter = context.GetCounter(InboundThroughputCounterName);
            _outboundThroughputCounter = context.GetCounter(OutboundThroughputCounterName);
            var counterHandler = new CounterHandlerInbound(_inboundThroughputCounter);
            _signal = new ManualResetEventSlimReadFinishedSignal(ResetEvent);

            var sb = new ServerBootstrap().Group(ServerGroup, WorkerGroup).Channel<TcpServerSocketChannel>()
                .ChildOption(ChannelOption.TcpNodelay, true)
                .ChildHandler(
                    new ActionChannelInitializer<TcpSocketChannel>(
                        channel =>
                        {
                            channel.Pipeline.AddLast(GetEncoder())
                                .AddLast(GetDecoder())
                                .AddLast(counterHandler)
                                .AddLast(new CounterHandlerOutbound(_outboundThroughputCounter))
                                .AddLast(new ReadFinishedHandler(_signal, WriteCount));
                        }));

            // start server
            _serverChannel = sb.BindAsync(TEST_ADDRESS).Result;

            // connect to server
            var address = (IPEndPoint) _serverChannel.LocalAddress;
            ClientSocket = new System.Net.Sockets.Socket(AddressFamily.InterNetworkV6, SocketType.Stream,
                ProtocolType.Tcp);
            ClientSocket.Connect(address.Address, address.Port);

            Stream = new NetworkStream(ClientSocket, true);
        }

        [PerfBenchmark(
            Description =
                "Measures how quickly and with how much GC overhead a TcpSocketChannel --> TcpServerSocketChannel connection can decode / encode realistic messages",
            NumberOfIterations = IterationCount, RunMode = RunMode.Iterations)]
        [CounterMeasurement(InboundThroughputCounterName)]
        [CounterMeasurement(OutboundThroughputCounterName)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        public void TcpChannel_Duplex_Throughput(BenchmarkContext context)
        {
            for (var i = 0; i < WriteCount; i++)
            {
                Stream.Write(message, 0, message.Length);
            }
            ResetEvent.Wait(Timeout);
        }


        [PerfCleanup]
        public void TearDown()
        {
            try
            {
                Stream.Close();
            }
            finally
            {
                CloseChannel(_serverChannel);
                Task.WaitAll(ServerGroup.ShutdownGracefullyAsync(), WorkerGroup.ShutdownGracefullyAsync());
            }
        }

        private static void CloseChannel(IChannel cc)
        {
            cc?.CloseAsync().Wait();
        }
    }
}

