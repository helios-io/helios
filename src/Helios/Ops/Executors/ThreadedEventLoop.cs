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

        public ThreadedEventLoop(IFiber scheduler) : base(scheduler) { }

        public override IExecutor Next()
        {
            return new ThreadedEventLoop(Scheduler);
        }
    }
}