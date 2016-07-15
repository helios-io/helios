// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;

namespace Helios.Ops
{
    /// <summary>
    ///     Interface used for creating chained eventloops
    ///     for processing network streams / events
    /// </summary>
    public interface IEventLoop : IExecutor, IDisposable
    {
        /// <summary>
        ///     Was this event loop disposed?
        /// </summary>
        bool WasDisposed { get; }

        /// <summary>
        ///     Return the next executor in the chain
        /// </summary>
        IExecutor Next();
    }
}