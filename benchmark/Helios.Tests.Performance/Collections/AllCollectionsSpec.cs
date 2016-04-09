using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helios.Util.Collections;
using NBench;

namespace Helios.Tests.Performance.Collections
{
    /// <summary>
    /// Performance specs for checking the underlying Collections' implementation's read / write performance
    /// </summary>
    public class AllCollectionsSpec
    {
        private Counter _insertsCounter;
        private const string InsertCounterName = "ItemInserts";
        private const int ItemCount = 1000;
        private const int ResizedItemCount = 10*ItemCount;

        private CircularBuffer<int> circularBuffer = new CircularBuffer<int>(ItemCount, ResizedItemCount);
        private ConcurrentCircularBuffer<int> concurrentCircularBuffer = new ConcurrentCircularBuffer<int>(ItemCount, ResizedItemCount);
        private ConcurrentQueue<int> concurrentQueue = new ConcurrentQueue<int>();

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
