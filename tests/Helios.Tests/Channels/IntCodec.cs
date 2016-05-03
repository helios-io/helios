using System.Threading.Tasks;
using Helios.Buffers;
using Helios.Channels;
using Helios.Util;

namespace Helios.Tests.Channels
{
    public class IntCodec : ChannelHandlerAdapter
    {
        public IntCodec(bool releaseMessages = false)
        {
            ReleaseMessages = releaseMessages;
        }

        public bool ReleaseMessages { get; }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (message is IByteBuf)
            {
                var buf = (IByteBuf) message;
                var integer = buf.ReadInt();
                if(ReleaseMessages)
                    ReferenceCountUtil.SafeRelease(message);
                context.FireChannelRead(integer);
            }
            else
            {
                context.FireChannelRead(message);
            }
        }

        public override Task WriteAsync(IChannelHandlerContext context, object message)
        {
            if (message is int)
            {
                var buf = Unpooled.Buffer(4).WriteInt((int) message);
                return context.WriteAsync(buf);
            }
            else
            {
                return context.WriteAsync(message);
            }
        }
    }
}