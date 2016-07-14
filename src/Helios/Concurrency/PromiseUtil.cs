// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using Helios.Logging;

namespace Helios.Concurrency
{
    /// <summary>
    ///     Utility class for working with <see cref="TaskCompletionSource" />
    /// </summary>
    internal static class PromiseUtil
    {
        public static void SafeSetSuccess(TaskCompletionSource promise, ILogger logger)
        {
            if (promise != TaskCompletionSource.Void && !promise.TryComplete())
            {
                logger.Warning("Failed to complete task successfully because it is done already: {0}", promise);
            }
        }

        public static void SafeSetFailure(TaskCompletionSource promise, Exception cause, ILogger logger)
        {
            if (promise != TaskCompletionSource.Void && !promise.TrySetException(cause))
            {
                logger.Warning(
                    "Failed to set exception on task successfully because it is done already: {0}; Cause: {1}", promise,
                    cause);
            }
        }
    }
}