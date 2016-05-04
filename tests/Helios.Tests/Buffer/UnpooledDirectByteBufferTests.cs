// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using Helios.Buffers;

namespace Helios.Tests.Buffer
{
    public class UnpooledDirectByteBufferTests : ByteBufferTests
    {
        protected override IByteBuf GetBuffer(int initialCapacity)
        {
            return UnpooledByteBufAllocator.Default.Buffer(initialCapacity);
        }

        protected override IByteBuf GetBuffer(int initialCapacity, int maxCapacity)
        {
            return UnpooledByteBufAllocator.Default.Buffer(initialCapacity, maxCapacity);
        }
    }
}

