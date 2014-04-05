using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public Task ExecuteAsync(Action op)
        {
            return Task.Run(op);
        }

        public virtual void Execute(IList<Action> op)
        {
            Execute(op, null);
        }

        public Task ExecuteAsync(IList<Action> op)
        {
            return ExecuteAsync(op, null);
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

        public Task ExecuteAsync(IList<Action> ops, Action<IEnumerable<Action>> remainingOps)
        {
            return Task.Run(() =>
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
            });
        }

        public virtual void Shutdown()
        {
            Deadline = Deadline.Now;
        }

        public virtual void Shutdown(TimeSpan gracePeriod)
        {
            Deadline = Deadline.Now + gracePeriod;
        }

        public Task GracefulShutdown(TimeSpan gracePeriod)
        {
            Shutdown(gracePeriod);
            return Task.Delay(gracePeriod);
        }
    }
}