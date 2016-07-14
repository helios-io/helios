// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Helios.Concurrency
{
    /// <summary>
    ///     Wraps a <see cref="IEventExecutor" /> inside a <see cref="TaskScheduler" />
    ///     and allow scheduling of <see cref="Task" /> instances onto that executor.
    /// </summary>
    public sealed class EventExecutorTaskScheduler : TaskScheduler
    {
        private readonly IEventExecutor _executor;
        private bool _started;

        public EventExecutorTaskScheduler(IEventExecutor executor)
        {
            _executor = executor;
        }

        protected override void QueueTask(Task task)
        {
            if (_started)
            {
                _executor.Execute(new TaskQueueNode(this, task));
            }
            else
            {
                // hack: enables this executor to be seen as default on Executor's worker thread.
                // This is a special case for SingleThreadEventExecutor.Loop initiated task.
                // see: https://github.com/Azure/DotNetty/blob/dev/src/DotNetty.Common/Concurrency/ExecutorTaskScheduler.cs
                _started = true;
                TryExecuteTask(task);
            }
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (taskWasPreviouslyQueued || !_executor.InEventLoop)
                return false;
            return TryExecuteTask(task);
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return null;
        }

        protected override bool TryDequeue(Task task)
        {
            return false;
        }

        private sealed class TaskQueueNode : IRunnable
        {
            private readonly EventExecutorTaskScheduler _scheduler;
            private readonly Task _task;

            public TaskQueueNode(EventExecutorTaskScheduler scheduler, Task task)
            {
                _scheduler = scheduler;
                _task = task;
            }

            public void Run()
            {
                _scheduler.TryExecuteTask(_task);
            }
        }
    }
}