using System;
using Helios.Util.TimedOps;

namespace Helios.Concurrency
{
    internal sealed class ActionWithStateAndContextScheduledTask : ScheduledTask
    {
        private readonly Action<object, object> _action;
        private readonly object _context;

        public ActionWithStateAndContextScheduledTask(AbstractScheduledEventExecutor executor, Action<object, object> action, object context, object state, PreciseDeadline deadline) : base(executor, deadline, new TaskCompletionSource(state))
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