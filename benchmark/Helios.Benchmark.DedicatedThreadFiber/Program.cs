// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Helios.Concurrency;

namespace Helios.Benchmark.DedicatedThreadFiber
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var generations = 4;
            var threadCount = Environment.ProcessorCount;
            for (var i = 0; i < generations; i++)
            {
                var workItems = 10000*(int) Math.Pow(10, i);
                Console.WriteLine(
                    "Comparing Systsem.Threading.ThreadPool vs Helios.Concurrency.DedicatedThreadFiber for {0} items",
                    workItems);
                Console.WriteLine("DedicatedThreadFiber.NumThreads: {0}", threadCount);

                Console.WriteLine("System.Threading.ThreadPool");
                Console.WriteLine(
                    TimeSpan.FromMilliseconds(
                        Enumerable.Range(0, 6).Select(_ =>
                        {
                            var sw = Stopwatch.StartNew();
                            CreateAndWaitForWorkItems(workItems);
                            return sw.ElapsedMilliseconds;
                        }).Skip(1).Average()
                        )
                    );

                Console.WriteLine("Helios.Concurrency.DedicatedThreadFiber");
                Console.WriteLine(
                    TimeSpan.FromMilliseconds(
                        Enumerable.Range(0, 6).Select(_ =>
                        {
                            var sw = Stopwatch.StartNew();
                            CreateAndWaitForWorkItems(workItems, threadCount);
                            return sw.ElapsedMilliseconds;
                        }).Skip(1).Average()
                        )
                    );
            }
        }

        private static void CreateAndWaitForWorkItems(int numWorkItems)
        {
            using (var mre = new ManualResetEvent(false))
            {
                var itemsRemaining = numWorkItems;
                for (var i = 0; i < numWorkItems; i++)
                {
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        if (Interlocked.Decrement(
                            ref itemsRemaining) == 0) mre.Set();
                    });
                }
                mre.WaitOne();
            }
        }

        private static void CreateAndWaitForWorkItems(int numWorkItems, int numThreads)
        {
            using (var mre = new ManualResetEvent(false))
            using (var fiber = FiberFactory.CreateFiber(numThreads))
            {
                var itemsRemaining = numWorkItems;
                for (var i = 0; i < numWorkItems; i++)
                {
                    fiber.Add(delegate
                    {
                        if (Interlocked.Decrement(
                            ref itemsRemaining) == 0) mre.Set();
                    });
                }
                mre.WaitOne();
            }
        }
    }
}

