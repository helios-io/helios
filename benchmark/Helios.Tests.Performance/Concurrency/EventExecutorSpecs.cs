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
    ///     Specs for setting speed and memory consumption benchmarks for <see cref="IEventExecutor" /> implementations.
    /// </summary>
    public abstract class EventExecutorSpecs
    {
        private const int ExecutorOperations = 100000;
        public const string EventExecutorThroughputCounterName = "ExecutorOps";

        private IEventExecutor _executor;
        private Counter _executorThroughput;
        private ManualResetEventSlim _resentEvent = new ManualResetEventSlim();
        private readonly AtomicCounter eventCount = new AtomicCounter(0);
        protected abstract IEventExecutor CreateExecutor();

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

        [PerfBenchmark(
            Description =
                "Test the throughput and memory footprint of Helios IFiber implementations using best practices",
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
            _executor.GracefulShutdownAsync();
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

