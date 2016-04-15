using System;
using System.Threading;
using Helios.Concurrency;
using Helios.Util;
using NBench;

namespace Helios.Tests.Performance.Concurrency
{
    /// <summary>
    /// Specs for setting a speed and memory consumption baseline for Fibers
    /// </summary>
    public abstract class FiberSpecs
    {
        /// <summary>
        /// Create a <see cref="IFiber"/> in accordance with the specifications
        /// of the concrete spec implementations.
        /// </summary>
        /// <returns>An <see cref="IFiber"/> instance</returns>
        protected abstract IFiber CreateFiber();

        private IFiber _fiber;
        private Counter _fiberThroughput;
        public const int FiberOperations = 100000;
        private ManualResetEventSlim _resentEvent = new ManualResetEventSlim();
        private AtomicCounter eventCount = new AtomicCounter(0);
        public const string FiberThroughputCounterName = "FiberOps";

        [PerfSetup]
        public void SetUp(BenchmarkContext context)
        {
            _fiber = CreateFiber();
            _fiberThroughput = context.GetCounter(FiberThroughputCounterName);
        }

        private void Operation()
        {
            _fiberThroughput.Increment();
            var next = eventCount.GetAndIncrement() + 1;
            if (next >= FiberOperations)
                _resentEvent.Set();
        }

        [PerfBenchmark(Description = "Test the throughput and memory footprint of Helios IFiber implementations using best practices",
            NumberOfIterations = 13, RunMode = RunMode.Iterations, RunTimeMilliseconds = 1000)]
        [CounterMeasurement(FiberThroughputCounterName)]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        public void FiberThroughputSingleDelegate(BenchmarkContext context)
        {
            for (var i = 0; i < FiberOperations;)
            {
                _fiber.Add(Operation);
                ++i;
            }
            _resentEvent.Wait(TimeSpan.FromSeconds(3));
        }

        [PerfBenchmark(Description = "Test the throughput and memory footprint of Helios IFiber implementations using not-so-great practices",
            NumberOfIterations = 13, RunMode = RunMode.Iterations, RunTimeMilliseconds = 1000)]
        [CounterMeasurement(FiberThroughputCounterName)]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        public void FiberThroughputDynamicDelegate(BenchmarkContext context)
        {
            for (var i = 0; i < FiberOperations;)
            {
                _fiber.Add(() => Operation());
                ++i;
            }
            _resentEvent.Wait();
        }

        [PerfCleanup]
        public void CleanUp()
        {
            _fiber.Stop();
        }
    }

    public class ThreadPoolFiberSpecs : FiberSpecs
    {
        protected override IFiber CreateFiber()
        {
            return FiberFactory.CreateFiber(FiberMode.MaximumConcurrency);
        }
    }

    /// <summary>
    /// using 2 threads
    /// </summary>
    public class DedicatedThreadPoolFiberSpecs : FiberSpecs
    {
        protected override IFiber CreateFiber()
        {
            return FiberFactory.CreateFiber(2);
        }
    }

    public class SingleThreadFiberSpecs : FiberSpecs
    {
        protected override IFiber CreateFiber()
        {
            return FiberFactory.CreateFiber(FiberMode.SingleThreaded);
        }
    }

    public class SynchronousFiberSpecs : FiberSpecs
    {
        protected override IFiber CreateFiber()
        {
            return FiberFactory.CreateFiber(FiberMode.Synchronous);
        }
    }
}
