using System;
using System.Threading;
using Helios.MultiNodeTests.TestKit;
using Helios.Net;
using NBench;

namespace Helios.Tests.Performance.Socket
{
    /// <summary>
    /// Going to re-use the multi-node testkit for running this benchmark
    /// </summary>
    public abstract class SocketThroughputSpec : MultiNodeTest
    {
        public override bool HighPerformance
        {
            get { return true; }
        }

        /// <summary>
        /// Single threaded execution, to respect order
        /// </summary>
        public override int WorkerThreads { get { return 1; } }

        public const int MessageLength = 200;
        public const int MessageCount = 1000;
        public const string MessagesReceivedCounter = "MessagesReceived";
        private ManualResetEventSlim _resentEvent = new ManualResetEventSlim(false);
        private byte[] message;
        private Counter benchmarkCounter;

        [PerfSetup]
        public void PerfSetUp(BenchmarkContext context)
        {
            benchmarkCounter = context.GetCounter(MessagesReceivedCounter);
            StartServer((data, channel) =>
            {
                benchmarkCounter.Increment();
                var serverReceived = ServerReceived.GetAndIncrement();
                if (serverReceived >= MessageCount-1)
                    _resentEvent.Set();
            });
            StartClient();
            message = new byte[MessageLength];
        }

        [PerfBenchmark(Description = "Tests a full request/response sequence for 1000 messages", RunMode = RunMode.Iterations, NumberOfIterations = 13, Skip = "Debugging")]
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
