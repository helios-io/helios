// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Threading;
using Helios.MultiNodeTests.TestKit;
using NBench;

namespace Helios.Tests.Performance.Socket
{
    /// <summary>
    ///     Going to re-use the multi-node testkit for running this benchmark
    /// </summary>
    public abstract class SocketThroughputSpec : MultiNodeTest
    {
        public const int MessageLength = 200;
        public const int MessageCount = 1000;
        public const string MessagesReceivedCounter = "MessagesReceived";
        private readonly ManualResetEventSlim _resentEvent = new ManualResetEventSlim(false);
        private Counter benchmarkCounter;
        private byte[] message;

        public override bool HighPerformance
        {
            get { return true; }
        }

        /// <summary>
        ///     Single threaded execution, to respect order
        /// </summary>
        public override int WorkerThreads
        {
            get { return 1; }
        }

        [PerfSetup]
        public void PerfSetUp(BenchmarkContext context)
        {
            benchmarkCounter = context.GetCounter(MessagesReceivedCounter);
            StartServer((data, channel) =>
            {
                benchmarkCounter.Increment();
                var serverReceived = ServerReceived.GetAndIncrement();
                if (serverReceived >= MessageCount - 1)
                    _resentEvent.Set();
            });
            StartClient();
            message = new byte[MessageLength];
        }

        [PerfBenchmark(Description = "Tests a full request/response sequence for 1000 messages",
            RunMode = RunMode.Iterations, NumberOfIterations = 13, Skip = "Debugging")]
        [CounterMeasurement(MessagesReceivedCounter)]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        public void OneWayThroughputBenchmark(BenchmarkContext context)
        {
            for (var i = 0; i < MessageCount; i++)
            {
                Send(message);
            }
            _resentEvent.Wait();
        }

        [PerfCleanup]
        public void PerfCleanup()
        {
            CleanUp();
        }
    }
}

