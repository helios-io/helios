// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using Helios.Buffers;

namespace Helios.Channels
{
    public interface IRecvByteBufferAllocatorHandle
    {
        /// <summary>
        ///     Allocates buffers for socket read and receive operations, with enough capacity to read all received
        ///     data in one shot.
        /// </summary>
        IByteBuf Allocate(IByteBufAllocator allocator);

        /// <summary>
        ///     Guesses the capacity of the next new buffer, but doesn't allocate it.
        /// </summary>
        int Guess();

        /// <summary>
        ///     Records the actual number of read bytes in the previous read, so it can provide some intelligence
        ///     as to how big the next buffer should be.
        /// </summary>
        /// <param name="actualReadBytes">The number of bytes actually read.</param>
        void Record(int actualReadBytes);
    }
}