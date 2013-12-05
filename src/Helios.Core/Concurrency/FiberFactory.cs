using Helios.Core.Concurrency.Impl;
using Helios.Core.Ops;

namespace Helios.Core.Concurrency
{
    /// <summary>
    /// Factory class for creating Fiber instances
    /// </summary>
    public static class FiberFactory
    {
        /// <summary>
        /// 3 threads is the default for a constrained ThreadPoolFiber
        /// </summary>
        public const int DefaultLimitedThreadPoolSize = 3; 

        public static IFiber CreateFiber(FiberMode mode = FiberMode.SingleThreaded)
        {
            switch (mode)
            {
                case FiberMode.MultiThreaded:
                    return new ThreadPoolFiber(DefaultLimitedThreadPoolSize);
                case FiberMode.SingleThreaded:
                    return new ThreadPoolFiber(1);
                case FiberMode.MaximumConcurrency:
                    return new ThreadPoolFiber();
                case FiberMode.Synchronous:
                default:
                    return new SynchronousFiber();
            }
        }

        public static IFiber CreateFiber(IExecutor executor, FiberMode mode = FiberMode.SingleThreaded)
        {
            switch (mode)
            {
                case FiberMode.MultiThreaded:
                    return new ThreadPoolFiber(executor, DefaultLimitedThreadPoolSize);
                case FiberMode.SingleThreaded:
                    return new ThreadPoolFiber(executor, 1);
                case FiberMode.MaximumConcurrency:
                    return new ThreadPoolFiber(executor);
                case FiberMode.Synchronous:
                default:
                    return new SynchronousFiber(executor);
            }
        }

        public static IFiber CreateFiber(IExecutor executor, int numThreads)
        {
            return new ThreadPoolFiber(executor, numThreads);
        }

        public static IFiber CreateFiber(int numThreads)
        {
            return new ThreadPoolFiber(numThreads);
        }
    }
}
