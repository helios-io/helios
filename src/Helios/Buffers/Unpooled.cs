// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

namespace Helios.Buffers
{
    /// <summary>
    ///     Utility class for managing and creating unpooled buffers
    /// </summary>
    public static class Unpooled
    {
        private static readonly IByteBufAllocator Alloc = UnpooledByteBufAllocator.Default;

        public static readonly IByteBuf Empty = Alloc.Buffer(0, 0);

        public static IByteBuf Buffer()
        {
            return Alloc.Buffer();
        }

        public static IByteBuf Buffer(int initialCapacity)
        {
            return Alloc.Buffer(initialCapacity);
        }

        public static IByteBuf Buffer(int initialCapacity, int maxCapacity)
        {
            return Alloc.Buffer(initialCapacity, maxCapacity);
        }

        public static IByteBuf WrappedBuffer(byte[] bytes)
        {
            return WrappedBuffer(bytes, 0, bytes.Length);
        }

        public static IByteBuf WrappedBuffer(byte[] bytes, int index, int length)
        {
            if (bytes.Length == 0)
                return Empty;
            return Alloc.Buffer(bytes.Length).WriteBytes(bytes, index, length);
        }
    }
}