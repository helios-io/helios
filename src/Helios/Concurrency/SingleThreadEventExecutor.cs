// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Helios.Logging;
using Helios.Util;
using Helios.Util.TimedOps;

namespace Helios.Concurrency
{
    /// <summary>
    ///     A single-threaded <see cref="IEventExecutor" />
    /// </summary>
    public class SingleThreadEventExecutor : AbstractScheduledEventExecutor
    {
        private const int ST_NOT_STARTED = 1;
        private const int ST_STARTED = 2;
        private const int ST_SHUTTING_DOWN = 3;
        private const int ST_SHUTDOWN = 4;
        private const int ST_TERMINATED = 5;

        private const string DefaultWorkerThreadName = "SingleThreadEventExecutor";

        private static readonly ILogger Logger = LoggingFactory.GetLogger<SingleThreadEventExecutor>();

        private static readonly Action<object, object> AddEventHookAction = (hook, hooks) =>
        {
            var run = hook as IRunnable;
            var shutdownHooks = hooks as HashSet<IRunnable>;
            if (run == null || shutdownHooks == null) return;
            shutdownHooks.Add(run);
        };

        private static readonly Action<object, object> RemoveEventHookAction = (hook, hooks) =>
        {
            var run = hook as IRunnable;
            var shutdownHooks = hooks as HashSet<IRunnable>;
            if (run == null || shutdownHooks == null) return;
            shutdownHooks.Remove(run);
        };

        private readonly TimeSpan _breakoutInterval;
        private readonly ManualResetEventSlim _emptyQueueEvent = new ManualResetEventSlim();
        private readonly HashSet<IRunnable> _shutdownHooks = new HashSet<IRunnable>();
        private readonly ConcurrentQueue<IRunnable> _taskQueue = new ConcurrentQueue<IRunnable>();
        private readonly TaskCompletionSource<int> _terminationCompletionSource;
        private TimeSpan _gracefulShutdownQuietPeriod;
        private PreciseDeadline _gracefulShutdownTimeout;
        private TimeSpan _lastExecutionTime;
        private volatile int _runningState = ST_NOT_STARTED;
        private readonly Thread _workerThread;

        public SingleThreadEventExecutor(string threadName, TimeSpan breakoutInterval)
        {
            _terminationCompletionSource = new TaskCompletionSource<int>();
            Scheduler = new EventExecutorTaskScheduler(this);
            _breakoutInterval = breakoutInterval;
            _workerThread = new Thread(Loop)
            {
                IsBackground = true,
                Name = string.IsNullOrEmpty(threadName) ? DefaultWorkerThreadName : threadName
            };
            _workerThread.Start();
        }

        public TaskScheduler Scheduler { get; }

        public override Task TerminationTask => _terminationCompletionSource.Task;
        public override bool IsShuttingDown => _runningState >= ST_SHUTTING_DOWN;
        public override bool IsShutDown => _runningState >= ST_SHUTDOWN;
        public override bool IsTerminated => _runningState >= ST_TERMINATED;

        public override bool IsInEventLoop(Thread thread)
        {
            return _workerThread == thread;
        }

        public override void Execute(IRunnable task)
        {
            _taskQueue.Enqueue(task);
            if (!InEventLoop)
                _emptyQueueEvent.Set();
        }

        public override Task GracefulShutdownAsync(TimeSpan quietPeriod, TimeSpan timeout)
        {
            Contract.Requires(quietPeriod >= TimeSpan.Zero);
            Contract.Requires(timeout >= quietPeriod);

            if (IsShuttingDown)
                return TerminationTask;

            int oldState;
            var inEventLoop = InEventLoop;
            bool wakeup;
            while (true)
            {
                if (IsShuttingDown)
                    return TerminationTask;
                int newState;
                wakeup = true;
                oldState = _runningState;
                if (inEventLoop)
                {
                    newState = ST_SHUTTING_DOWN;
                }
                else
                {
                    switch (oldState)
                    {
                        case ST_NOT_STARTED:
                        case ST_STARTED:
                            newState = ST_SHUTTING_DOWN;
                            break;
                        default:
                            newState = oldState;
                            wakeup = false;
                            break;
                    }
                }
                if (Interlocked.CompareExchange(ref _runningState, newState, oldState) == oldState)
                {
                    break;
                }
            }

            _gracefulShutdownQuietPeriod = MonotonicClock.ElapsedHighRes + quietPeriod;
            _gracefulShutdownTimeout = PreciseDeadline.Now + timeout;

            if (wakeup)
            {
                Wakeup(inEventLoop);
            }

            return TerminationTask;
        }

        public override IEventExecutor Unwrap()
        {
            return this;
        }

        private void Loop()
        {
            Task.Factory.StartNew(() =>
            {
                Interlocked.CompareExchange(ref _runningState, ST_STARTED, ST_NOT_STARTED);

                while (!ConfirmShutdown())
                {
                    RunAllTasks(_breakoutInterval);
                }

                CleanupAndShutdown(true);
            }, CancellationToken.None, TaskCreationOptions.None, Scheduler);
        }

        protected bool ConfirmShutdown()
        {
            if (!IsShuttingDown)
                return false;
            if (!InEventLoop)
                throw new InvalidOperationException("ConfirmShutdown must be invoked from this event loop");

            if (RunAllTasks() || RunShutdownHooks())
            {
                if (IsShutDown)
                {
                    // we're shut down - no more new tasks
                    return true;
                }

                // There were tasks in the queue. Wait a little bit more until no tasks are queued for the quiet period.
                Wakeup(true);
                return false;
            }

            var currentTime = MonotonicClock.ElapsedHighRes;
            if (IsShutDown || _gracefulShutdownTimeout.IsOverdue)
                return true;

            if (currentTime - _lastExecutionTime <= _gracefulShutdownQuietPeriod)
            {
                // Check if any tasks were added to the queue every 100ms
                Wakeup(true);
                Thread.Sleep(100);
                return false;
            }

            // No tasks were added in the last quiet period, should be safe to shut down
            return true;
        }

        protected void CleanupAndShutdown(bool success)
        {
            while (true)
            {
                var oldState = _runningState;
                if (oldState >= ST_SHUTTING_DOWN ||
                    Interlocked.CompareExchange(ref _runningState, ST_SHUTTING_DOWN, oldState) == oldState)
                {
                    break;
                }
            }

            // Check if confirmShutdown() was called at the end of the loop.
            if (success && _gracefulShutdownTimeout == PreciseDeadline.Zero)
            {
                Logger.Error(
                    "Buggy {0} implementation; {1}.ConfirmShutdown() must be called " +
                    "before run() implementation terminates.",
                    typeof(IEventExecutor).Name,
                    typeof(SingleThreadEventExecutor).Name);
            }

            try
            {
                // Run all remaining tasks and shutdown hooks
                while (true)
                {
                    if (ConfirmShutdown())
                    {
                        break;
                    }
                }
            }
            finally
            {
                Interlocked.Exchange(ref _runningState, ST_TERMINATED);
                if (!_taskQueue.IsEmpty)
                {
                    Logger.Warning("An event executor terminated with non-empty task queue ({0})", _taskQueue.Count);
                }

                // complete the termination promise
                _terminationCompletionSource.SetResult(0);
            }
        }

        protected bool RunAllTasks()
        {
            FetchFromScheduledTaskQueue();
            var task = PollTask();
            if (task == null)
                return false;

            while (true)
            {
                try
                {
                    task.Run();
                }
                catch (Exception ex)
                {
                    Logger.Warning("A task raised an exception: {0}", ex);
                }

                task = PollTask();
                if (task == null)
                {
                    _lastExecutionTime = MonotonicClock.ElapsedHighRes;
                    return true;
                }
            }
        }

        private bool RunAllTasks(TimeSpan breakoutInternval)
        {
            FetchFromScheduledTaskQueue();
            var task = PollTask();
            if (task == null)
                return false;

            var runTasks = 0L;
            var timeout = PreciseDeadline.Now + breakoutInternval;
            while (true)
            {
                try
                {
                    task.Run();
                }
                catch (Exception ex)
                {
                    Logger.Warning("A task raised an exception: {0}", ex);
                }

                runTasks++;

                // Check timeout every 64 tasks because nanoTime() is relatively expensive.
                // XXX: Hard-coded value - will make it configurable if it is really a problem.
                if ((runTasks & 0x3F) == 0)
                {
                    if (timeout.IsOverdue)
                    {
                        break;
                    }
                }

                task = PollTask();
                if (task == null)
                {
                    break;
                }
            }

            _lastExecutionTime = MonotonicClock.ElapsedHighRes;
            return true;
        }

        /// <summary>
        ///     Add a <see cref="IRunnable" /> which will be executed on the shutdown of this instance
        /// </summary>
        /// >
        public void AddShutdownHook(IRunnable hook)
        {
            if (InEventLoop)
            {
                _shutdownHooks.Add(hook);
            }
            else
            {
                Execute(AddEventHookAction, hook, _shutdownHooks);
            }
        }

        /// <summary>
        ///     Remove a previously added <see cref="IRunnable" /> as a shutdown hook
        /// </summary>
        /// >
        public void RemoveShutdownHook(IRunnable hook)
        {
            if (InEventLoop)
            {
                _shutdownHooks.Remove(hook);
            }
            else
            {
                Execute(RemoveEventHookAction, hook, _shutdownHooks);
            }
        }

        private bool RunShutdownHooks()
        {
            var ran = false;
            // Note shutdown hooks can add / remove shutdown hooks.
            while (_shutdownHooks.Any())
            {
                var copy = new List<IRunnable>(_shutdownHooks);
                _shutdownHooks.Clear();
                foreach (var task in copy)
                {
                    try
                    {
                        task.Run();
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning("Shutdown hook raised an exception. Cause: {0}", ex);
                    }
                    finally
                    {
                        ran = true;
                    }
                }
            }

            if (ran)
            {
                _lastExecutionTime = MonotonicClock.ElapsedHighRes;
            }

            return ran;
        }

        private void FetchFromScheduledTaskQueue()
        {
            if (HasScheduledTasks())
            {
                var tickCount = MonotonicClock.GetTicks();
                while (true)
                {
                    var scheduledTask = PollScheduledTask(tickCount);
                    if (scheduledTask == null)
                    {
                        break;
                    }

                    _taskQueue.Enqueue(scheduledTask);
                }
            }
        }

        private IRunnable PollTask()
        {
            Contract.Assert(InEventLoop);
            IRunnable task;
            if (!_taskQueue.TryDequeue(out task))
            {
                _emptyQueueEvent.Reset();
                if (!_taskQueue.TryDequeue(out task) && !IsShuttingDown)
                    // revisit queue as producer might have put a task in meanwhile
                {
                    var nextScheduledTask = ScheduledTaskQueue.Peek();
                    if (nextScheduledTask != null)
                    {
                        var wakeupTimeout = new TimeSpan(nextScheduledTask.Deadline.When - MonotonicClock.GetTicks());
                        if (wakeupTimeout.Ticks > 0)
                        {
                            if (_emptyQueueEvent.Wait(wakeupTimeout))
                            {
                                // woken up before the next scheduled task was due
                                _taskQueue.TryDequeue(out task);
                            }
                        }
                    }
                    else
                    {
                        _emptyQueueEvent.Wait(); // wait until work is put into the queue
                        _taskQueue.TryDequeue(out task);
                    }
                }
            }
            return task;
        }

        protected void Wakeup(bool inEventLoop)
        {
            if (!InEventLoop || _runningState == ST_SHUTTING_DOWN)
            {
                Execute(WakeupTask.Instance);
            }
        }

        private class WakeupTask : IRunnable
        {
            public static readonly WakeupTask Instance = new WakeupTask();

            private WakeupTask()
            {
            }

            public void Run()
            {
                // do nothing
            }
        }
    }
}