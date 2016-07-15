// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Threading;
using System.Threading.Tasks;
using Helios.Ops;
using Helios.Util;
using Xunit;

namespace Helios.Tests.Ops
{
    public class NetworkEventLoopTests
    {
        #region Tests

        [Fact]
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
            eventLoop.Execute(() => { throw new Exception("I'm an exception!"); });

            backgroundProducer.Wait();

            Assert.Equal(10, count.Current);
            Assert.True(trappedException);
        }

        #endregion

        #region Setup / Teardown

        #endregion
    }
}