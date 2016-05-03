using System;
using System.Threading;
using Helios.Util.TimedOps;

namespace Helios.Concurrency
{
    sealed class ActionScheduledAsyncTask : ScheduledAsyncTask
    {
        private readonly Action _action;

        public ActionScheduledAsyncTask(AbstractScheduledEventExecutor executor, Action action, PreciseDeadline deadline, CancellationToken cancellationToken) : base(executor, deadline, new TaskCompletionSource(), cancellationToken)
        {
            _action = action;
        }

        protected override void Execute()
        {
            _action();
        }
    }
}