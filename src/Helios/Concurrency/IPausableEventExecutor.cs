// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

namespace Helios.Concurrency
{
    /// <summary>
    ///     An <see cref="IEventExecutor" /> that can reject new work while paused.
    /// </summary>
    public interface IPausableEventExecutor : IWrappedEventExecutor
    {
        /// <summary>
        ///     Returns true if paused, false otherwise.
        /// </summary>
        bool IsAcceptingNewTasks { get; }

        /// <summary>
        ///     Pause - may throw a <see cref="RejectedTaskException" /> if work is queued while paused.
        /// </summary>
        void RejectNewTasks();

        /// <summary>
        ///     Unpause.
        /// </summary>
        void AcceptNewTasks();
    }
}