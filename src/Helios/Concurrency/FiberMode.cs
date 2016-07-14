// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

namespace Helios.Concurrency
{
    public enum FiberMode
    {
        Synchronous,
        MultiThreaded,
        SingleThreaded,
        MaximumConcurrency
    }
}