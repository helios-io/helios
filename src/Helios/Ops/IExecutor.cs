// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Helios.Ops
{
    /// <summary>
    ///     Interface used for executing commands and actions - represents
    ///     the lowest possible unit of work
    /// </summary>
    public interface IExecutor
    {
        bool AcceptingJobs { get; }

        void Execute(Action op);

        Task ExecuteAsync(Action op);

        void Execute(IList<Action> op);

        void Execute(Task task);

        Task ExecuteAsync(IList<Action> op);

        /// <summary>
        ///     Process a queue of tasks - if the IExecutor is shut down before
        ///     it has a chance to complete its queue, all of the remaining jobs
        ///     will be passed to an optional callback <see cref="remainingOps" />
        /// </summary>
        /// <param name="ops">The queue of actions to execute</param>
        /// <param name="remainingOps">
        ///     OPTIONAL. Can be null. Callback function for placing any jobs that couldn't be run
        ///     due to an exception or shutdown.
        /// </param>
        void Execute(IList<Action> ops, Action<IEnumerable<Action>> remainingOps);

        Task ExecuteAsync(IList<Action> ops, Action<IEnumerable<Action>> remainingOps);

        /// <summary>
        ///     Immediate shutdown
        /// </summary>
        void Shutdown();

        /// <summary>
        ///     Shut down tasks within the allotted time
        /// </summary>
        /// <param name="gracePeriod">The amount of time left to process tasks before forcibly killing the executor</param>
        void Shutdown(TimeSpan gracePeriod);

        /// <summary>
        ///     Gracefully shuts down the executor - attempts to drain all of the remaining actions before time expires
        /// </summary>
        /// <param name="gracePeriod">The amount of time <see cref="IExecutor" /> is given to execute any remaining tasks</param>
        /// <returns>
        ///     A task with a status result of "success" as long as no unhandled exceptions are thrown by the time the
        ///     executor is terminated
        /// </returns>
        Task GracefulShutdown(TimeSpan gracePeriod);

        /// <summary>
        ///     Checks to see if this <see cref="IExecutor" /> is executing inside the given thread
        /// </summary>
        bool InThread(Thread thread);

        /// <summary>
        ///     Creates a deep clone of this <see cref="IExecutor" /> instance
        /// </summary>
        IExecutor Clone();
    }
}