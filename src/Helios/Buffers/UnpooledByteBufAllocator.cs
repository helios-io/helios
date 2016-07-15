// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

namespace Helios.Buffers
{
    /// <summary>
    ///     Unpooled implementation of <see cref="IByteBufAllocator" />.
    /// </summary>
    public class UnpooledByteBufAllocator : AbstractByteBufAllocator
    {
        /// <summary>
        ///     Default instance
        /// </summary>
        public static readonly UnpooledByteBufAllocator Default = new UnpooledByteBufAllocator();

        protected override IByteBuf NewDirectBuffer(int initialCapacity, int maxCapacity)
        {
            return new UnpooledDirectByteBuf(this, initialCapacity, maxCapacity);
        }
    }
}