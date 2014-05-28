using System;
using System.Threading.Tasks;
using Helios.Ops;
using Helios.Ops.Executors;

namespace Helios.Concurrency.Impl
{
    /// <summary>
    /// IFiber implementation that doesn't use any form of concurrency under the hood
    /// </summary>
    public class SynchronousFiber : IFiber
    {
        public SynchronousFiber() : this(new BasicExecutor()) { }

        public SynchronousFiber(IExecutor executor)
        {
            Executor = executor ?? new BasicExecutor();
        }

        public IExecutor Executor { get; private set; }
        public bool WasDisposed { get; private set; }

        public void Add(Action op)
        {
            if(Executor.AcceptingJobs)
                Executor.Execute(op);
        }

        public void SwapExecutor(IExecutor executor)
        {
            //Shut down the previous executor
            Executor.GracefulShutdown(TimeSpan.FromSeconds(3));
            Executor = executor;
        }

        public void Shutdown(TimeSpan gracePeriod)
        {
            Executor.Shutdown(gracePeriod);
        }

        public Task GracefulShutdown(TimeSpan gracePeriod)
        {
            return Executor.GracefulShutdown(gracePeriod);
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

        public IFiber Clone()
        {
            return new SynchronousFiber(Executor.Clone());
        }

        #region IDisposable members

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}