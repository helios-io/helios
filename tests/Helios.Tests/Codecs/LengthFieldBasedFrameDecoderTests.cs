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
            EmbeddedChannel ch = new EmbeddedChannel(new LengthFieldBasedFrameDecoder(5, 0, 4, 0, 4, false));
            for (int i = 0; i < 2; i++)
            {
                Assert.False(ch.WriteInbound(Unpooled.WrappedBuffer(new byte[] { 0, 0, 0, 2 })));
                Assert.Throws<TooLongFrameException>(() =>
                {
                    Assert.True(ch.WriteInbound(Unpooled.WrappedBuffer(new byte[] { 0, 0 })));
                    Assert.True(false, typeof(DecoderException).Name + " must be raised.");
                });
                ch.WriteInbound(Unpooled.WrappedBuffer(new byte[] { 0, 0, 0, 1, (byte)'A' }));
                IByteBuf buf = ch.ReadInbound<IByteBuf>();
                Encoding iso = Encoding.GetEncoding("ISO-8859-1");
                Assert.Equal("A", iso.GetString(buf.ToArray()));
                buf.Release();
            }
        }

        [Fact]
        public void TestFailFastTooLongFrameRecovery()
        {
            EmbeddedChannel ch = new EmbeddedChannel(
                new LengthFieldBasedFrameDecoder(5, 0, 4, 0, 4));

            for (int i = 0; i < 2; i++)
            {
                Assert.Throws<TooLongFrameException>(() =>
                {
                    Assert.True(ch.WriteInbound(Unpooled.WrappedBuffer(new byte[] { 0, 0, 0, 2 })));
                    Assert.True(false, typeof(DecoderException).Name + " must be raised.");
                });

                ch.WriteInbound(Unpooled.WrappedBuffer(new byte[] { 0, 0, 0, 0, 0, 1, (byte)'A' }));
                IByteBuf buf = ch.ReadInbound<IByteBuf>();
                Encoding iso = Encoding.GetEncoding("ISO-8859-1");
                Assert.Equal("A", iso.GetString(buf.ToArray()));
                //buf.Release();
            }
        }
    }
}
