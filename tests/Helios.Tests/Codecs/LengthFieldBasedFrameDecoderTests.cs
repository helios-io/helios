// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Text;
using Helios.Buffers;
using Helios.Channels.Embedded;
using Helios.Codecs;
using Xunit;

namespace Helios.Tests.Codecs
{
    public class LengthFieldBasedFrameDecoderTests
    {
        [Fact]
        public void FailSlowTooLongFrameRecovery()
        {
            var ch = new EmbeddedChannel(new LengthFieldBasedFrameDecoder(5, 0, 4, 0, 4, false));
            for (var i = 0; i < 2; i++)
            {
                Assert.False(ch.WriteInbound(Unpooled.WrappedBuffer(new byte[] {2, 0, 0, 0})));
                Assert.Throws<TooLongFrameException>(() =>
                {
                    Assert.True(ch.WriteInbound(Unpooled.WrappedBuffer(new byte[] {0, 0})));
                    Assert.True(false, typeof (DecoderException).Name + " must be raised.");
                });
                ch.WriteInbound(Unpooled.WrappedBuffer(new byte[] {1, 0, 0, 0, (byte) 'A'}));
                var buf = ch.ReadInbound<IByteBuf>();
                var iso = Encoding.GetEncoding("ISO-8859-1");
                Assert.Equal("A", iso.GetString(buf.ToArray()));
                buf.Release();
            }
        }

        [Fact]
        public void TestFailFastTooLongFrameRecovery()
        {
            var buf = Unpooled.Buffer();
            buf.WriteInt(32);
            for (var i = 0; i < 32; i++)
                buf.WriteByte(i);
            buf.WriteInt(1);
            buf.WriteByte('a');
            var ch = new EmbeddedChannel(
                new LengthFieldBasedFrameDecoder(16,0,4));

            Assert.Throws<TooLongFrameException>(() =>
            {
                ch.WriteInbound(buf.ReadSlice(14).Retain());
            });
            Assert.True(ch.WriteInbound(buf.ReadSlice(buf.ReadableBytes).Retain()));
            Assert.True(ch.Finish());

            var b = ch.ReadInbound<IByteBuf>();
            Assert.Equal(5, b.ReadableBytes);
            Assert.Equal(1, b.ReadInt());
            Assert.Equal('a', (char)b.ReadByte());

            Assert.Null(ch.ReadInbound<IByteBuf>());
            ch.Finish();
        }
    }
}

