using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Channels
{
    public interface IChannelHandler
    {
        void ChannelRegistered(IChannelHandlerContext context);

        void ChannelUnregistered(IChannelHandlerContext context);

        void ChannelActive(IChannelHandlerContext context);

        void ChannelInactive(IChannelHandlerContext context);

        void ChannelRead(IChannelHandlerContext context, object message);

        void ChannelReadComplete(IChannelHandlerContext context);

        void ChannelWritabilityChanged(IChannelHandlerContext context);

        void HandlerAdded(IChannelHandlerContext context);

        void HandlerRemoved(IChannelHandlerContext context);

        Task WriteAsync(IChannelHandlerContext context, object message);
        void Flush(IChannelHandlerContext context);

        Task BindAsync(IChannelHandlerContext context, EndPoint localAddress);

        Task ConnectAsync(IChannelHandlerContext context, EndPoint remoteAddress, EndPoint localAddress);

        Task DisconnectAsync(IChannelHandlerContext context);
        Task CloseAsync(IChannelHandlerContext context);

        void ExceptionCaught(IChannelHandlerContext context, Exception exception);

        Task DeregisterAsync(IChannelHandlerContext context);

        void Read(IChannelHandlerContext context);

        void UserEventTriggered(IChannelHandlerContext context, object evt);
    }
}
