using Helios.Concurrency;

namespace Helios.Ops.Executors
{
    /// <summary>
    /// Simple multi-threaded event loop
    /// </summary>
    public class ThreadedEventLoop : AbstractEventLoop
    {
        public ThreadedEventLoop(int workerThreads) : base(FiberFactory.CreateFiber(workerThreads))
        {
        }

        public ThreadedEventLoop(IExecutor internalExecutor, int workerThreads)
            : base(FiberFactory.CreateFiber(internalExecutor, workerThreads))
        {
        }

        public ThreadedEventLoop(IFiber scheduler) : base(scheduler) { }

        public override IExecutor Clone()
        {
            return new ThreadedEventLoop(Scheduler.Clone());
        }
    }
}