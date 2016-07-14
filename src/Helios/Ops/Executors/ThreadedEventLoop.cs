// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using Helios.Concurrency;

namespace Helios.Ops.Executors
{
    /// <summary>
    ///     Simple multi-threaded event loop
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

        public ThreadedEventLoop(IFiber scheduler) : base(scheduler)
        {
        }

        public override IExecutor Clone()
        {
            return new ThreadedEventLoop(Scheduler.Clone());
        }

        public override IExecutor Next()
        {
            return new ThreadedEventLoop(Scheduler);
        }
    }
}