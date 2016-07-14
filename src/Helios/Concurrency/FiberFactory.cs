// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using Helios.Concurrency.Impl;
using Helios.Ops;

namespace Helios.Concurrency
{
    /// <summary>
    ///     Factory class for creating Fiber instances
    /// </summary>
    public static class FiberFactory
    {
        /// <summary>
        ///     3 threads is the default for a constrained DedicatedThreadPoolFiber
        /// </summary>
        public const int DefaultLimitedThreadPoolSize = 3;

        public static IFiber CreateFiber(FiberMode mode = FiberMode.SingleThreaded)
        {
            switch (mode)
            {
                case FiberMode.MultiThreaded:
                    return new DedicatedThreadPoolFiber(DefaultLimitedThreadPoolSize);
                case FiberMode.SingleThreaded:
                    return new DedicatedThreadPoolFiber(1);
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
                    return new DedicatedThreadPoolFiber(executor, DefaultLimitedThreadPoolSize);
                case FiberMode.SingleThreaded:
                    return new DedicatedThreadPoolFiber(executor, 1);
                case FiberMode.MaximumConcurrency:
                    return new ThreadPoolFiber(executor);
                case FiberMode.Synchronous:
                default:
                    return new SynchronousFiber(executor);
            }
        }

        public static IFiber CreateFiber(IExecutor executor, int numThreads)
        {
            return new DedicatedThreadPoolFiber(executor, numThreads);
        }

        public static IFiber CreateFiber(int numThreads)
        {
            return new DedicatedThreadPoolFiber(numThreads);
        }
    }
}