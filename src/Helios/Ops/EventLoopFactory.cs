// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using Helios.Net;
using Helios.Ops.Executors;

namespace Helios.Ops
{
    /// <summary>
    ///     Static factory class for creating <see cref="IEventLoop" /> instances
    /// </summary>
    public static class EventLoopFactory
    {
        public static IEventLoop CreateThreadedEventLoop(int defaultSize = 2, IExecutor internalExecutor = null)
        {
            return new ThreadedEventLoop(internalExecutor, defaultSize);
        }

        public static NetworkEventLoop CreateNetworkEventLoop(int defaultSize = 2, IExecutor internalExecutor = null)
        {
            return new NetworkEventLoop(internalExecutor, defaultSize);
        }
    }
}