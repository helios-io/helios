using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Helios.Ops;
using Helios.Ops.Executors;
using Helios.Util.Collections;

namespace Helios.MultiNodeTests.TestKit
{
    /// <summary>
    /// <see cref="IExecutor"/> implementation that collects unhandled exceptions into an internal buffer, which is exposed via
    /// the <see cref="Exceptions"/> collection.
    /// </summary>
    public class AssertExecutor : IExecutor
    {
        public readonly ConcurrentCircularBuffer<Exception> Exceptions = new ConcurrentCircularBuffer<Exception>(100);

        private readonly IExecutor _internalExecutor;

        public AssertExecutor()
        {
            _internalExecutor = new TryCatchExecutor(exception => Exceptions.Add(exception));
        }

        public bool AcceptingJobs { get { return _internalExecutor.AcceptingJobs; } }
        public void Execute(Action op)
        {
            _internalExecutor.Execute(op);
        }

        public Task ExecuteAsync(Action op)
        {
            return _internalExecutor.ExecuteAsync(op);
        }

        public void Execute(IList<Action> op)
        {
            _internalExecutor.Execute(op);
        }

        public void Execute(Task task)
        {
            _internalExecutor.Execute(task);
        }

        public Task ExecuteAsync(IList<Action> op)
        {
            return _internalExecutor.ExecuteAsync(op);
        }

        public void Execute(IList<Action> ops, Action<IEnumerable<Action>> remainingOps)
        {
            _internalExecutor.Execute(ops, remainingOps);
        }

        public Task ExecuteAsync(IList<Action> ops, Action<IEnumerable<Action>> remainingOps)
        {
            return _internalExecutor.ExecuteAsync(ops, remainingOps);
        }

        public void Shutdown()
        {
            _internalExecutor.Shutdown();
        }

        public void Shutdown(TimeSpan gracePeriod)
        {
            _internalExecutor.Shutdown(gracePeriod);
        }

        public Task GracefulShutdown(TimeSpan gracePeriod)
        {
            return _internalExecutor.GracefulShutdown(gracePeriod);
        }

        public bool InThread(Thread thread)
        {
            return _internalExecutor.InThread(thread);
        }

        public IExecutor Clone()
        {
            //force the entire test application to report its exceptions to one place
            return this;
        }
    }
}
