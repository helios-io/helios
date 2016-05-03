using System;
using System.Threading;
using Helios.Util.TimedOps;

namespace Helios.Concurrency
{
    internal sealed class ActionWithStateAndContextScheduledAsyncTask : ScheduledAsyncTask
    {
        private readonly Action<object, object> _action;
        private readonly object _context;

        public ActionWithStateAndContextScheduledAsyncTask(AbstractScheduledEventExecutor executor, Action<object, object>  action, object context, object state, PreciseDeadline deadline, CancellationToken cancellationToken) : base(executor, deadline, new TaskCompletionSource(state), cancellationToken)
        {
            _action = action;
            _context = context;
        }

        protected override void Execute()
        {
            _action(_context, Completion.AsyncState);
        }
    }
}