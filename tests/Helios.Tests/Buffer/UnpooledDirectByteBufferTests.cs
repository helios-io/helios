// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Linq;
using Helios.Buffers;
using Xunit;

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

        [Fact]
        public void UnpooledDirectByteBuffer_write_int_little_endian()
        {
            Assert.True(BitConverter.IsLittleEndian, "this spec is designed for little endian hardware");
            var testInt = 1;
            var littleEndianBytes = BitConverter.GetBytes(testInt);
            var buf = GetBuffer(4).WriteInt(testInt);
            Assert.True(littleEndianBytes.SequenceEqual(buf.Array));
        }

        [Fact]
        public void UnpooledDirectByteBuffer_write_long_little_endian()
        {
            Assert.True(BitConverter.IsLittleEndian, "this spec is designed for little endian hardware");
            var testLong = -4L;
            var littleEndianBytes = BitConverter.GetBytes(testLong);
            var buf = GetBuffer(8).WriteLong(testLong);
            Assert.True(littleEndianBytes.SequenceEqual(buf.Array));
        }

        [Fact]
        public void UnpooledDirectByteBuffer_write_short_little_endian()
        {
            Assert.True(BitConverter.IsLittleEndian, "this spec is designed for little endian hardware");
            short testShort = -4;
            var littleEndianBytes = BitConverter.GetBytes(testShort);
            var buf = GetBuffer(2).WriteShort(testShort);
            Assert.True(littleEndianBytes.SequenceEqual(buf.Array));
        }
    }
}

