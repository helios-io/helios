using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Helios.Concurrency;
using Helios.Util;
using NBench;

namespace Helios.Tests.Performance.Concurrency
{
    /// <summary>
    /// Specs for setting speed and memory consumption benchmarks for <see cref="IEventExecutor"/> implementations.
    /// </summary>
    public abstract class EventExecutorSpecs
    {
        protected abstract IEventExecutor CreateExecutor();

        private IEventExecutor _executor;
        private Counter _executorThroughput;
        private const int ExecutorOperations = 100000;
        private ManualResetEventSlim _resentEvent = new ManualResetEventSlim();
        private AtomicCounter eventCount = new AtomicCounter(0);
        public const string EventExecutorThroughputCounterName = "ExecutorOps";

        [PerfSetup]
        public void SetUp(BenchmarkContext context)
        {
            _executor = CreateExecutor();
            _executorThroughput = context.GetCounter(EventExecutorThroughputCounterName);
        }

        private void Operation()
        {
            _executorThroughput.Increment();
            eventCount.GetAndIncrement();
        }

        [PerfBenchmark(Description = "Test the throughput and memory footprint of Helios IFiber implementations using best practices",
           NumberOfIterations = 13, RunMode = RunMode.Iterations, RunTimeMilliseconds = 1000)]
        [CounterMeasurement(EventExecutorThroughputCounterName)]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        public void FiberThroughputSingleDelegate(BenchmarkContext context)
        {
            for (var i = 0; i < ExecutorOperations;)
            {
                _executor.Execute(Operation);
                ++i;
            }
            SpinWait.SpinUntil(() => eventCount.Current >= ExecutorOperations, TimeSpan.FromSeconds(3));
        }

        [PerfCleanup]
        public void CleanUp()
        {
            _executor.GracefulShutdownAsync().Wait(TimeSpan.FromSeconds(10));
        }
    }

    public class SingleThreadEventExecutorSpecs : EventExecutorSpecs
    {
        protected override IEventExecutor CreateExecutor()
        {
            return new SingleThreadEventExecutor("Foo", TimeSpan.FromMilliseconds(100));
        }
    }
}
