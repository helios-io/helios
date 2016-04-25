using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helios.Buffers;
using Helios.Channels.Embedded;
using Helios.Codecs;
using Xunit;

namespace Helios.Tests.Codecs
{
    public class LengthFieldPrependerSpecs
    {
        IByteBuf msg;

        public LengthFieldPrependerSpecs()
        {
            Encoding iso = Encoding.GetEncoding("ISO-8859-1");
            byte[] charBytes = iso.GetBytes("A");
            msg = Unpooled.WrappedBuffer(charBytes);
        }

        [Fact]
        public void TestPrependLength()
        {
            EmbeddedChannel ch = new EmbeddedChannel(new LengthFieldPrepender(4));
            ch.WriteOutbound(msg);
            IByteBuf buf = ch.ReadOutbound<IByteBuf>();
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
            EmbeddedChannel ch = new EmbeddedChannel(new LengthFieldPrepender(4, true));
            ch.WriteOutbound(msg);
            IByteBuf buf = ch.ReadOutbound<IByteBuf>();
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
            EmbeddedChannel ch = new EmbeddedChannel(new LengthFieldPrepender(4, -1));
            ch.WriteOutbound(msg);
            IByteBuf buf = ch.ReadOutbound<IByteBuf>();
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
            EmbeddedChannel ch = new EmbeddedChannel(new LengthFieldPrepender(4, -2));
            AggregateException ex = Assert.Throws<AggregateException>(() =>
            {
                ch.WriteOutbound(msg);
                Assert.True(false, typeof(EncoderException).Name + " must be raised.");
            });

            Assert.IsType<EncoderException>(ex.InnerExceptions.Single());
        }

        [Fact]
        public void TestPrependLengthInLittleEndian()
        {
            EmbeddedChannel ch = new EmbeddedChannel(new LengthFieldPrepender(ByteOrder.LittleEndian, 4, 0, false));
            ch.WriteOutbound(msg);
            IByteBuf buf = ch.ReadOutbound<IByteBuf>();
            Assert.Equal(4, buf.ReadableBytes);
            byte[] writtenBytes = new byte[buf.ReadableBytes];
            buf.GetBytes(0, writtenBytes);
            Assert.Equal(1, writtenBytes[0]);
            Assert.Equal(0, writtenBytes[1]);
            Assert.Equal(0, writtenBytes[2]);
            Assert.Equal(0, writtenBytes[3]);
            //buf.Release();

            buf = ch.ReadOutbound<IByteBuf>();
            Assert.Same(buf, msg);
            //buf.Release();
            Assert.False(ch.Finish(), "The channel must have been completely read");
        }
    }
}
