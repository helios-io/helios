using System;
using System.Threading;
using System.Threading.Tasks;
using Helios.Concurrency;
using Helios.Ops;
using Helios.Ops.Executors;
using Helios.Util.TimedOps;

namespace Helios.Concurrency.Impl
{
    /// <summary>
    /// A <see cref="IFiber"/> implementation that uses the built-in .NET threadpool for maximum concurrency
    /// </summary>
    public class ThreadPoolFiber : IFiber
    {
        public ThreadPoolFiber() : this(new BasicExecutor()) { }

        public ThreadPoolFiber(IExecutor executor)
        {
            Executor = executor;
            Running = true;
        }

        public IExecutor Executor { get; private set; }
        public bool Running { get; set; }
        public bool WasDisposed { get; private set; }

        public void Add(Action op)
        {
            if (!Running) return;

            var wc = new WaitCallback(_ => Executor.Execute(op));
            ThreadPool.UnsafeQueueUserWorkItem(wc, null);
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
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
