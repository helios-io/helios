using System;
using System.Net;
using System.Threading.Tasks;

namespace Helios.Channels
{
    /// <summary>
    /// Default <see cref="IChannelHandler"/> implementation, designed to have its virtual methods overridden by user-defined
    /// handlers.
    /// </summary>
    public class ChannelHandlerAdapter : IChannelHandler
    {
        public virtual void ChannelRegistered(IChannelHandlerContext context)
        {
            context.FireChannelRegistered();
        }

        public virtual void ChannelUnregistered(IChannelHandlerContext context)
        {
            context.FireChannelUnregistered();
        }

        public virtual void ChannelActive(IChannelHandlerContext context)
        {
            context.FireChannelActive();
        }

        public virtual void ChannelInactive(IChannelHandlerContext context)
        {
            context.FireChannelInactive();
        }

        public virtual void ChannelRead(IChannelHandlerContext context, object message)
        {
            context.FireChannelRead(message);
        }

        public virtual void ChannelReadComplete(IChannelHandlerContext context)
        {
            context.FireChannelReadComplete();
        }

        public virtual void ChannelWritabilityChanged(IChannelHandlerContext context)
        {
            context.FireChannelWritabilityChanged();
        }

        public virtual void HandlerAdded(IChannelHandlerContext context)
        {
        }

        public virtual void HandlerRemoved(IChannelHandlerContext context)
        {
        }

        public virtual void UserEventTriggered(IChannelHandlerContext context, object evt)
        {
            context.FireUserEventTriggered(evt);
        }

        public virtual Task WriteAsync(IChannelHandlerContext context, object message)
        {
            return context.WriteAsync(message);
        }

        public virtual void Flush(IChannelHandlerContext context)
        {
            context.Flush();
        }

        public virtual Task BindAsync(IChannelHandlerContext context, EndPoint localAddress)
        {
            return context.BindAsync(localAddress);
        }

        public virtual Task ConnectAsync(IChannelHandlerContext context, EndPoint remoteAddress, EndPoint localAddress)
        {
            return context.ConnectAsync(remoteAddress, localAddress);
        }

        public virtual Task DisconnectAsync(IChannelHandlerContext context)
        {
            return context.DisconnectAsync();
        }

        public virtual Task CloseAsync(IChannelHandlerContext context)
        {
            return context.CloseAsync();
        }

        public virtual void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            context.FireExceptionCaught(exception);
        }

        public virtual Task DeregisterAsync(IChannelHandlerContext context)
        {
            return context.DeregisterAsync();
        }

        public virtual void Read(IChannelHandlerContext context)
        {
            context.Read();
        }
    }
}