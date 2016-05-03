using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Helios.Util.TimedOps;

namespace Helios.Concurrency
{
    abstract class ScheduledTask : IScheduledRunnable
    {
        const int CancellationProhibited = 1;
        const int CancellationRequested = 1 << 1;

        protected readonly TaskCompletionSource Promise;
        protected readonly AbstractScheduledEventExecutor Executor;
        private int _volatileCancellationState;

        protected ScheduledTask(AbstractScheduledEventExecutor executor, PreciseDeadline deadline, TaskCompletionSource promise)
        {
            Executor = executor;
            Deadline = deadline;
            Promise = promise;
        }

        public virtual void Run()
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
            int cancellationState = Volatile.Read(ref this._volatileCancellationState);
            int oldCancellationState;
            do
            {
                oldCancellationState = cancellationState;
                if ((cancellationState & illegalBits) != 0)
                {
                    return false;
                }
                cancellationState = Interlocked.CompareExchange(ref this._volatileCancellationState, cancellationState | newBits, cancellationState);
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