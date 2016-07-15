// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Helios.Concurrency
{
    /// <summary>
    ///     Abstract base class for <see cref="IEventExecutor" /> implementations
    /// </summary>
    public abstract class AbstractEventExecutor : IEventExecutor
    {
        protected static readonly TimeSpan DefaultShutdownQuietPeriod = TimeSpan.FromSeconds(2);
        protected static readonly TimeSpan DefaultShutdownTimeout = TimeSpan.FromSeconds(15);

        public bool InEventLoop => IsInEventLoop(Thread.CurrentThread);
        public abstract bool IsInEventLoop(Thread thread);
        public abstract Task TerminationTask { get; }
        public abstract bool IsShuttingDown { get; }
        public abstract bool IsShutDown { get; }
        public abstract bool IsTerminated { get; }
        public abstract void Execute(IRunnable task);

        public void Execute(Action<object> action, object state)
        {
            Execute(new StateActionTaskQueueNode(action, state));
        }

        public void Execute(Action action)
        {
            Execute(new ActionTaskQueueItem(action));
        }

        public void Execute(Action<object, object> action, object context, object state)
        {
            Execute(new StateAndContextActionTaskQueueNode(action, context, state));
        }

        public Task<T> SubmitAsync<T>(Func<T> func)
        {
            return SubmitAsync(func, CancellationToken.None);
        }

        public Task<T> SubmitAsync<T>(Func<T> func, CancellationToken cancellationToken)
        {
            var queueItem = new FuncTaskQueueItem<T>(func, cancellationToken);
            Execute(queueItem);
            return queueItem.Task;
        }

        public Task<T> SubmitAsync<T>(Func<object, T> func, object state)
        {
            return SubmitAsync(func, state, CancellationToken.None);
        }

        public Task<T> SubmitAsync<T>(Func<object, T> func, object state, CancellationToken cancellationToken)
        {
            var queueItem = new StateFuncWithTaskQueueItem<T>(func, state, cancellationToken);
            Execute(queueItem);
            return queueItem.Task;
        }

        public Task<T> SubmitAsync<T>(Func<object, object, T> func, object context, object state)
        {
            return SubmitAsync(func, context, state, CancellationToken.None);
        }

        public Task<T> SubmitAsync<T>(Func<object, object, T> func, object context, object state,
            CancellationToken cancellationToken)
        {
            var queueItem = new StateAndContextFuncWithTaskQueueItem<T>(func, context, state, cancellationToken);
            Execute(queueItem);
            return queueItem.Task;
        }

        public virtual IScheduledTask Schedule(Action action, TimeSpan delay)
        {
            throw new NotSupportedException();
        }

        public virtual IScheduledTask Schedule(Action<object> action, object state, TimeSpan delay)
        {
            throw new NotSupportedException();
        }

        public virtual IScheduledTask Schedule(Action<object, object> action, object context, object state,
            TimeSpan delay)
        {
            throw new NotSupportedException();
        }

        public virtual Task ScheduleAsync(Action<object> action, object state, TimeSpan delay,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual Task ScheduleAsync(Action<object> action, object state, TimeSpan delay)
        {
            return ScheduleAsync(action, state, delay, CancellationToken.None);
        }

        public virtual Task ScheduleAsync(Action action, TimeSpan delay, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual Task ScheduleAsync(Action action, TimeSpan delay)
        {
            return ScheduleAsync(action, delay, CancellationToken.None);
        }

        public virtual Task ScheduleAsync(Action<object, object> action, object context, object state, TimeSpan delay)
        {
            return ScheduleAsync(action, context, state, delay, CancellationToken.None);
        }

        public virtual Task ScheduleAsync(Action<object, object> action, object context, object state, TimeSpan delay,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task GracefulShutdownAsync()
        {
            return GracefulShutdownAsync(DefaultShutdownQuietPeriod, DefaultShutdownTimeout);
        }

        public abstract Task GracefulShutdownAsync(TimeSpan quietPeriod, TimeSpan timeout);
        public abstract IEventExecutor Unwrap();

        #region Task queueing data structures

        protected abstract class RunnableTaskQueueItem : IRunnable
        {
            public abstract void Run();
        }

        private sealed class ActionTaskQueueItem : RunnableTaskQueueItem
        {
            private readonly Action _action;

            public ActionTaskQueueItem(Action action)
            {
                _action = action;
            }

            public override void Run()
            {
                _action();
            }
        }

        private sealed class StateActionTaskQueueNode : RunnableTaskQueueItem
        {
            private readonly Action<object> _action;
            private readonly object _state;

            public StateActionTaskQueueNode(Action<object> action, object state)
            {
                _action = action;
                _state = state;
            }

            public override void Run()
            {
                _action(_state);
            }
        }

        private sealed class StateAndContextActionTaskQueueNode : RunnableTaskQueueItem
        {
            private readonly Action<object, object> _action;
            private readonly object _context;
            private readonly object _state;

            public StateAndContextActionTaskQueueNode(Action<object, object> action, object context, object state)
            {
                _action = action;
                _context = context;
                _state = state;
            }

            public override void Run()
            {
                _action(_context, _state);
            }
        }

        /// <summary>
        ///     Underlying <see cref="IRunnable" /> type used for executing items which return <see cref="Task" /> instances.
        /// </summary>
        /// <typeparam name="T">The return type of the <see cref="Task{T}" /></typeparam>
        private abstract class FuncTaskQueueItemBase<T> : RunnableTaskQueueItem
        {
            private readonly CancellationToken _cancellationToken;
            private readonly TaskCompletionSource<T> _promise;

            protected FuncTaskQueueItemBase(TaskCompletionSource<T> promise, CancellationToken cancellationToken)
            {
                _promise = promise;
                _cancellationToken = cancellationToken;
            }

            public Task<T> Task => _promise.Task;

            public override void Run()
            {
                // bail early if the task has been cancelled
                if (_cancellationToken.IsCancellationRequested)
                {
                    _promise.TrySetCanceled();
                    return;
                }

                try
                {
                    var result = RunInternal();
                    _promise.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    _promise.TrySetException(ex);
                }
            }

            protected abstract T RunInternal();
        }

        private sealed class FuncTaskQueueItem<T> : FuncTaskQueueItemBase<T>
        {
            private readonly Func<T> _function;

            public FuncTaskQueueItem(Func<T> function, CancellationToken cancellationToken)
                : base(new TaskCompletionSource<T>(), cancellationToken)
            {
                _function = function;
            }

            protected override T RunInternal()
            {
                return _function();
            }
        }

        private sealed class StateFuncWithTaskQueueItem<T> : FuncTaskQueueItemBase<T>
        {
            private readonly Func<object, T> _function;

            public StateFuncWithTaskQueueItem(Func<object, T> function, object state,
                CancellationToken cancellationToken)
                : base(new TaskCompletionSource<T>(state), cancellationToken)
            {
                _function = function;
            }

            protected override T RunInternal()
            {
                return _function(Task.AsyncState);
            }
        }

        private sealed class StateAndContextFuncWithTaskQueueItem<T> : FuncTaskQueueItemBase<T>
        {
            private readonly object _context;
            private readonly Func<object, object, T> _function;

            public StateAndContextFuncWithTaskQueueItem(Func<object, object, T> function, object context, object state,
                CancellationToken cancellationToken)
                : base(new TaskCompletionSource<T>(state), cancellationToken)
            {
                _function = function;
                _context = context;
            }

            protected override T RunInternal()
            {
                return _function(_context, Task.AsyncState);
            }
        }

        #endregion
    }
}