using System;
using System.Collections.Generic;
using Helios.Core.Util.Collections;

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
        }

        public bool AcceptingJobs { get; private set; }
        public void Execute(Action op)
        {
            if (!AcceptingJobs) return;

            op();
        }

        public void Execute(IList<Action> op)
        {
            IList<Action> remaining;
            Execute(op, out remaining);
        }

        public void Execute(IList<Action> ops, out IList<Action> remainingOps)
        {
            for (var i = 0; i < ops.Count; i++)
            {
                if (!AcceptingJobs)
                {
                    remainingOps = ops.Subset(i, ops.Count - i);
                    break;
                }

                ops[i]();
            }

            remainingOps = new List<Action>(0); //empty list
        }

        public void Shutdown()
        {
            AcceptingJobs = false;
        }
    }
}