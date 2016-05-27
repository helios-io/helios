// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Linq;
using System.Text;
using Helios.Buffers;
using Helios.Channels.Embedded;
using Helios.Codecs;
using Xunit;

namespace Helios.Tests.Codecs
{
    public class LengthFieldPrependerSpecs
    {
        private readonly IByteBuf msg;

        public LengthFieldPrependerSpecs()
        {
            var iso = Encoding.GetEncoding("ISO-8859-1");
            var charBytes = iso.GetBytes("A");
            msg = Unpooled.WrappedBuffer(charBytes);
        }

        [Fact]
        public void TestPrependLength()
        {
            var ch = new EmbeddedChannel(new LengthFieldPrepender(4));
            ch.WriteOutbound(msg);
            var buf = ch.ReadOutbound<IByteBuf>();
            Assert.Equal(4, buf.ReadableBytes);
            Assert.Equal(msg.ReadableBytes, buf.ReadInt());
            //buf.Release();

            buf = ch.ReadOutbound<IByteBuf>();
            Assert.Same(buf, msg);
            //buf.Release();
        }

        [Fact]
        public void TestPrependLengthIncludesLengthFieldLength()
        {
            var ch = new EmbeddedChannel(new LengthFieldPrepender(4, true));
            ch.WriteOutbound(msg);
            var buf = ch.ReadOutbound<IByteBuf>();
            Assert.Equal(4, buf.ReadableBytes);
            Assert.Equal(5, buf.ReadInt());
            //buf.Release();

            buf = ch.ReadOutbound<IByteBuf>();
            Assert.Same(buf, msg);
            //buf.Release();
        }

        [Fact]
        public void TestPrependAdjustedLength()
        {
            var ch = new EmbeddedChannel(new LengthFieldPrepender(4, -1));
            ch.WriteOutbound(msg);
            var buf = ch.ReadOutbound<IByteBuf>();
            Assert.Equal(4, buf.ReadableBytes);
            Assert.Equal(msg.ReadableBytes - 1, buf.ReadInt());
            //buf.Release();

            buf = ch.ReadOutbound<IByteBuf>();
            Assert.Same(buf, msg);
            //buf.Release();
        }

        [Fact]
        public void TestPrependAdjustedLengthLessThanZero()
        {
            var ch = new EmbeddedChannel(new LengthFieldPrepender(4, -2));
            var ex = Assert.Throws<AggregateException>(() =>
            {
                ch.WriteOutbound(msg);
                Assert.True(false, typeof (EncoderException).Name + " must be raised.");
            });

            Assert.IsType<EncoderException>(ex.InnerExceptions.Single());
        }

        [Fact]
        public void TestPrependLengthInBigEndian()
        {
            var ch = new EmbeddedChannel(new LengthFieldPrepender(ByteOrder.BigEndian, 4, 0, false));
            ch.WriteOutbound(msg);
            var buf = ch.ReadOutbound<IByteBuf>();
            Assert.Equal(4, buf.ReadableBytes);
            var writtenBytes = new byte[buf.ReadableBytes];
            buf.GetBytes(0, writtenBytes);
            Assert.Equal(0, writtenBytes[0]);
            Assert.Equal(0, writtenBytes[1]);
            Assert.Equal(0, writtenBytes[2]);
            Assert.Equal(1, writtenBytes[3]);
            //buf.Release();

            buf = ch.ReadOutbound<IByteBuf>();
            Assert.Same(buf, msg);
            //buf.Release();
            Assert.False(ch.Finish(), "The channel must have been completely read");
        }
    }
}

