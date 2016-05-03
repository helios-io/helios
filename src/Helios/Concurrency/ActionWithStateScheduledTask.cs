using System;
using Helios.Util.TimedOps;

namespace Helios.Concurrency
{
    sealed class ActionWithStateScheduledTask : ScheduledTask
    {
        private readonly Action<object> _action;

        public ActionWithStateScheduledTask(AbstractScheduledEventExecutor executor, Action<object> action, object state, PreciseDeadline deadline) : base(executor, deadline, new TaskCompletionSource(state))
        {
            _action = action;
        }

        protected override void Execute()
        {
            _action(Completion.AsyncState);
        }
    }
}