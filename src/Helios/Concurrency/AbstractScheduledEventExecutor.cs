// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Helios.Util;
using Helios.Util.Collections;
using Helios.Util.Concurrency;
using Helios.Util.TimedOps;

namespace Helios.Concurrency
{
    /// <summary>
    ///     Abstract base class for <see cref="IEventExecutor" />s that need to support scheduling
    /// </summary>
    public abstract class AbstractScheduledEventExecutor : AbstractEventExecutor
    {
        private static readonly Action<object, object> EnqueueAction =
            (e, t) => ((AbstractScheduledEventExecutor) e).ScheduledTaskQueue.Enqueue((IScheduledRunnable) t);

        private static readonly Action<object, object> RemoveAction =
            (e, t) => ((AbstractScheduledEventExecutor) e).ScheduledTaskQueue.Remove((IScheduledRunnable) t);

        protected readonly PriorityQueue<IScheduledRunnable> ScheduledTaskQueue =
            new PriorityQueue<IScheduledRunnable>();

        protected static bool IsNullOrEmpty<T>(PriorityQueue<T> taskQueue) where T : class
        {
            return taskQueue == null || taskQueue.Count == 0;
        }

        protected virtual void CancelScheduledTasks()
        {
            Contract.Assert(InEventLoop);
            var scheduledTaskQueue = ScheduledTaskQueue;
            if (IsNullOrEmpty(scheduledTaskQueue))
            {
                return;
            }

            var tasks = scheduledTaskQueue.ToArray();
            foreach (var t in tasks)
            {
                t.Cancel();
            }
            ScheduledTaskQueue.Clear();
        }

        protected static long GetTicks()
        {
            return MonotonicClock.GetTicks();
        }

        protected IScheduledRunnable PollScheduledTask(long ticks)
        {
            Contract.Assert(InEventLoop);
            var scheduledTask = ScheduledTaskQueue.Peek();
            if (scheduledTask == null)
            {
                return null;
            }

            if (scheduledTask.Deadline.When <= ticks)
            {
                ScheduledTaskQueue.Dequeue();
                return scheduledTask;
            }
            return null;
        }

        protected PreciseDeadline NextScheduledTaskTicks()
        {
            var nextScheduledRunnable = PeekScheduledTask();
            return nextScheduledRunnable == null ? PreciseDeadline.MinusOne : nextScheduledRunnable.Deadline;
        }

        protected IScheduledRunnable PeekScheduledTask()
        {
            var scheduledTaskQueue = ScheduledTaskQueue;
            return IsNullOrEmpty(scheduledTaskQueue) ? null : scheduledTaskQueue.Peek();
        }

        protected IScheduledRunnable PollScheduledTask()
        {
            return PollScheduledTask(GetTicks());
        }

        protected bool HasScheduledTasks()
        {
            var scheduledTask = ScheduledTaskQueue.Peek();
            return scheduledTask != null && scheduledTask.Deadline.IsOverdue;
        }

        public override IScheduledTask Schedule(Action action, TimeSpan delay)
        {
            return Schedule(new ActionScheduledTask(this, action, new PreciseDeadline(delay)));
        }

        public override IScheduledTask Schedule(Action<object> action, object state, TimeSpan delay)
        {
            return Schedule(new ActionWithStateScheduledTask(this, action, state, new PreciseDeadline(delay)));
        }

        public override IScheduledTask Schedule(Action<object, object> action, object context, object state,
            TimeSpan delay)
        {
            return
                Schedule(new ActionWithStateAndContextScheduledTask(this, action, context, state,
                    new PreciseDeadline(delay)));
        }

        public override Task ScheduleAsync(Action action, TimeSpan delay, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return TaskEx.Cancelled;
            }

            if (!cancellationToken.CanBeCanceled)
            {
                return Schedule(action, delay).Completion;
            }

            return
                Schedule(new ActionScheduledAsyncTask(this, action, new PreciseDeadline(delay), cancellationToken))
                    .Completion;
        }

        public override Task ScheduleAsync(Action<object, object> action, object context, object state, TimeSpan delay,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return TaskEx.Cancelled;
            }

            if (!cancellationToken.CanBeCanceled)
            {
                return Schedule(action, context, state, delay).Completion;
            }

            return
                Schedule(new ActionWithStateAndContextScheduledAsyncTask(this, action, context, state,
                    new PreciseDeadline(delay), cancellationToken))
                    .Completion;
        }

        public override Task ScheduleAsync(Action<object> action, object state, TimeSpan delay,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return TaskEx.Cancelled;
            }

            if (!cancellationToken.CanBeCanceled)
            {
                return Schedule(action, state, delay).Completion;
            }

            return
                Schedule(new ActionWithStateScheduledAsyncTask(this, action, state, new PreciseDeadline(delay),
                    cancellationToken))
                    .Completion;
        }

        protected IScheduledRunnable Schedule(IScheduledRunnable task)
        {
            if (InEventLoop)
            {
                ScheduledTaskQueue.Enqueue(task);
            }
            else
            {
                Execute(EnqueueAction, this, task);
            }
            return task;
        }

        internal void RemoveScheduled(IScheduledRunnable scheduledTask)
        {
            if (InEventLoop)
            {
                ScheduledTaskQueue.Remove(scheduledTask);
            }
            else
            {
                Execute(RemoveAction, this, scheduledTask);
            }
        }
    }
}