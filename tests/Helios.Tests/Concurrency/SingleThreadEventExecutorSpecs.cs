// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using Helios.Concurrency;
using Helios.Util;
using Xunit;

namespace Helios.Tests.Concurrency
{
    public class SingleThreadEventExecutorSpecs
    {
        public static AtomicCounter ThreadNameCounter { get; } = new AtomicCounter(0);

        [Fact]
        public void STE_should_complete_task_when_operation_is_completed()
        {
            var executor = new SingleThreadEventExecutor("Foo" + ThreadNameCounter.GetAndIncrement(),
                TimeSpan.FromMilliseconds(100));

            Func<bool> myFunc = () => true;
            var task = executor.SubmitAsync(myFunc);
            Assert.True(task.Wait(200), "Should have completed task in under 200 milliseconds");
            Assert.True(task.Result);
            executor.GracefulShutdownAsync();
        }

        [Fact]
        public void STE_should_execute_scheduled_tasks_on_time()
        {
            var counter = new AtomicCounter(0);
            var executor = new SingleThreadEventExecutor("Foo" + ThreadNameCounter.GetAndIncrement(),
                TimeSpan.FromMilliseconds(100));
            Action<object> increment = o => ((AtomicCounter) o).GetAndIncrement();

            // schedule a delayed operation
            var checkCounter = executor.ScheduleAsync(o => Assert.True(((AtomicCounter) o).Current == 4), counter,
                TimeSpan.FromMilliseconds(40));

            // schedule 4 immediate operations
            executor.Execute(increment, counter);
            executor.Execute(increment, counter);
            executor.Execute(increment, counter);
            executor.Execute(increment, counter);

            // delay should run after the first 4 previous
            checkCounter.Wait(TimeSpan.FromMilliseconds(100));
        }

        [Fact]
        public void STE_should_run_shutdown_hooks()
        {
            var executor = new SingleThreadEventExecutor("Foo" + ThreadLocalRandom.Current.Next(),
                TimeSpan.FromMilliseconds(100));

            var hook = new MyHook();
            executor.AddShutdownHook(hook);

            // added a sanity check here to make sure that normal scheduled operations can run
            Func<bool> myFunc = () => true;
            var task = executor.SubmitAsync(myFunc);
            Assert.True(task.Wait(200), "Should have completed task in under 200 milliseconds");
            Assert.True(task.Result);
            // end sanity check, begin actual test

            executor.GracefulShutdownAsync().Wait();
            Assert.True(hook.WasExecuted);
        }

        [Fact]
        public void STE_should_not_run_cancelled_shutdown_hooks()
        {
            var executor = new SingleThreadEventExecutor("Foo" + ThreadLocalRandom.Current.Next(),
                TimeSpan.FromMilliseconds(100));

            var hook = new MyHook();
            executor.AddShutdownHook(hook);
            executor.RemoveShutdownHook(hook);
            executor.GracefulShutdownAsync().Wait();
            Assert.False(hook.WasExecuted);
        }

        private class MyHook : IRunnable
        {
            public bool WasExecuted { get; private set; }

            public void Run()
            {
                WasExecuted = true;
            }
        }
    }
}

