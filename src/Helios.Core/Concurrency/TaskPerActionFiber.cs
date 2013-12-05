using System;
using System.Collections.Concurrent;
using Helios.Core.Ops;
using Helios.Core.Ops.Executors;

namespace Helios.Core.Concurrency
{
    public class TaskPerActionFiber : IFiber
    {
        protected readonly IExecutor Executor;
        protected readonly ConcurrentQueue<Action> JobQueue;

        public TaskPerActionFiber() : this(new TryCatchExecutor()) { }

        public TaskPerActionFiber(IExecutor executor)
        {
            Executor = executor;
            JobQueue = new ConcurrentQueue<Action>();
        }

        public void Add(Action op)
        {
            if(Executor.AcceptingJobs)
                JobQueue.Enqueue(op);
        }

        public void Shutdown(TimeSpan gracePeriod)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}