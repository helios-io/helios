// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Threading.Tasks;
using Helios.Ops;

namespace Helios.Concurrency
{
    /// <summary>
    /// Interface for lightweight threading and execution
    /// </summary>
    [Obsolete()]
    public interface IFiber : IDisposable
    {
        /// <summary>
        /// The internal executor used to execute tasks
        /// </summary>
        IExecutor Executor { get; }

        /// <summary>
        /// Is this Fiber still running?
        /// </summary>
        bool Running { get; }

        bool WasDisposed { get; }

        void Add(Action op);

        /// <summary>
        /// Replaces the current <see cref="Executor"/> with a new <see cref="IEventExecutor"/> instance
        /// </summary>
        /// <param name="executor">The new executor</param>
        void SwapExecutor(IExecutor executor);

        /// <summary>
        /// Shuts down this Fiber within the allotted timeframe
        /// </summary>
        /// <param name="gracePeriod">The amount of time given for currently executing tasks to complete</param>
        void Shutdown(TimeSpan gracePeriod);

        /// <summary>
        /// Shuts down this fiber within the allotted timeframe and provides a task that can be waited on during the interim
        /// </summary>
        /// <param name="gracePeriod">The amount of time given for currently executing tasks to complete</param>
        Task GracefulShutdown(TimeSpan gracePeriod);

        /// <summary>
        /// Performs a hard-stop on the Fiber - no more actions can be executed
        /// </summary>
        void Stop();

        void Dispose(bool isDisposing);

        /// <summary>
        /// Creates a deep clone of this <see cref="IFiber"/> instance
        /// </summary>
        IFiber Clone();
    }
}