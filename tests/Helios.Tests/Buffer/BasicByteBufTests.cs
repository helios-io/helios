// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Text;
using Helios.Buffers;
using Xunit;

namespace Helios.Tests.Buffer
{
    public class BasicByteBufTests
    {
        [Fact]
        public void Should_pretty_print_buffer()
        {
            var buf = Unpooled.Buffer(10).WriteBoolean(true).WriteInt(4);
            var str = buf.ToString(Encoding.ASCII);
            Assert.NotNull(str);
        }
    }
}