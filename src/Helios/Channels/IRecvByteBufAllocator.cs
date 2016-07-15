// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

namespace Helios.Channels
{
    /// <summary>
    ///     Allocates buffers for socket read and receive operations, with enough capacity to read all received
    ///     data in one shot.
    /// </summary>
    public interface IRecvByteBufAllocator
    {
        /// <summary>
        ///     Creates a new handle for estimating incoming receive buffer sizes and creating buffers
        ///     based on those estimates.
        /// </summary>
        IRecvByteBufferAllocatorHandle NewHandle();
    }
}