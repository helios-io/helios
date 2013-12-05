using System;
using Helios.Core.Ops;
using Helios.Core.Ops.Executors;

namespace Helios.Core.Concurrency
{
    /// <summary>
    /// IFiber implementation that doesn't use any form of concurrency under the hood
    /// </summary>
    public class SynchronousFiber : IFiber
    {
        protected readonly IExecutor Executor;

        public SynchronousFiber() : this(new BasicExecutor()) { }

        public SynchronousFiber(IExecutor executor)
        {
            Executor = executor;
        }

        public void Add(Action op)
        {
            if(Executor.AcceptingJobs)
                Executor.Execute(op);
        }

        public void Shutdown(TimeSpan gracePeriod)
        {
            Executor.Shutdown(gracePeriod);
        }

        public void Stop()
        {
            Executor.Shutdown();
        }
    }
}