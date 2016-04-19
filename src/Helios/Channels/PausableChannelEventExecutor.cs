using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Helios.Concurrency;

namespace Helios.Channels
{
    abstract class PausableChannelEventExecutor : IPausableEventExecutor
    {
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

        public Task<T> SubmitAsync<T>(Func<object, object, T> func, object context, object state, CancellationToken cancellationToken)
        {
            VerifyAcceptingNewTasks();
            return Unwrap().SubmitAsync(func, context, state, cancellationToken);
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

        internal abstract IChannel Channel { get; }

        public IEventExecutor Executor => this;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void VerifyAcceptingNewTasks()
        {
            if (!this.IsAcceptingNewTasks)
            {
                throw RejectedTaskException.Instance;
            }
        }
    }
}
