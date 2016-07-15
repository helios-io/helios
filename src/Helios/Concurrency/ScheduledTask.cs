// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Helios.Util.TimedOps;

namespace Helios.Concurrency
{
    internal abstract class ScheduledTask : IScheduledRunnable
    {
        private const int CancellationProhibited = 1;
        private const int CancellationRequested = 1 << 1;
        protected readonly AbstractScheduledEventExecutor Executor;

        protected readonly TaskCompletionSource Promise;
        private int _volatileCancellationState;

        protected ScheduledTask(AbstractScheduledEventExecutor executor, PreciseDeadline deadline,
            TaskCompletionSource promise)
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

        public bool Cancel()
        {
            if (!AtomicCancellationStateUpdate(CancellationProhibited, CancellationRequested))
            {
                return false;
            }

            var canceled = Promise.TrySetCanceled();
            if (canceled)
            {
                Executor.RemoveScheduled(this);
            }
            return canceled;
        }

        public PreciseDeadline Deadline { get; }
        public Task Completion => Promise.Task;

        public TaskAwaiter GetAwaiter()
        {
            return Completion.GetAwaiter();
        }

        public int CompareTo(IScheduledRunnable other)
        {
            return Deadline.CompareTo(other.Deadline);
        }

        protected abstract void Execute();

        private bool TrySetUncancelable()
        {
            return AtomicCancellationStateUpdate(CancellationProhibited, CancellationRequested);
        }

        private bool AtomicCancellationStateUpdate(int newBits, int illegalBits)
        {
            var cancellationState = Volatile.Read(ref _volatileCancellationState);
            int oldCancellationState;
            do
            {
                oldCancellationState = cancellationState;
                if ((cancellationState & illegalBits) != 0)
                {
                    return false;
                }
                cancellationState = Interlocked.CompareExchange(ref _volatileCancellationState,
                    cancellationState | newBits, cancellationState);
            } while (cancellationState != oldCancellationState);

            return true;
        }
    }
}