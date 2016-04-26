using System;
using Helios.Concurrency;
using Helios.Util;
using Xunit;

namespace Helios.Tests.Concurrency
{
    public class SingleThreadEventExecutorSpecs : IDisposable
    {
        public static AtomicCounter ThreadNameCounter { get; } = new AtomicCounter(0);

        public SingleThreadEventExecutorSpecs()
        {
            Executor = new SingleThreadEventExecutor("Foo" + ThreadNameCounter.GetAndIncrement(), TimeSpan.FromMilliseconds(100));
        }

        protected SingleThreadEventExecutor Executor { get; }
        
        [Fact]
        public void STE_should_complete_task_when_operation_is_completed()
        {
            Func<bool> myFunc = () => true;
            var task = Executor.SubmitAsync(myFunc);
            Assert.True(task.Wait(200), "Should have completed task in under 200 milliseconds");
            Assert.True(task.Result);
        }

        class MyHook : IRunnable
        {
            public bool WasExecuted { get; private set; }

            public void Run()
            {
                WasExecuted = true;
            }
        }

        [Fact]
        public void STE_should_run_shutdown_hooks()
        {
            var hook = new MyHook();
            Executor.AddShutdownHook(hook);
            Assert.True(Executor.GracefulShutdownAsync().Wait(21000));
            Assert.True(hook.WasExecuted);
        }

        public void Dispose()
        {
            Executor.Dispose();
        }
    }
}
