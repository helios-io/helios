using System;
using System.Collections.Generic;
using System.Linq;
using Helios.Core.Util;
using Helios.Core.Util.TimedOps;

namespace Helios.Core.Ops.Executors
{
    /// <summary>
    /// Basic synchronous executor
    /// </summary>
    public class BasicExecutor : IExecutor
    {
        public BasicExecutor()
        {
            AcceptingJobs = true;
            ScheduledValue.ScheduleFinished += (sender, args) => ScheduledValue.Dispose();
        }

        protected ScheduledValue<bool> ScheduledValue;

        public bool AcceptingJobs
        {
            get
            {
                return ScheduledValue.Value;
            }
            protected set
            {
                ScheduledValue = value;
            }
        }

        public void Execute(Action op)
        {
            if (!AcceptingJobs) return;

            op();
        }

        public void Execute(IList<Action> op)
        {
            Execute(op, null);
        }

        public void Execute(IList<Action> ops, Action<IEnumerable<Action>> remainingOps)
        {
            for (var i = 0; i < ops.Count; i++)
            {
                if (!AcceptingJobs)
                {
                    remainingOps.NotNull(obj => remainingOps(ops.Skip(i + 1)));
                    break;
                }

                ops[i]();
            }
        }

        public void Shutdown()
        {
            AcceptingJobs = false;
            ScheduledValue.Dispose();
        }

        public void Shutdown(TimeSpan gracePeriod)
        {
            ScheduledValue.Schedule(false, gracePeriod);
        }

        ~BasicExecutor()
        {
            if(!ScheduledValue.WasDisposed)
                ScheduledValue.Dispose();
        }
    }
}