using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Helios.Buffers;
using Helios.Channels;
using Helios.Channels.Bootstrap;
using Helios.Channels.Sockets;
using Helios.Codecs;
using Helios.Logging;
using Helios.Tests.Performance.Channels;
using Helios.Util;
using Helios.Util.TimedOps;
using NBench;

namespace Helios.HorizontalScaling.Performance.Channels
{
    public class TcpServerSocketChannelHorizontalScaleSpec
    {
        static TcpServerSocketChannelHorizontalScaleSpec()
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

        private const string ClientConnectCounterName = "connected clients";
        private Counter _clientConnectedCounter;

        private byte[] message;
        private const string InboundThroughputCounterName = "inbound ops";
        private Counter _inboundThroughputCounter;

        private const string OutboundThroughputCounterName = "outbound ops";
        private Counter _outboundThroughputCounter;

        private IChannel _serverChannel;
        private ConcurrentBag<IChannel> _clientChannels;
        private CancellationTokenSource _shutdownBenchmark;
        protected readonly ManualResetEventSlim ResetEvent = new ManualResetEventSlim(false);

        public const int IterationCount = 3; // these are LONG-running benchmarks. stick with a lower iteration count

        // Sleep main thread, then start a new client every 30 ms
        private readonly TimeSpan SleepInterval = TimeSpan.FromMilliseconds(30);

        private IReadFinishedSignal _signal;
        
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
            ClientGroup = new MultithreadEventLoopGroup(Environment.ProcessorCount/2);
            ServerGroup = new MultithreadEventLoopGroup(1);
            WorkerGroup = new MultithreadEventLoopGroup(Environment.ProcessorCount/2);

            _shutdownBenchmark = new CancellationTokenSource();
            _clientChannels = new ConcurrentBag<IChannel>();

            Encoding iso = Encoding.GetEncoding("ISO-8859-1");
            message = iso.GetBytes("ABC");

            _inboundThroughputCounter = context.GetCounter(InboundThroughputCounterName);
            _outboundThroughputCounter = context.GetCounter(OutboundThroughputCounterName);
            _clientConnectedCounter = context.GetCounter(ClientConnectCounterName);
            var counterHandler = new CounterHandlerInbound(_inboundThroughputCounter);
            _signal = new ManualResetEventSlimReadFinishedSignal(ResetEvent);

            var sb = new ServerBootstrap().Group(ServerGroup, WorkerGroup).Channel<TcpServerSocketChannel>()
                .ChildOption(ChannelOption.TcpNodelay, true)
                .ChildHandler(new ActionChannelInitializer<TcpSocketChannel>(channel =>
                {
                    channel.Pipeline.AddLast(GetEncoder()).AddLast(GetDecoder()).AddLast(counterHandler).AddLast(new CounterHandlerOutbound(_outboundThroughputCounter));
                }));

            var cb = new ClientBootstrap().Group(ClientGroup)
                .Option(ChannelOption.TcpNodelay, true)
                .Channel<TcpSocketChannel>().Handler(new ActionChannelInitializer<TcpSocketChannel>(
                channel =>
                {
                    channel.Pipeline.AddLast(GetEncoder()).AddLast(GetDecoder()).AddLast(counterHandler)
                        .AddLast(new CounterHandlerOutbound(_outboundThroughputCounter));
                }));

            var token = _shutdownBenchmark.Token;
            EventLoop = () =>
            {
                while (!token.IsCancellationRequested)
                {
                    foreach (var channel in _clientChannels)
                    {
                        // unrolling a loop
                        channel.WriteAsync(Unpooled.WrappedBuffer(message));
                        channel.WriteAsync(Unpooled.WrappedBuffer(message));
                        channel.WriteAsync(Unpooled.WrappedBuffer(message));
                        channel.WriteAsync(Unpooled.WrappedBuffer(message));
                        channel.WriteAsync(Unpooled.WrappedBuffer(message));
                        channel.WriteAsync(Unpooled.WrappedBuffer(message));
                        channel.WriteAsync(Unpooled.WrappedBuffer(message));
                        channel.WriteAsync(Unpooled.WrappedBuffer(message));
                        channel.WriteAsync(Unpooled.WrappedBuffer(message));
                        channel.Flush();
                    }

                    // sleep for a tiny bit, then get going again
                    Thread.Sleep(10);
                }
            };

            // start server
            _serverChannel = sb.BindAsync(TEST_ADDRESS).Result;

            // connect to server with 1 client initially
            _clientChannels.Add(cb.ConnectAsync(_serverChannel.LocalAddress).Result);
            
        }

        private Action EventLoop;

        [PerfBenchmark(Description = "Measures how quickly and with how much GC overhead a TcpSocketChannel --> TcpServerSocketChannel connection can decode / encode realistic messages",
            NumberOfIterations = IterationCount, RunMode = RunMode.Iterations)]
        [CounterMeasurement(InboundThroughputCounterName)]
        [CounterMeasurement(OutboundThroughputCounterName)]
        [CounterMeasurement(ClientConnectCounterName)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        public void TcpServerSocketChannel_horizontal_scale(BenchmarkContext context)
        {
            _clientConnectedCounter.Increment(); // for the initial client
            var totalRunSeconds = TimeSpan.FromSeconds(ThreadLocalRandom.Current.Next(180, 360)); // 3-6 minutes
            var deadline = new PreciseDeadline(totalRunSeconds);
            var task = Task.Factory.StartNew(EventLoop); // start writing
            while (!deadline.IsOverdue)
            {
                // add a new client
                _clientChannels.Add(ClientBootstrap.ConnectAsync(_serverChannel.LocalAddress).Result);
                _clientConnectedCounter.Increment();
                Thread.Sleep(SleepInterval);
            }
            _shutdownBenchmark.Cancel();
        }


        [PerfCleanup]
        public void TearDown()
        {
            EventLoop = null;
            var shutdownTasks = new List<Task>();
            foreach (var channel in _clientChannels)
            {
                shutdownTasks.Add(channel.CloseAsync());
            }
            Task.WaitAll(shutdownTasks.ToArray());
            CloseChannel(_serverChannel);
            Task.WaitAll(ClientGroup.ShutdownGracefullyAsync(), ServerGroup.ShutdownGracefullyAsync(), WorkerGroup.ShutdownGracefullyAsync());
        }

        private static void CloseChannel(IChannel cc)
        {
            cc?.CloseAsync().Wait();
        }
    }
}
