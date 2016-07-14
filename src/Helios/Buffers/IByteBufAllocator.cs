// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

namespace Helios.Buffers
{
    /// <summary>
    ///     Thread-safe interface for allocating <see cref="IByteBuf" /> instances for use inside Helios reactive I/O
    /// </summary>
    public interface IByteBufAllocator
    {
        IByteBuf Buffer();

        IByteBuf Buffer(int initialCapcity);

        IByteBuf Buffer(int initialCapacity, int maxCapacity);
    }
}