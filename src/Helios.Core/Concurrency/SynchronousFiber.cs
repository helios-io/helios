using System;
using System.Timers;
using Helios.Core.Ops;

namespace Helios.Core.Concurrency
{
    public class SynchronousFiber : IFiber
    {
        bool _acceptingRequests;

        protected Timer ShutdownTimer;
        protected readonly IExecutor Executor;

        public SynchronousFiber(IExecutor executor)
        {
            _acceptingRequests = true;
            Executor = executor;
        }

        public void Add(Action op)
        {
            if(_acceptingRequests)
                op();
        }

        public void Shutdown(TimeSpan gracePeriod)
        {
            ShutdownTimer = new Timer(gracePeriod.TotalMilliseconds);

            ShutdownTimer.Elapsed += (sender, args) =>
            {
                Stop();
                ShutdownTimer.Stop();
                ShutdownTimer.Dispose();
                ShutdownTimer = null;
            };
        }

        public void Stop()
        {
            _acceptingRequests = false;
        }
    }
}