using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using Helios.Logging;
using Helios.Util;
using Helios.Util.TimedOps;

namespace Helios.Concurrency
{
    /// <summary>
    /// A single-threaded <see cref="IEventExecutor"/>
    /// </summary>
    public class SingleThreadEventExecutor : AbstractEventExecutor, IDisposable
    {
        const int ST_NOT_STARTED = 1;
        const int ST_STARTED = 2;
        const int ST_SHUTTING_DOWN = 3;
        const int ST_SHUTDOWN = 4;
        const int ST_TERMINATED = 5;

        const string DefaultWorkerThreadName = "SingleThreadEventExecutor";

        private static readonly ILogger Logger = LoggingFactory.GetLogger<SingleThreadEventExecutor>();
        private readonly ConcurrentQueue<IRunnable> _taskQueue = new ConcurrentQueue<IRunnable>();
        private Thread _workerThread;
        private readonly ManualResetEventSlim _emptyQueueEvent = new ManualResetEventSlim();
        volatile int _runningState = 0;
        private readonly TaskCompletionSource<int> _terminationCompletionSource;
        private bool _disposed;
        private readonly TaskScheduler _scheduler;
        private TimeSpan _gracefulShutdownQuietPeriod;
        private PreciseDeadline _gracefulShutdownTimeout;
        private TimeSpan _lastExecutionTime;
        private readonly TimeSpan _breakoutInterval;

        public SingleThreadEventExecutor(string threadName, TimeSpan breakoutInterval)
        {
            _terminationCompletionSource = new TaskCompletionSource<int>();
            _scheduler = new EventExecutorTaskScheduler(this);
            _breakoutInterval = breakoutInterval;
            _workerThread = new Thread(Loop)
            {
                IsBackground = true,
                Name = string.IsNullOrEmpty(threadName) ? DefaultWorkerThreadName : threadName
            };
            _workerThread.Start();
        }

        public override bool IsInEventLoop(Thread thread)
        {
            return _workerThread == thread;
        }

        public TaskScheduler Scheduler => _scheduler;

        public override Task TerminationTask => _terminationCompletionSource.Task;
        public override bool IsShuttingDown => _runningState >= ST_SHUTTING_DOWN;
        public override bool IsShutDown => _runningState >= ST_SHUTDOWN;
        public override bool IsTerminated => _runningState >= ST_TERMINATED;

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
            bool inEventLoop = InEventLoop;
            while (true)
            {
                if (IsShuttingDown)
                    return TerminationTask;
                int newState;
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
            return TerminationTask;
        }

        private void Loop()
        {
            Task.Factory.StartNew(() =>
            {
                if (Interlocked.CompareExchange(ref _runningState, ST_STARTED, ST_NOT_STARTED) == ST_NOT_STARTED)
                {
                    while (!this.ConfirmShutdown())
                    {
                        this.RunAllTasks(_breakoutInterval);
                    }

                    this.CleanupAndShutdown(true);
                }
            }, CancellationToken.None, TaskCreationOptions.None, Scheduler);
        }

        protected bool ConfirmShutdown()
        {
            if (!IsShuttingDown)
                return false;
            if (!InEventLoop)
                throw new InvalidOperationException("ConfirmShutdown must be invoked from this event loop");

            if (RunAllTasks())
            {
                if (IsShutDown)
                {
                    // we're shut down - no more new tasks
                    return true;
                }

                return false;
            }

            var currentTime = MonotonicClock.ElapsedHighRes;
            if (IsShutDown || _gracefulShutdownTimeout.IsOverdue)
                return true;

            if (currentTime - _lastExecutionTime <= _gracefulShutdownQuietPeriod)
            {
                // Check if any tasks were added to the queue every 100ms
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
                Logger.Error("Buggy {0} implementation; {1}.ConfirmShutdown() must be called " + "before run() implementation terminates.",
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

        private IRunnable PollTask()
        {
            Contract.Assert(InEventLoop);
            IRunnable task;
            if (!_taskQueue.TryDequeue(out task))
            {
                _emptyQueueEvent.Reset();
                if (!_taskQueue.TryDequeue(out task)) // revisit queue as producer might have put a task in meanwhile
                {
                    _emptyQueueEvent.Wait(); // wait until work is put into the queue
                    _taskQueue.TryDequeue(out task);
                }
            }
            return task;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        public void Dispose(bool isDisposing)
        {
            if (!_disposed)
            {
                if (isDisposing)
                {
                    _workerThread = null;
                }
            }

            _disposed = true;
        }
    }
}