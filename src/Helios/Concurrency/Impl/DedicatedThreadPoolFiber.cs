// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
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

        private volatile IExecutor _executor;
        private readonly DedicatedThreadPool _threadPool;

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
            _threadPool = new DedicatedThreadPool(new DedicatedThreadPoolSettings(numThreads));
            Running = true;
        }

        public IExecutor Executor
        {
            get { return _executor; }
            set { _executor = value; }
        }

        public bool WasDisposed { get; private set; }

        public bool Running { get; set; }

        public void Add(Action op)
        {
            if (Running)
                _threadPool.QueueUserWorkItem(() => Executor.Execute(op));
        }

        public void SwapExecutor(IExecutor executor)
        {
            //Shut down the previous executor gracefully (in case there's thread-contention)
            Executor.GracefulShutdown(TimeSpan.FromSeconds(3));
            Executor = executor;
        }

        public void Shutdown(TimeSpan gracePeriod)
        {
            Running = false;
            Executor.Shutdown(gracePeriod);
        }

        public Task GracefulShutdown(TimeSpan gracePeriod)
        {
            Shutdown(gracePeriod);
            return TaskRunner.Delay(gracePeriod).ContinueWith(tr => Stop());
        }

        public void Stop()
        {
            _threadPool.Dispose();
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
                    _threadPool.Dispose();
                    Shutdown(TimeSpan.Zero);
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