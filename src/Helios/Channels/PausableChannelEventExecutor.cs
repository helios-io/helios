// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Helios.Concurrency;

namespace Helios.Channels
{
    internal abstract class PausableChannelEventExecutor : IPausableEventExecutor
    {
        internal abstract IChannel Channel { get; }

        public IEventExecutor Executor => this;
        public bool InEventLoop => Unwrap().InEventLoop;

        public bool IsInEventLoop(Thread thread)
        {
            return Unwrap().IsInEventLoop(thread);
        }

        public Task TerminationTask => Unwrap().TerminationTask;
        public bool IsShuttingDown => Unwrap().IsShuttingDown;
        public bool IsShutDown => Unwrap().IsShutDown;
        public bool IsTerminated => Unwrap().IsTerminated;

        public void Execute(IRunnable task)
        {
            VerifyAcceptingNewTasks();
            Unwrap().Execute(task);
        }

        public void Execute(Action<object> action, object state)
        {
            VerifyAcceptingNewTasks();
            Unwrap().Execute(action, state);
        }

        public void Execute(Action action)
        {
            VerifyAcceptingNewTasks();
            Unwrap().Execute(action);
        }

        public void Execute(Action<object, object> action, object context, object state)
        {
            VerifyAcceptingNewTasks();
            Unwrap().Execute(action, context, state);
        }

        public Task<T> SubmitAsync<T>(Func<T> func)
        {
            VerifyAcceptingNewTasks();
            return Unwrap().SubmitAsync(func);
        }

        public Task<T> SubmitAsync<T>(Func<T> func, CancellationToken cancellationToken)
        {
            VerifyAcceptingNewTasks();
            return Unwrap().SubmitAsync(func, cancellationToken);
        }

        public Task<T> SubmitAsync<T>(Func<object, T> func, object state)
        {
            VerifyAcceptingNewTasks();
            return Unwrap().SubmitAsync(func, state);
        }

        public Task<T> SubmitAsync<T>(Func<object, T> func, object state, CancellationToken cancellationToken)
        {
            VerifyAcceptingNewTasks();
            return Unwrap().SubmitAsync(func, state, cancellationToken);
        }

        public Task<T> SubmitAsync<T>(Func<object, object, T> func, object context, object state)
        {
            VerifyAcceptingNewTasks();
            return Unwrap().SubmitAsync(func, context, state);
        }

        public Task<T> SubmitAsync<T>(Func<object, object, T> func, object context, object state,
            CancellationToken cancellationToken)
        {
            VerifyAcceptingNewTasks();
            return Unwrap().SubmitAsync(func, context, state, cancellationToken);
        }

        public IScheduledTask Schedule(Action action, TimeSpan delay)
        {
            VerifyAcceptingNewTasks();
            return Unwrap().Schedule(action, delay);
        }

        public IScheduledTask Schedule(Action<object> action, object state, TimeSpan delay)
        {
            VerifyAcceptingNewTasks();
            return Unwrap().Schedule(action, state, delay);
        }

        public IScheduledTask Schedule(Action<object, object> action, object context, object state, TimeSpan delay)
        {
            VerifyAcceptingNewTasks();
            return Unwrap().Schedule(action, context, state, delay);
        }

        public Task ScheduleAsync(Action<object> action, object state, TimeSpan delay,
            CancellationToken cancellationToken)
        {
            VerifyAcceptingNewTasks();
            return Unwrap().ScheduleAsync(action, state, delay, cancellationToken);
        }

        public Task ScheduleAsync(Action<object> action, object state, TimeSpan delay)
        {
            VerifyAcceptingNewTasks();
            return Unwrap().ScheduleAsync(action, state, delay);
        }

        public Task ScheduleAsync(Action action, TimeSpan delay, CancellationToken cancellationToken)
        {
            VerifyAcceptingNewTasks();
            return Unwrap().ScheduleAsync(action, delay, cancellationToken);
        }

        public Task ScheduleAsync(Action action, TimeSpan delay)
        {
            VerifyAcceptingNewTasks();
            return Unwrap().ScheduleAsync(action, delay);
        }

        public Task ScheduleAsync(Action<object, object> action, object context, object state, TimeSpan delay)
        {
            VerifyAcceptingNewTasks();
            return Unwrap().ScheduleAsync(action, context, state, delay);
        }

        public Task ScheduleAsync(Action<object, object> action, object context, object state, TimeSpan delay,
            CancellationToken cancellationToken)
        {
            VerifyAcceptingNewTasks();
            return Unwrap().ScheduleAsync(action, context, state, delay, cancellationToken);
        }

        public Task GracefulShutdownAsync()
        {
            return Unwrap().GracefulShutdownAsync();
        }

        public Task GracefulShutdownAsync(TimeSpan quietPeriod, TimeSpan timeout)
        {
            return Unwrap().GracefulShutdownAsync(quietPeriod, timeout);
        }

        public abstract IEventExecutor Unwrap();
        public abstract void RejectNewTasks();

        public abstract void AcceptNewTasks();

        public abstract bool IsAcceptingNewTasks { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void VerifyAcceptingNewTasks()
        {
            if (!IsAcceptingNewTasks)
            {
                throw RejectedTaskException.Instance;
            }
        }
    }
}