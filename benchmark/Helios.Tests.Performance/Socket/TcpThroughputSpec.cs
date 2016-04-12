using System;
using System.Net;
using System.Threading;
using Helios.MultiNodeTests.TestKit;
using Helios.Net;
using NBench;

namespace Helios.Tests.Performance.Socket
{
    /// <summary>
    /// Going to re-use the multi-node testkit for running this benchmark
    /// </summary>
    public class TcpThroughputSpec : MultiNodeTest
    {
        public override TransportType TransportType
        {
            get { return TransportType.Tcp; }
        }

        public override bool HighPerformance
        {
            get { return true; }
        }

        public const int MessageLength = 200;
        public const int MessageCount = 10;
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
                if(ServerReceived.GetAndIncrement() >= MessageCount)
                    _resentEvent.Set();
            });
            StartClient();
            message = new byte[MessageLength];
        }

        [PerfBenchmark(Description = "Tests a full request/response sequence for 100000 messages", RunMode = RunMode.Iterations, NumberOfIterations = 13, Skip="Buggy with Helios at the moment")]
        [CounterMeasurement(MessagesReceivedCounter)]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        public void RunBenchmark(BenchmarkContext context)
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
