using System;
using Helios.Util.TimedOps;

namespace Helios.Concurrency
{
    sealed class ActionScheduledTask : ScheduledTask
    {
        private readonly Action _action;
        public ActionScheduledTask(AbstractScheduledEventExecutor executor, Action action, PreciseDeadline deadline) : base(executor, deadline, new TaskCompletionSource())
        {
            _action = action;
        }

        protected override void Execute()
        {
            _action();
        }
    }
}