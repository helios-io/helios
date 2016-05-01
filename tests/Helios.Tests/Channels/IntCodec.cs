using System.Threading.Tasks;
using Helios.Buffers;
using Helios.Channels;

namespace Helios.Tests.Channels
{
    public class IntCodec : ChannelHandlerAdapter
    {
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (message is IByteBuf)
            {
                var buf = (IByteBuf) message;
                var integer = buf.ReadInt();
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