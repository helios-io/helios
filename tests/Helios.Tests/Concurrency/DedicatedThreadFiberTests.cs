using System;
using Helios.Concurrency;
using Helios.Util;
using Xunit;

namespace Helios.Tests.Concurrency
{
    
    public class DedicatedThreadFiberTests
    {
        [Fact]
        public void Should_use_multiple_threads_to_process_queue()
        {
            var atomicCounter = new AtomicCounter(0);
            var fiber = FiberFactory.CreateFiber(2);
            for (var i = 0; i < 1000; i++)
            {
                fiber.Add(() => atomicCounter.GetAndIncrement());
            }
            fiber.GracefulShutdown(TimeSpan.FromSeconds(1)).Wait(); //wait for the fiber to finish
            Assert.Equal(1000, atomicCounter.Current);
        }

        [Fact]
        public void Should_not_be_able_to_add_jobs_after_shutdown()
        {
            var atomicCounter = new AtomicCounter(0);
            var fiber = FiberFactory.CreateFiber(2);
            for (var i = 0; i < 1000; i++)
            {
                fiber.Add(() => atomicCounter.GetAndIncrement());
            }
            fiber.GracefulShutdown(TimeSpan.FromSeconds(1)).Wait(); //wait for the fiber to finish

            //try to increment the counter a bunch more times
            for (var i = 0; i < 1000; i++)
            {
                fiber.Add(() => atomicCounter.GetAndIncrement());
            }

            //value should be equal to its pre-shutdown value
            Assert.Equal(1000, atomicCounter.Current);
        }
    }
}
