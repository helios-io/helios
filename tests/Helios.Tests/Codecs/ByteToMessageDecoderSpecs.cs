using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helios.Buffers;
using Helios.Channels;
using Helios.Channels.Embedded;
using Helios.Codecs;
using Xunit;

namespace Helios.Tests.Codecs
{
    public class ByteToMessageDecoderSpecs
    {
        class RemovedDecoder : ByteToMessageDecoder
        {
            private bool _removed;

            protected override void Decode(IChannelHandlerContext context, IByteBuf input, IList<object> output)
            {
                Assert.False(_removed);
                input.ReadByte();
                context.Pipeline.Remove(this);
                _removed = true;
            }
        }

        [Fact]
        public void Should_remove_itself()
        {
            var ec = new EmbeddedChannel(new RemovedDecoder());
            var buf = Unpooled.WrappedBuffer(new char[] {'a', 'b', 'c'}.Select(Convert.ToByte).ToArray());
            ec.WriteInbound(buf.Copy());
            IByteBuf b = ec.ReadInbound<IByteBuf>();
            Assert.Equal(b, buf.SkipBytes(1), AbstractByteBuf.ByteBufComparer);
        }
    }
}
