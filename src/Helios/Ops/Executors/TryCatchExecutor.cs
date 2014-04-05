using System;
using System.Collections.Generic;
using System.Linq;
using Helios.Util;

namespace Helios.Ops.Executors
{
    public class TryCatchExecutor : BasicExecutor
    {
        public TryCatchExecutor() : this(exception => { })
        {
        }

        public TryCatchExecutor(Action<Exception> callback)
        {
            _exceptionCallback = callback;
        }

        private readonly Action<Exception> _exceptionCallback;


        public override void Execute(Action op)
        {
            try
            {
                if (!AcceptingJobs) return;
                op();
            }
            catch (Exception ex)
            {
                _exceptionCallback(ex);
            }
        }

        public override void Execute(IList<Action> ops, Action<IEnumerable<Action>> remainingOps)
        {
            var i = 0;
            try
            {
                for (; i < ops.Count; i++)
                {
                    if (!AcceptingJobs)
                    {
// ReSharper disable once AccessToModifiedClosure
                        remainingOps.NotNull(obj => remainingOps(ops.Skip(i + 1)));
                        break;
                    }

                    ops[i]();
                }
            }
            catch (Exception ex)
            {
                remainingOps.NotNull(obj => remainingOps(ops.Skip(i + 1)));
                _exceptionCallback(ex);
            }
        }
    }
}
