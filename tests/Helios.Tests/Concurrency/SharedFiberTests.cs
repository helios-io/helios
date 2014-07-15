using System;
using System.Threading;
using Helios.Concurrency;
using Helios.Concurrency.Impl;
using Helios.Util;
using NUnit.Framework;

namespace Helios.Tests.Concurrency
{
    [TestFixture]
    public class SharedFiberTests
    {
        [Test]
        public void SharedFiber_shutdown_should_not_disrupt_original_Fiber()
        {
            var atomicCounter = new AtomicCounter(0);
            var originalFiber = FiberFactory.CreateFiber(2); //going to use a dedicated thread Fiber
            var sharedFiber1 = new SharedFiber(originalFiber);
            var sharedFiber2 = sharedFiber1.Clone();

            for (var i = 0; i < 1000; i++)
            {
                originalFiber.Add(() => atomicCounter.GetAndIncrement());
                sharedFiber1.Add(() => atomicCounter.GetAndIncrement());
            }
            sharedFiber1.GracefulShutdown(TimeSpan.FromSeconds(1)).Wait(); //wait for the fiber to finish

            Assert.AreEqual(2000, atomicCounter.Current); //should have a total count of 2000

            for (var i = 0; i < 1000; i++)
            {
                originalFiber.Add(() => atomicCounter.GetAndIncrement());
                sharedFiber1.Add(() => atomicCounter.GetAndIncrement());
            }
            Thread.Sleep(TimeSpan.FromSeconds(1));
            Assert.AreEqual(3000, atomicCounter.Current); //should have a total count of 3000
            Assert.IsTrue(sharedFiber2.Running);
            Assert.IsTrue(originalFiber.Running);
            Assert.IsFalse(sharedFiber1.Running);
        }
    }
}
