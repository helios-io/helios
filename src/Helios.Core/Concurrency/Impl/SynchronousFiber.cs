using System;
using Helios.Ops;
using Helios.Ops.Executors;

namespace Helios.Concurrency.Impl
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

        public bool WasDisposed { get; private set; }

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

        public void Dispose(bool isDisposing)
        {
            if (!WasDisposed)
            {
                if (isDisposing)
                {
                    Executor.Shutdown();
                } 
            }

            WasDisposed = true;
        }

        #region IDisposable members

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}