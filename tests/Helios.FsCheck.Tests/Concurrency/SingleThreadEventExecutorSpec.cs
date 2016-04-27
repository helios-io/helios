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

        [Property(QuietOnSuccess = true, MaxTest = 5000)]
        public Property SingleThreadEventExecutor_must_execute_operations_in_FIFO_order()
        {
            var model = new SingleThreadEventExecutorModelSpec();
            return model.ToProperty();
        }

        public class SingleThreadEventExecutorModelSpec : EventExecutorSpecBase, IDisposable
        {
            public static AtomicCounter ThreadNameCounter { get; } = new AtomicCounter(0);

            public SingleThreadEventExecutorModelSpec() : base(new SingleThreadEventExecutor("SpecThread" + ThreadNameCounter.GetAndIncrement(), TimeSpan.FromMilliseconds(40)))
            {
            }

            public void Dispose()
            {
                Executor.GracefulShutdownAsync().Wait(TimeSpan.FromSeconds(10));
            }
        }

        public void Dispose()
        {
            ((SingleThreadEventExecutorModelSpec)Model).Dispose();
        }
    }
}