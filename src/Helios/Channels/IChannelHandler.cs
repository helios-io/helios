using System;
using System.Net;
using System.Threading.Tasks;

namespace Helios.Channels
{
    /// <summary>
    /// Defines the range of all possible operations that can be handled at each stage in the <see cref="IChannelPipeline"/>
    /// </summary>
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