using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Helios.Buffers;
using Helios.Channels;
using Helios.Channels.Bootstrap;
using Helios.Channels.Local;
using Helios.Codecs;
using Helios.Concurrency;
using Helios.Logging;
using Helios.Util;
using NBench;

namespace Helios.Tests.Performance.Channels
{
    /// <summary>
    /// End-to-end performance benchmark for a realistic-ish pipeline built using the <see cref="LocalChannel"/>
    /// 
    /// Contains a total of three handlers and runs on the same thread as the caller, so all calls made against the pipeline
    /// are totally synchronous. Tests the overhead of the following components working together:
    /// 
    /// 1. <see cref="IChannelPipeline"/> default implementation
    /// 2. <see cref="IChannelHandlerContext"/> default implementation
    /// 3. <see cref="IRecvByteBufAllocator"/> default implementation
    /// 4. <see cref="IByteBufAllocator"/> default implementation 
    /// 5. <see cref="ChannelOutboundBuffer"/>
    /// 6. <see cref="ObjectPool{T}"/> and <see cref="RecyclableArrayList"/>
    /// 7. <see cref="AbstractChannel"/> and its built-in <see cref="IChannelUnsafe"/> implementation
    /// 8. <see cref="LengthFieldPrepender"/> and <see cref="LengthFieldBasedFrameDecoder"/>
    /// 9. And finally, <see cref="AbstractScheduledEventExecutor"/>.
    /// 
    /// Buffer size for each individual written message is intentionally kept small, since this is a throughput and GC overhead test.
    /// </summary>
    public class LocalChannelPerfSpecs
    {
        static LocalChannelPerfSpecs()
        {
            // Disable the logging factory
            LoggingFactory.DefaultFactory = new NoOpLoggerFactory();
        }

        private static readonly LocalAddress TEST_ADDRESS = new LocalAddress("test.id");

        protected ClientBootstrap ClientBootstrap;
        protected ServerBootstrap ServerBoostrap;

        protected IEventLoopGroup ClientGroup;
        protected IEventLoopGroup ServerGroup;

        private byte[] message;
        private const string InboundThroughputCounterName = "inbound ops";
        private Counter _inboundThroughputCounter;

        private const string OutboundThroughputCounterName = "outbound ops";
        private Counter _outboundThroughputCounter;

        private IChannel _serverChannel;
        private IChannel _clientChannel;
        protected readonly ManualResetEventSlim ResetEvent = new ManualResetEventSlim();

        public const int WriteCount = 10;
        private IReadFinishedSignal _signal;

        protected virtual IChannelHandler GetEncoder()
        {
            return new LengthFieldPrepender(4, false);
        }

        protected virtual IChannelHandler GetDecoder()
        {
            return new LengthFieldBasedFrameDecoder(20, 0, 4, 0, 4);
        }

        [PerfSetup]
        public void SetUp(BenchmarkContext context)
        {
            ClientGroup = new MultithreadEventLoopGroup(1);
            ServerGroup = new MultithreadEventLoopGroup(2);

            Encoding iso = Encoding.GetEncoding("ISO-8859-1");
            message = iso.GetBytes("ABC");

            _inboundThroughputCounter = context.GetCounter(InboundThroughputCounterName);
            _outboundThroughputCounter = context.GetCounter(OutboundThroughputCounterName);
            var counterHandler = new CounterHandlerInbound(_inboundThroughputCounter);
            _signal = new SimpleReadFinishedSignal();

            var sb = new ServerBootstrap().Group(ServerGroup).Channel<LocalServerChannel>()
                .ChildHandler(new ActionChannelInitializer<LocalChannel>(channel =>
                {
                    channel.Pipeline.AddLast(GetEncoder()).AddLast(GetDecoder()).AddLast(counterHandler).AddLast(new CounterHandlerOutbound(_outboundThroughputCounter)).AddLast(new ReadFinishedHandler(_signal, WriteCount));
                }));

            var cb = new ClientBootstrap().Group(ClientGroup).Channel<LocalChannel>().Handler(new ActionChannelInitializer<LocalChannel>(
                channel =>
                {
                    channel.Pipeline.AddLast(GetEncoder()).AddLast(GetDecoder()).AddLast(counterHandler)
                        .AddLast(new CounterHandlerOutbound(_outboundThroughputCounter));
                }));

            // start server
            _serverChannel = sb.BindAsync(TEST_ADDRESS).Result;

            // connect to server
            _clientChannel = cb.ConnectAsync(_serverChannel.LocalAddress).Result;
        }

        [PerfBenchmark(Description = "Measures how quickly and with how much GC overhead a LocalChannel --> LocalServerChannel connection can decode / encode realistic messages",
            NumberOfIterations = 13, RunMode = RunMode.Iterations, Skip = "Race issues on connect?")]
        [CounterMeasurement(InboundThroughputCounterName)]
        [CounterMeasurement(OutboundThroughputCounterName)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        public void LocalChannel_Duplex_Throughput(BenchmarkContext context)
        {
            for (var i = 0; i < WriteCount; i++)
            {
                _clientChannel.WriteAsync(Unpooled.WrappedBuffer(message).Retain());
                if (i%10 == 0) // flush every 10 writes
                    _clientChannel.Flush();
            }
            _clientChannel.Flush();
            SpinWait.SpinUntil(() => _signal.Finished);
        }


        [PerfCleanup]
        public void TearDown()
        {
            CloseChannel(_clientChannel);
            CloseChannel(_serverChannel);
            Task.WaitAll(ClientGroup.ShutdownGracefullyAsync(), ServerGroup.ShutdownGracefullyAsync());
        }

        private static void CloseChannel(IChannel cc)
        {
            cc?.CloseAsync().Wait();
        }
    }
}
