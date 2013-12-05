using System;
using System.Collections.Generic;
using System.Linq;
using Helios.Core.Util;
using Helios.Core.Util.TimedOps;

namespace Helios.Core.Ops.Executors
{
    public class TryCatchExecutor : IExecutor
    {
        public TryCatchExecutor() : this(exception => { })
        {
        }

        public TryCatchExecutor(Action<Exception> callback)
        {
            _exceptionCallback = callback;
            AcceptingJobs = true;
        }

        private readonly Action<Exception> _exceptionCallback;
        protected ScheduledValue<bool> ScheduledValue;

        public bool AcceptingJobs
        {
            get
            {
                return ScheduledValue.Value;
            }
            set
            {
                ScheduledValue = value;
            }
        }

        public void Execute(Action op)
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

        public void Execute(IList<Action> op)
        {
            Execute(op, null);
        }

        public void Execute(IList<Action> ops, Action<IEnumerable<Action>> remainingOps)
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

        public void Shutdown()
        {
            AcceptingJobs = false;
            ScheduledValue.Cancel();
        }

        public void Shutdown(TimeSpan gracePeriod)
        {
            ScheduledValue.Schedule(false, gracePeriod);
        }

        ~TryCatchExecutor()
        {
            if(!ScheduledValue.WasDisposed)
                ScheduledValue.Dispose();
        }
    }
}
