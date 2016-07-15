// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Threading.Tasks;
using Helios.Ops;
using Helios.Ops.Executors;

namespace Helios.Concurrency.Impl
{
    /// <summary>
    ///     IFiber implementation that doesn't use any form of concurrency under the hood
    /// </summary>
    public class SynchronousFiber : IFiber
    {
        public SynchronousFiber() : this(new BasicExecutor())
        {
        }

        public SynchronousFiber(IExecutor executor)
        {
            Executor = executor ?? new BasicExecutor();
            Running = true;
        }

        public IExecutor Executor { get; private set; }

        public bool Running { get; set; }
        public bool WasDisposed { get; private set; }

        public void Add(Action op)
        {
            if (Running)
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
            Running = false;
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


        public IFiber Clone()
        {
            return new SynchronousFiber(Executor.Clone());
        }

        #region IDisposable members

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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}