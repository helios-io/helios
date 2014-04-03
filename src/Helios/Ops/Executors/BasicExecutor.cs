using System;
using System.Collections.Generic;
using System.Linq;
using Helios.Util;
using Helios.Util.TimedOps;

namespace Helios.Ops.Executors
{
    /// <summary>
    /// Basic synchronous executor
    /// </summary>
    public class BasicExecutor : IExecutor
    {
        public BasicExecutor()
        {
            Deadline = Util.TimedOps.Deadline.Never;
        }

        protected Deadline Deadline;
        public bool AcceptingJobs
        {
            get
            {
                return Deadline.HasTimeLeft;
            }
        }

        public virtual void Execute(Action op)
        {
            if (!AcceptingJobs) return;

            op();
        }

        public virtual void Execute(IList<Action> op)
        {
            Execute(op, null);
        }

        public virtual void Execute(IList<Action> ops, Action<IEnumerable<Action>> remainingOps)
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

        public virtual void Shutdown()
        {
            Deadline = Deadline.Now;
        }

        public virtual void Shutdown(TimeSpan gracePeriod)
        {
            Deadline = Deadline.Now + gracePeriod;
        }
    }
}