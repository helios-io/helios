// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Helios.Channels;
using Helios.Channels.Bootstrap;
using Helios.Channels.Sockets;
using Helios.Codecs;
using Helios.Logging;
using Helios.Tests.Channels;
using Helios.Tests.Performance.Channels;
using Helios.Util;
using Helios.Util.TimedOps;
using NBench;

namespace Helios.HorizontalScaling.Tests.Performance.Channels
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

        private const string InboundThroughputCounterName = "inbound ops";
        private Counter _inboundThroughputCounter;

        private const string OutboundThroughputCounterName = "outbound ops";
        private Counter _outboundThroughputCounter;

        private const string ErrorCounterName = "exceptions caught";
        private Counter _errorCounter;

        private IChannel _serverChannel;
        private ConcurrentBag<IChannel> _clientChannels;
        private CancellationTokenSource _shutdownBenchmark;
        protected readonly ManualResetEventSlim ResetEvent = new ManualResetEventSlim(false);

        public const int IterationCount = 1; // these are LONG-running benchmarks. stick with a lower iteration count

        // Sleep main thread, then start a new client every 30 ms
        private static readonly TimeSpan SleepInterval = TimeSpan.FromMilliseconds(300);

        /// <summary>
        /// If it takes longer than <see cref="SaturationThreshold"/> to establish 10 connections, we're saturated.
        /// 
        /// End the stress test.
        /// </summary>
        private static readonly TimeSpan SaturationThreshold = TimeSpan.FromSeconds(15);

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

            _inboundThroughputCounter = context.GetCounter(InboundThroughputCounterName);
            _outboundThroughputCounter = context.GetCounter(OutboundThroughputCounterName);
            _clientConnectedCounter = context.GetCounter(ClientConnectCounterName);
            _errorCounter = context.GetCounter(ErrorCounterName);

            _signal = new ManualResetEventSlimReadFinishedSignal(ResetEvent);

            var sb = new ServerBootstrap().Group(ServerGroup, WorkerGroup).Channel<TcpServerSocketChannel>()
                .ChildOption(ChannelOption.TcpNodelay, true)
                .ChildHandler(new ActionChannelInitializer<TcpSocketChannel>(channel =>
                {
                    channel.Pipeline.AddLast(GetEncoder())
                        .AddLast(GetDecoder())
                        .AddLast(new IntCodec(true))
                        .AddLast(new CounterHandlerInbound(_inboundThroughputCounter))
                        .AddLast(new CounterHandlerOutbound(_outboundThroughputCounter))
                        .AddLast(new ErrorCounterHandler(_errorCounter));
                }));

            ClientBootstrap = new ClientBootstrap().Group(ClientGroup)
                .Option(ChannelOption.TcpNodelay, true)
                .Channel<TcpSocketChannel>().Handler(new ActionChannelInitializer<TcpSocketChannel>(
                channel =>
                {
                    channel.Pipeline.AddLast(GetEncoder())
                    .AddLast(GetDecoder())
                    .AddLast(new IntCodec(true))
                    .AddLast(new CounterHandlerInbound(_inboundThroughputCounter))
                    .AddLast(new CounterHandlerOutbound(_outboundThroughputCounter))
                    .AddLast(new ErrorCounterHandler(_errorCounter));
                }));

            var token = _shutdownBenchmark.Token;
            _eventLoop = () =>
            {
                while (!token.IsCancellationRequested)
                {
                    foreach (var channel in _clientChannels)
                    {
                        // unrolling a loop
                        channel.WriteAsync(ThreadLocalRandom.Current.Next());
                        channel.WriteAsync(ThreadLocalRandom.Current.Next());
                        channel.WriteAsync(ThreadLocalRandom.Current.Next());
                        channel.WriteAsync(ThreadLocalRandom.Current.Next());
                        channel.WriteAsync(ThreadLocalRandom.Current.Next());
                        channel.WriteAsync(ThreadLocalRandom.Current.Next());
                        channel.WriteAsync(ThreadLocalRandom.Current.Next());
                        channel.WriteAsync(ThreadLocalRandom.Current.Next());
                        channel.WriteAsync(ThreadLocalRandom.Current.Next());
                        channel.Flush();
                    }

                    // sleep for a tiny bit, then get going again
                    Thread.Sleep(40);
                }
            };

            // start server
            _serverChannel = sb.BindAsync(TEST_ADDRESS).Result;

            // connect to server with 1 client initially
            _clientChannels.Add(ClientBootstrap.ConnectAsync(_serverChannel.LocalAddress).Result);
            
        }

        private Action _eventLoop;

        [PerfBenchmark(Description = "Measures how quickly and with how much GC overhead a TcpSocketChannel --> TcpServerSocketChannel connection can decode / encode realistic messages",
            NumberOfIterations = IterationCount, RunMode = RunMode.Iterations)]
        [CounterMeasurement(InboundThroughputCounterName)]
        [CounterMeasurement(OutboundThroughputCounterName)]
        [CounterMeasurement(ClientConnectCounterName)]
        [CounterMeasurement(ErrorCounterName)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        public void TcpServerSocketChannel_horizontal_scale_stress_test(BenchmarkContext context)
        {
            _clientConnectedCounter.Increment(); // for the initial client
            var totalRunSeconds = TimeSpan.FromSeconds(ThreadLocalRandom.Current.Next(180, 360)); // 3-6 minutes
            Console.WriteLine("Running benchmark for {0} minutes", totalRunSeconds.TotalMinutes);
            var deadline = new PreciseDeadline(totalRunSeconds);
            var due = DateTime.Now + totalRunSeconds;
            var lastMeasure = due;
            var task = Task.Factory.StartNew(_eventLoop); // start writing
            var runCount = 1;
            while (!deadline.IsOverdue)
            {
                // add a new client
                _clientChannels.Add(ClientBootstrap.ConnectAsync(_serverChannel.LocalAddress).Result);
                _clientConnectedCounter.Increment();
                Thread.Sleep(SleepInterval);
                if (++runCount%10 == 0)
                {
                    Console.WriteLine("{0} minutes remaining [{1} connections active].", (due - DateTime.Now).TotalMinutes, runCount);
                    var saturation = (DateTime.Now - lastMeasure);
                    if (saturation > SaturationThreshold)
                    {
                        Console.WriteLine("Took {0} to create 10 connections; exceeded pre-defined saturation threshold of {1}. Ending stress test.", saturation, SaturationThreshold);
                        break;
                    }
                    lastMeasure = DateTime.Now;
                }
            }
            _shutdownBenchmark.Cancel();
        }


        [PerfCleanup]
        public void TearDown()
        {
            _eventLoop = null;
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

