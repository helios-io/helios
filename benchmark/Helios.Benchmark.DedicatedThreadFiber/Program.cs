using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using Helios.Concurrency;

namespace Helios.Benchmark.DedicatedThreadFiber
{
    class Program
    {
        static void Main(string[] args)
        {
            var generations = 4;
            var threadCount = Environment.ProcessorCount;
            for (int i = 0; i < generations; i++)
            {
                var workItems = 10000 * (int)Math.Pow(10, i);
                Console.WriteLine("Comparing Systsem.Threading.ThreadPool vs Helios.Concurrency.DedicatedThreadFiber for {0} items", workItems);
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

        static void CreateAndWaitForWorkItems(int numWorkItems)
        {
            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                int itemsRemaining = numWorkItems;
                for (int i = 0; i < numWorkItems; i++)
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

        static void CreateAndWaitForWorkItems(int numWorkItems, int numThreads)
        {
            using (ManualResetEvent mre = new ManualResetEvent(false))
            using (var fiber = FiberFactory.CreateFiber(numThreads))
            {
                int itemsRemaining = numWorkItems;
                for (int i = 0; i < numWorkItems; i++)
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
