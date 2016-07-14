// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using Helios.Buffers;
using Xunit;

namespace Helios.Tests.Buffer
{
    public class ByteBufferDerivationSpecs
    {
        [Fact]
        public void Swap_in_reverse_should_be_original()
        {
            var buf = Unpooled.Buffer(8).SetIndex(1, 7);
            var swapped = buf.WithOrder(ByteOrder.BigEndian);

            Assert.IsType<SwappedByteBuffer>(swapped);
            Assert.Null(swapped.Unwrap());
            Assert.Same(buf, swapped.WithOrder(ByteOrder.LittleEndian));
            Assert.Same(swapped, swapped.WithOrder(ByteOrder.BigEndian));
            buf.SetIndex(2, 6);
            Assert.Equal(swapped.ReaderIndex, 2);
            Assert.Equal(swapped.WriterIndex, 6);
        }
    }
}