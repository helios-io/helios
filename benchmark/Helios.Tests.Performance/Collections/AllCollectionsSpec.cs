// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Collections.Concurrent;
using Helios.Util.Collections;
using NBench;

namespace Helios.Tests.Performance.Collections
{
    /// <summary>
    ///     Performance specs for checking the underlying Collections' implementation's read / write performance
    /// </summary>
    public class AllCollectionsSpec
    {
        private const string InsertCounterName = "ItemInserts";
        private const int ItemCount = 1000;
        private const int ResizedItemCount = 10*ItemCount;
        private Counter _insertsCounter;

        private readonly CircularBuffer<int> circularBuffer = new CircularBuffer<int>(ItemCount);

        private readonly ConcurrentCircularBuffer<int> concurrentCircularBuffer =
            new ConcurrentCircularBuffer<int>(ItemCount);

        private readonly ConcurrentQueue<int> concurrentQueue = new ConcurrentQueue<int>();

        [PerfSetup]
        public void SetUp(BenchmarkContext context)
        {
            _insertsCounter = context.GetCounter(InsertCounterName);
        }

        [PerfBenchmark(NumberOfIterations = 13, RunMode = RunMode.Iterations, TestMode = TestMode.Measurement)]
        [CounterMeasurement(InsertCounterName)]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        public void CircularBufferWithoutResizing(BenchmarkContext context)
        {
            for (var i = 0; i < ItemCount;)
            {
                circularBuffer.Add(i);
                _insertsCounter.Increment();
                ++i;
            }
        }

        [PerfBenchmark(NumberOfIterations = 13, RunMode = RunMode.Iterations, TestMode = TestMode.Measurement)]
        [CounterMeasurement(InsertCounterName)]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        public void CircularBufferWithResizing(BenchmarkContext context)
        {
            for (var i = 0; i < ResizedItemCount;)
            {
                circularBuffer.Add(i);
                _insertsCounter.Increment();
                ++i;
            }
        }

        [PerfBenchmark(NumberOfIterations = 13, RunMode = RunMode.Iterations, TestMode = TestMode.Measurement)]
        [CounterMeasurement(InsertCounterName)]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        public void ConcurrentCircularBufferWithoutResizing(BenchmarkContext context)
        {
            for (var i = 0; i < ItemCount;)
            {
                concurrentCircularBuffer.Add(i);
                _insertsCounter.Increment();
                ++i;
            }
        }

        [PerfBenchmark(NumberOfIterations = 13, RunMode = RunMode.Iterations, TestMode = TestMode.Measurement)]
        [CounterMeasurement(InsertCounterName)]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        public void ConcurrentCircularBufferWithResizing(BenchmarkContext context)
        {
            for (var i = 0; i < ResizedItemCount;)
            {
                concurrentCircularBuffer.Add(i);
                _insertsCounter.Increment();
                ++i;
            }
        }

        [PerfBenchmark(NumberOfIterations = 13, RunMode = RunMode.Iterations, TestMode = TestMode.Measurement)]
        [CounterMeasurement(InsertCounterName)]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        public void ConcurrentQueueWithoutResizing(BenchmarkContext context)
        {
            for (var i = 0; i < ItemCount;)
            {
                concurrentQueue.Enqueue(i);
                _insertsCounter.Increment();
                ++i;
            }
        }

        [PerfBenchmark(NumberOfIterations = 13, RunMode = RunMode.Iterations, TestMode = TestMode.Measurement)]
        [CounterMeasurement(InsertCounterName)]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        public void ConcurrentQueueWithResizing(BenchmarkContext context)
        {
            for (var i = 0; i < ResizedItemCount;)
            {
                concurrentQueue.Enqueue(i);
                _insertsCounter.Increment();
                ++i;
            }
        }
    }
}

