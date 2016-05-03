using System;
using System.Threading;
using Helios.Util.TimedOps;

namespace Helios.Concurrency
{
    abstract class ScheduledAsyncTask : ScheduledTask
    {
        private readonly CancellationToken _cancellationToken;
        private CancellationTokenRegistration _cancellationTokenRegistration;

        private static readonly Action<object> CancellationAction = s => ((ScheduledAsyncTask) s).Cancel();

        protected ScheduledAsyncTask(AbstractScheduledEventExecutor executor, PreciseDeadline deadline, TaskCompletionSource promise, CancellationToken cancellationToken) 
            : base(executor, deadline, promise)
        {
            _cancellationToken = cancellationToken;
            _cancellationTokenRegistration = cancellationToken.Register(CancellationAction, this);
        }

        public override void Run()
        {
            _cancellationTokenRegistration.Dispose();
            if (_cancellationToken.IsCancellationRequested)
            {
                Promise.TrySetCanceled();
            }
            else
            {
                base.Run();
            }
        }
    }
}