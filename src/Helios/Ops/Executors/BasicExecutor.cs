using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;
using Helios.Util;
using Helios.Util.Concurrency;
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
            return TaskRunner.Run(op);
        }

        public virtual void Execute(IList<Action> op)
        {
            Execute(op, null);
        }

        public void Execute(Task task)
        {
            if (!AcceptingJobs) return;

            task.RunSynchronously();
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
            return TaskRunner.Run(() =>
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
            return TaskRunner.Delay(gracePeriod);
        }

        public bool InThread(Thread thread)
        {
            return (Thread.CurrentThread.ManagedThreadId == thread.ManagedThreadId);
        }

        public IExecutor Clone()
        {
            return new BasicExecutor();
        }
    }
}