// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Generic;
using System.Linq;
using Helios.Buffers;
using Helios.Channels;
using Helios.Channels.Embedded;
using Helios.Codecs;
using Helios.Util;
using Xunit;

namespace Helios.Tests.Codecs
{
    public class ByteToMessageDecoderSpecs
    {
        [Fact]
        public void Should_remove_itself()
        {
            var ec = new EmbeddedChannel(new RemovedDecoder1());
            var buf = Unpooled.WrappedBuffer(new[] {'a', 'b', 'c'}.Select(Convert.ToByte).ToArray());
            ec.WriteInbound(buf.Copy());
            var b = ec.ReadInbound<IByteBuf>();
            Assert.Equal(b, buf.SkipBytes(1), AbstractByteBuf.ByteBufComparer);
            buf.Release();
            b.Release();
        }

        [Fact]
        public void Should_remove_itself_WriteBuffer()
        {
            var buf = Unpooled.Buffer().WriteBytes(new[] {'a', 'b', 'c'}.Select(Convert.ToByte).ToArray());
            var ec = new EmbeddedChannel(new RemovedDecoder2(buf));
            ec.WriteInbound(buf.Copy());
            var expected = Unpooled.WrappedBuffer(new[] {'b', 'c'}.Select(Convert.ToByte).ToArray());
            var b = ec.ReadInbound<IByteBuf>();
            Assert.Equal(expected, b, AbstractByteBuf.ByteBufComparer);
            buf.Release();
            b.Release();
        }

        [Fact]
        public void Should_remove_while_in_Decode()
        {
            var decoder = new RemovedDecoder3();
            var ec = new EmbeddedChannel(decoder, new InboundAdapter(decoder));

            var buf = Unpooled.WrappedBuffer(new[] {'a', 'b', 'c'}.Select(Convert.ToByte).ToArray());
            Assert.True(ec.WriteInbound(buf.Copy()));
            var b = ec.ReadInbound<IByteBuf>();
            Assert.Equal(b, buf.SkipBytes(1), AbstractByteBuf.ByteBufComparer);
            Assert.False(ec.Finish());
            buf.Release();
            b.Release();
        }

        [Fact]
        public void Should_DecodeLast_with_EmptyBuffer()
        {
            var ec = new EmbeddedChannel(new LastEmptyBufferDecoder());
            var bytes = new byte[1024];
            ThreadLocalRandom.Current.NextBytes(bytes);

            Assert.True(ec.WriteInbound(Unpooled.WrappedBuffer(bytes)));
            Assert.Equal(Unpooled.WrappedBuffer(bytes), ec.ReadInbound<IByteBuf>(), AbstractByteBuf.ByteBufComparer);
            Assert.Null(ec.ReadInbound<IByteBuf>());
            Assert.False(ec.Finish());
            Assert.Null(ec.ReadInbound<IByteBuf>());
        }

        [Fact]
        public void Should_DecodeLast_with_NonEmptyBuffer()
        {
            var ec = new EmbeddedChannel(new LastNonEmptyBufferDecoder());
            var bytes = new byte[1024];
            ThreadLocalRandom.Current.NextBytes(bytes);

            Assert.True(ec.WriteInbound(Unpooled.WrappedBuffer(bytes)));
            Assert.Equal(Unpooled.WrappedBuffer(bytes, 0, bytes.Length - 1), ec.ReadInbound<IByteBuf>(),
                AbstractByteBuf.ByteBufComparer);
            Assert.Null(ec.ReadInbound<IByteBuf>());
            Assert.True(ec.Finish());
            Assert.Equal(Unpooled.WrappedBuffer(bytes, bytes.Length - 1, 1), ec.ReadInbound<IByteBuf>(),
                AbstractByteBuf.ByteBufComparer);
            Assert.Null(ec.ReadInbound<IByteBuf>());
        }

        private class RemovedDecoder1 : ByteToMessageDecoder
        {
            private bool _removed;

            protected override void Decode(IChannelHandlerContext context, IByteBuf input, List<object> output)
            {
                Assert.False(_removed);
                input.ReadByte();
                context.Pipeline.Remove(this);
                _removed = true;
            }
        }

        private class RemovedDecoder2 : ByteToMessageDecoder
        {
            private readonly IByteBuf _buf;
            private bool _removed;

            public RemovedDecoder2(IByteBuf buf)
            {
                _buf = buf;
            }

            protected override void Decode(IChannelHandlerContext context, IByteBuf input, List<object> output)
            {
                Assert.False(_removed);
                input.ReadByte();
                context.Pipeline.Remove(this);

                // This should not let it keep calling Decode()
                _buf.WriteByte(Convert.ToByte('d'));

                _removed = true;
            }
        }

        private class RemovedDecoder3 : ByteToMessageDecoder
        {
            public readonly object UpgradeMessage = new object();

            protected override void Decode(IChannelHandlerContext context, IByteBuf input, List<object> output)
            {
                Assert.Equal(Convert.ToByte('a'), input.ReadByte());
                output.Add(UpgradeMessage);
            }
        }

        private class InboundAdapter : ChannelHandlerAdapter
        {
            private readonly RemovedDecoder3 _decoder;
            private readonly object _upgradeMessage;

            public InboundAdapter(RemovedDecoder3 decoder)
            {
                _decoder = decoder;
                _upgradeMessage = decoder.UpgradeMessage;
            }

            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                if (message == _upgradeMessage)
                {
                    context.Pipeline.Remove(_decoder);
                    return;
                }
                context.FireChannelRead(message);
            }
        }

        private class LastEmptyBufferDecoder : ByteToMessageDecoder
        {
            protected override void Decode(IChannelHandlerContext context, IByteBuf input, List<object> output)
            {
                var readable = input.ReadableBytes;
                Assert.True(readable > 0);
                output.Add(input.ReadBytes(readable));
            }
        }

        private class LastNonEmptyBufferDecoder : ByteToMessageDecoder
        {
            private bool _decodeLast;

            protected override void Decode(IChannelHandlerContext context, IByteBuf input, List<object> output)
            {
                var readable = input.ReadableBytes;
                Assert.True(readable > 0);
                if (!_decodeLast && readable == 1)
                    return;
                output.Add(input.ReadBytes(_decodeLast ? readable : readable - 1)); // need to leave a byte leftover
            }

            protected override void DecodeLast(IChannelHandlerContext context, IByteBuf input, List<object> output)
            {
                Assert.False(_decodeLast);
                _decodeLast = true;
                base.DecodeLast(context, input, output);
            }
        }
    }
}

