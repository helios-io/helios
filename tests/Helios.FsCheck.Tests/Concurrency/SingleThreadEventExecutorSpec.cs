// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using FsCheck;
using FsCheck.Experimental;
using FsCheck.Xunit;
using Helios.Concurrency;
using Helios.Util;

namespace Helios.FsCheck.Tests.Concurrency
{
    public class SingleThreadEventExecutorSpec : IDisposable
    {
        public SingleThreadEventExecutorSpec()
        {
            Model = new SingleThreadEventExecutorModelSpec();
        }

        public EventExecutorSpecBase Model { get; }

        public void Dispose()
        {
            ((SingleThreadEventExecutorModelSpec) Model).Dispose();
        }

        [Property]
        public Property SingleThreadEventExecutor_must_execute_operations_in_FIFO_order()
        {
            var model = new SingleThreadEventExecutorModelSpec();
            return model.ToProperty();
        }

        public class SingleThreadEventExecutorModelSpec : EventExecutorSpecBase, IDisposable
        {
            public SingleThreadEventExecutorModelSpec()
                : base(
                    new SingleThreadEventExecutor("SpecThread" + ThreadNameCounter.GetAndIncrement(),
                        TimeSpan.FromMilliseconds(40)))
            {
            }

            public static AtomicCounter ThreadNameCounter { get; } = new AtomicCounter(0);

            public void Dispose()
            {
                Executor.GracefulShutdownAsync().Wait(TimeSpan.FromSeconds(10));
            }
        }
    }
}

