using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Helios.Ops;
using Helios.Ops.Executors;
using Helios.Util;
using Helios.Util.Concurrency;

namespace Helios.Concurrency.Impl
{
    public class DedicatedThreadPoolFiber : IFiber
    {
        private readonly int _numThreads;
        private List<Thread> _threads;

        private readonly BlockingCollection<Action> _blockingCollection = new BlockingCollection<Action>(25000);

        public DedicatedThreadPoolFiber(int numThreads)
            : this(new BasicExecutor(), numThreads)
        {
        }

        public DedicatedThreadPoolFiber(IExecutor executor, int numThreads)
        {
            Executor = executor ?? new BasicExecutor();
            numThreads.NotNegative();
            numThreads.NotLessThan(1);
            _numThreads = numThreads;
            SpawnThreads(numThreads);
        }

        protected void SpawnThreads(int threadCount)
        {
            _threads = new List<Thread>(threadCount);
            for (var i = 0; i < threadCount; i++)
            {

                var thread = new Thread(_ =>
                {
                    foreach (var task in _blockingCollection.GetConsumingEnumerable())
                    {
                        Executor.Execute(task);
                        if (!Executor.AcceptingJobs) break;
                    }
                }) { IsBackground = true };
                thread.Start();
                _threads.Add(thread);
            }

        }

        private volatile IExecutor _executor;
        public IExecutor Executor { get { return _executor; } set { _executor = value; } }
        public bool WasDisposed { get; private set; }

        public bool Running { get { return Executor.AcceptingJobs; } }

        public void Add(Action op)
        {
            if (Executor.AcceptingJobs)
                _blockingCollection.Add(op);
        }

        public void SwapExecutor(IExecutor executor)
        {
            //Shut down the previous executor gracefully (in case there's thread-contention)
            Executor.GracefulShutdown(TimeSpan.FromSeconds(3));
            Executor = executor;
        }

        public void Shutdown(TimeSpan gracePeriod)
        {
            Executor.Shutdown(gracePeriod);
        }

        public Task GracefulShutdown(TimeSpan gracePeriod)
        {
            Shutdown(gracePeriod);
            return TaskRunner.Delay(gracePeriod);
        }

        public void Stop()
        {
            Executor.Shutdown();
        }

        #region IDisposable members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool isDisposing)
        {
            if (!WasDisposed)
            {
                if (isDisposing)
                {
                    Shutdown(TimeSpan.Zero);
                    _threads = null;
                }
            }

            WasDisposed = true;
        }

        public IFiber Clone()
        {
            return new DedicatedThreadPoolFiber(Executor.Clone(), _numThreads);
        }

        #endregion
    }
}