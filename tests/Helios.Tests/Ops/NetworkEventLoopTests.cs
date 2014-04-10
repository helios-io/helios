
using System;
using System.Threading;
using System.Threading.Tasks;
using Helios.Ops;
using Helios.Util;
using NUnit.Framework;

namespace Helios.Tests.Ops
{
    [TestFixture]
    public class NetworkEventLoopTests
    {
        #region Setup / Teardown
        #endregion

        #region Tests
        
        [Test]
        public void Should_be_able_to_change_NetworkEventLoop_error_handler_at_runtime()
        {
            var eventLoop = EventLoopFactory.CreateNetworkEventLoop();
            var count = new AtomicCounter(0);
            var trappedException = false;
            var backgroundProducer = Task.Run(() =>
            {

                for (var i = 0; i < 10; i++)
                {
                    eventLoop.Execute(() => count.GetAndIncrement());
                    Thread.Sleep(10);
                }
            });

            eventLoop.SetExceptionHandler((connection, exception) => trappedException = true, null);
            eventLoop.Execute(() =>
            {
                throw new Exception("I'm an exception!");
            });

            backgroundProducer.Wait();

            Assert.AreEqual(10, count.Current);
            Assert.IsTrue(trappedException);
        }

        #endregion
    }
}
