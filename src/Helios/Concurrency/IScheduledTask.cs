using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Helios.Util.TimedOps;

namespace Helios.Concurrency
{
    public interface IScheduledTask
    {
        bool Cancel();

        PreciseDeadline Deadline { get; }

        Task Completion { get; }

        TaskAwaiter GetAwaiter();
    }

    abstract class ScheduledTask : IScheduledRunnable
    {
        const int CancellationProhibited = 1;
        const int CancellationRequested = 1 << 1;

        protected readonly TaskCompletionSource Promise;
        protected readonly AbstractSchedulerEventExecutor Executor;
        private int volatileCancellationState;

        protected ScheduledTask(AbstractSchedulerEventExecutor executor, PreciseDeadline deadline, TaskCompletionSource promise)
        {
            Executor = executor;
            Deadline = deadline;
            Promise = promise;
        }

        public void Run()
        {
            if (TrySetUncancelable())
            {
                try
                {
                    Execute();
                    Promise.TryComplete();
                }
                catch (Exception ex)
                {
                    // todo: check for fatal
                    Promise.TrySetException(ex);
                }
            }
        }

        protected abstract void Execute();

        public bool Cancel()
        {
            if (!this.AtomicCancellationStateUpdate(CancellationProhibited, CancellationRequested))
            {
                return false;
            }

            bool canceled = this.Promise.TrySetCanceled();
            if (canceled)
            {
                this.Executor.RemoveScheduled(this);
            }
            return canceled;
        }

        public PreciseDeadline Deadline { get; }
        public Task Completion => Promise.Task;

        public TaskAwaiter GetAwaiter()
        {
            return Completion.GetAwaiter();
        }

        bool TrySetUncancelable()
        {
            return AtomicCancellationStateUpdate(CancellationProhibited, CancellationRequested);
        }

        bool AtomicCancellationStateUpdate(int newBits, int illegalBits)
        {
            int cancellationState = Volatile.Read(ref this.volatileCancellationState);
            int oldCancellationState;
            do
            {
                oldCancellationState = cancellationState;
                if ((cancellationState & illegalBits) != 0)
                {
                    return false;
                }
                cancellationState = Interlocked.CompareExchange(ref this.volatileCancellationState, cancellationState | newBits, cancellationState);
            }
            while (cancellationState != oldCancellationState);

            return true;
        }

        public int CompareTo(IScheduledRunnable other)
        {
            return Deadline.CompareTo(other.Deadline);
        }
    }
}
