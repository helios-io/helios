using System;
using System.Threading;
using Helios.Util.TimedOps;

namespace Helios.Concurrency
{
    sealed class ActionWithStateScheduledAsyncTask : ScheduledAsyncTask
    {
        private readonly Action<object> _action;

        public ActionWithStateScheduledAsyncTask(AbstractScheduledEventExecutor executor, Action<object> action, object state, PreciseDeadline deadline, CancellationToken cancellationToken) : base(executor, deadline, new TaskCompletionSource(state), cancellationToken)
        {
            _action = action;
        }

        protected override void Execute()
        {
            _action(Completion.AsyncState);
        }
    }
}