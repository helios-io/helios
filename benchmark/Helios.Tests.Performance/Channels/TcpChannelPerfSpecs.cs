using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Helios.Buffers;
using Helios.Channels;
using Helios.Channels.Bootstrap;
using Helios.Channels.Local;
using Helios.Channels.Sockets;
using Helios.Codecs;
using Helios.Concurrency;
using Helios.Logging;
using NBench;

namespace Helios.Tests.Performance.Channels
{
    public class TcpChannelPerfSpecs
    {
        static TcpChannelPerfSpecs()
        {
            // Disable the logging factory
            LoggingFactory.DefaultFactory = new NoOpLoggerFactory();
        }

        private static readonly IPEndPoint TEST_ADDRESS = new IPEndPoint(IPAddress.IPv6Loopback, 0);

        protected ClientBootstrap ClientBootstrap;
        protected ServerBootstrap ServerBoostrap;

        protected IEventLoopGroup ClientGroup;
        protected IEventLoopGroup WorkerGroup;
        protected IEventLoopGroup ServerGroup;

        private byte[] message;
        private const string InboundThroughputCounterName = "inbound ops";
        private Counter _inboundThroughputCounter;

        private const string OutboundThroughputCounterName = "outbound ops";
        private Counter _outboundThroughputCounter;

        private IChannel _serverChannel;
        private IChannel _clientChannel;
        protected readonly ManualResetEventSlim ResetEvent = new ManualResetEventSlim(false);

        public const int IterationCount = 5;
        public const int WriteCount = 1000000;
        public const int MessagesPerMinute = 1000000;
        public TimeSpan Timeout = TimeSpan.FromMinutes((double)WriteCount / MessagesPerMinute);

        private IReadFinishedSignal _signal;

        private IByteBuf[] messages = new IByteBuf[WriteCount];

        protected virtual IChannelHandler GetEncoder()
        {
            return new LengthFieldPrepender(4, false);
        }

        protected virtual IChannelHandler GetDecoder()
        {
            return new LengthFieldBasedFrameDecoder(Int32.MaxValue, 0, 4, 0, 4);
        }

        [PerfSetup]
        public void SetUp(BenchmarkContext context)
        {
            ClientGroup = new MultithreadEventLoopGroup(1);
            ServerGroup = new MultithreadEventLoopGroup(1);
            WorkerGroup = new MultithreadEventLoopGroup();
            

            Encoding iso = Encoding.GetEncoding("ISO-8859-1");
            message = iso.GetBytes("ABC");

            // pre-allocate all messages
            foreach (var m in Enumerable.Range(0, WriteCount))
            {
                messages[m] = Unpooled.WrappedBuffer(message);
            }

            _inboundThroughputCounter = context.GetCounter(InboundThroughputCounterName);
            _outboundThroughputCounter = context.GetCounter(OutboundThroughputCounterName);
            var counterHandler = new CounterHandlerInbound(_inboundThroughputCounter);
            _signal = new ManualResetEventSlimReadFinishedSignal(ResetEvent);

            var sb = new ServerBootstrap().Group(ServerGroup, WorkerGroup).Channel<TcpServerSocketChannel>()
                .ChildOption(ChannelOption.TcpNodelay, true)
                .ChildHandler(new ActionChannelInitializer<TcpSocketChannel>(channel =>
                {
                    channel.Pipeline.AddLast(GetEncoder()).AddLast(GetDecoder()).AddLast(counterHandler).AddLast(new CounterHandlerOutbound(_outboundThroughputCounter)).AddLast(new ReadFinishedHandler(_signal, WriteCount));
                }));

            var cb = new ClientBootstrap().Group(ClientGroup)
                .Option(ChannelOption.TcpNodelay, true)
                .Channel<TcpSocketChannel>().Handler(new ActionChannelInitializer<TcpSocketChannel>(
                channel =>
                {
                    channel.Pipeline.AddLast(GetEncoder()).AddLast(GetDecoder()).AddLast(counterHandler)
                        .AddLast(new CounterHandlerOutbound(_outboundThroughputCounter));
                }));

            // start server
            _serverChannel = sb.BindAsync(TEST_ADDRESS).Result;

            // connect to server
            _clientChannel = cb.ConnectAsync(_serverChannel.LocalAddress).Result;
            //_clientChannel.Configuration.AutoRead = false;
        }

        [PerfBenchmark(Description = "Measures how quickly and with how much GC overhead a TcpSocketChannel --> TcpServerSocketChannel connection can decode / encode realistic messages",
            NumberOfIterations = IterationCount, RunMode = RunMode.Iterations)]
        [CounterMeasurement(InboundThroughputCounterName)]
        [CounterMeasurement(OutboundThroughputCounterName)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        public void TcpChannel_Duplex_Throughput(BenchmarkContext context)
        {
            for (var i = 0; i < WriteCount; i++)
            {
                _clientChannel.WriteAsync(messages[i]);
                if (i % 100 == 0) // flush every 100 writes
                    _clientChannel.Flush();
            }
            _clientChannel.Flush();
            ResetEvent.Wait(Timeout);
        }


        [PerfCleanup]
        public void TearDown()
        {
            CloseChannel(_clientChannel);
            CloseChannel(_serverChannel);
            Task.WaitAll(ClientGroup.ShutdownGracefullyAsync(), ServerGroup.ShutdownGracefullyAsync(), WorkerGroup.ShutdownGracefullyAsync());
        }

        private static void CloseChannel(IChannel cc)
        {
            cc?.CloseAsync().Wait();
        }
    }
}
