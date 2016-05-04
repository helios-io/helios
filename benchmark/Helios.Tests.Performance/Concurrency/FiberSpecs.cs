// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Threading;
using Helios.Concurrency;
using Helios.Util;
using NBench;

namespace Helios.Tests.Performance.Concurrency
{
    /// <summary>
    ///     Specs for setting a speed and memory consumption baseline for Fibers
    /// </summary>
    public abstract class FiberSpecs
    {
        public const int FiberOperations = 100000;
        public const string FiberThroughputCounterName = "FiberOps";

        private IFiber _fiber;
        private Counter _fiberThroughput;
        private ManualResetEventSlim _resentEvent = new ManualResetEventSlim();
        private readonly AtomicCounter eventCount = new AtomicCounter(0);

        /// <summary>
        ///     Create a <see cref="IFiber" /> in accordance with the specifications
        ///     of the concrete spec implementations.
        /// </summary>
        /// <returns>An <see cref="IFiber" /> instance</returns>
        protected abstract IFiber CreateFiber();

        [PerfSetup]
        public void SetUp(BenchmarkContext context)
        {
            _fiber = CreateFiber();
            _fiberThroughput = context.GetCounter(FiberThroughputCounterName);
        }

        private void Operation()
        {
            _fiberThroughput.Increment();
            eventCount.GetAndIncrement();
        }

        [PerfBenchmark(
            Description =
                "Test the throughput and memory footprint of Helios IFiber implementations using best practices",
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
            SpinWait.SpinUntil(() => eventCount.Current >= FiberOperations, TimeSpan.FromSeconds(3));
        }

        [PerfBenchmark(
            Description =
                "Test the throughput and memory footprint of Helios IFiber implementations using not-so-great practices",
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
            SpinWait.SpinUntil(() => eventCount.Current >= FiberOperations, TimeSpan.FromSeconds(3));
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
    ///     using 2 threads
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

