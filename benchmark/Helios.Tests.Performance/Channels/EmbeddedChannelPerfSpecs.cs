// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using Helios.Buffers;
using Helios.Channels;
using Helios.Channels.Embedded;
using Helios.Codecs;
using Helios.Concurrency;
using Helios.Logging;
using Helios.Util;
using NBench;

namespace Helios.Tests.Performance.Channels
{
    /// <summary>
    ///     End-to-end performance benchmark for a realistic-ish pipeline built using the <see cref="EmbeddedChannel" />
    ///     Contains a total of three handlers and runs on the same thread as the caller, so all calls made against the
    ///     pipeline
    ///     are totally synchronous. Tests the overhead of the following components working together:
    ///     1. <see cref="IChannelPipeline" /> default implementation
    ///     2. <see cref="IChannelHandlerContext" /> default implementation
    ///     3. <see cref="IRecvByteBufAllocator" /> default implementation
    ///     4. <see cref="IByteBufAllocator" /> default implementation
    ///     5. <see cref="ChannelOutboundBuffer" />
    ///     6. <see cref="ObjectPool{T}" /> and <see cref="RecyclableArrayList" />
    ///     7. <see cref="AbstractChannel" /> and its built-in <see cref="IChannelUnsafe" /> implementation
    ///     8. <see cref="LengthFieldPrepender" /> and <see cref="LengthFieldBasedFrameDecoder" />
    ///     9. And finally, <see cref="AbstractEventExecutor" />.
    ///     Buffer size for each individual written message is intentionally kept small, since this is a throughput and GC
    ///     overhead test.
    /// </summary>
    public class EmbeddedChannelPerfSpecs
    {
        private const string InboundThroughputCounterName = "inbound ops";

        private const string OutboundThroughputCounterName = "outbound ops";
        private IChannelHandler _counterHandlerInbound;
        private IChannelHandler _counterHandlerOutbound;
        private Counter _inboundThroughputCounter;
        private Counter _outboundThroughputCounter;

        private EmbeddedChannel channel;

        static EmbeddedChannelPerfSpecs()
        {
            // Disable the logging factory
            LoggingFactory.DefaultFactory = new NoOpLoggerFactory();
        }


        [PerfSetup]
        public void SetUp(BenchmarkContext context)
        {
            _inboundThroughputCounter = context.GetCounter(InboundThroughputCounterName);
            _counterHandlerInbound = new CounterHandlerInbound(_inboundThroughputCounter);
            _outboundThroughputCounter = context.GetCounter(OutboundThroughputCounterName);
            _counterHandlerOutbound = new CounterHandlerOutbound(_outboundThroughputCounter);

            channel = new EmbeddedChannel(_counterHandlerOutbound, _counterHandlerInbound);
        }

        [PerfBenchmark(
            Description =
                "Measures how quickly and with how much GC overhead the EmbeddedChannel can decode / encode realistic messages",
            NumberOfIterations = 13, RunMode = RunMode.Throughput)]
        [CounterMeasurement(InboundThroughputCounterName)]
        [CounterMeasurement(OutboundThroughputCounterName)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        public void EmbeddedChannel_Inbound_Throughput(BenchmarkContext context)
        {
            channel.WriteInbound(1);
        }

        [PerfBenchmark(
            Description =
                "Measures how quickly and with how much GC overhead the EmbeddedChannel can decode / encode realistic messages",
            NumberOfIterations = 13, RunMode = RunMode.Throughput)]
        [CounterMeasurement(InboundThroughputCounterName)]
        [CounterMeasurement(OutboundThroughputCounterName)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        public void EmbeddedChannel_Outbound_Throughput(BenchmarkContext context)
        {
            channel.WriteOutbound(1);
        }

        [PerfCleanup]
        public void CleanUp()
        {
            channel.Finish();
            channel = null;
        }
    }
}

