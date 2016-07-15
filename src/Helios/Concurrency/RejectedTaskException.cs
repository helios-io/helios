// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;

namespace Helios.Concurrency
{
    /// <summary>
    ///     An exception thrown when a <see cref="IPausableEventExecutor" /> is asked to a queue a task
    ///     when it is no longer accepting work.
    /// </summary>
    public class RejectedTaskException : Exception
    {
        public static readonly RejectedTaskException Instance = new RejectedTaskException();

        public RejectedTaskException() : base("Not accepting new work at this time!")
        {
        }
    }
}