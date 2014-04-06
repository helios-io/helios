using System;
using System.Threading.Tasks;
using Helios.Net;
using Helios.Topology;

namespace Helios.Channels
{
    /// <summary>
    /// Bare-bones implementation of an <see cref="IChannelHandler"/>
    /// </summary>
    public class ChannelHandlerAdapter : IChannelHandler
    {
        public virtual void HandlerAdded(IChannelHandlerContext handlerContext)
        {
            //NO-OP
        }

        public virtual void HandlerRemoved(IChannelHandlerContext handlerContext)
        {
            //NO-OP
        }

        public virtual void ExceptionCaught(IChannelHandlerContext handlerContext, Exception ex)
        {
            handlerContext.FireExceptionCaught(ex);
        }

        public virtual void ChannelRegistered(IChannelHandlerContext handlerContext)
        {
            handlerContext.FireChannelRegistered();
        }

        public virtual void ChannelActive(IChannelHandlerContext handlerContext)
        {
            handlerContext.FireChannelActive();
        }

        public virtual void ChannelInactive(IChannelHandlerContext handlerContext)
        {
            handlerContext.FireChannelInactive();
        }

        public virtual void ChannelRead(IChannelHandlerContext handlerContext, NetworkData message)
        {
            handlerContext.FireChannelRead(message);
        }

        public virtual void ChannelWritabilityChanged(IChannelHandlerContext handlerContext)
        {
            handlerContext.FireChannelWritabilityChanged();
        }

        public virtual void Bind(IChannelHandlerContext handlerContext, INode localAddress, TaskCompletionSource<bool> bindCompletionSource)
        {
            handlerContext.Bind(localAddress, bindCompletionSource);
        }

        public virtual void Connect(IChannelHandlerContext handlerContext, INode remoteAddress, INode localAddress,
            TaskCompletionSource<bool> connectCompletionSource)
        {
            handlerContext.Connect(remoteAddress, localAddress, connectCompletionSource);
        }

        public virtual void Disconnect(IChannelHandlerContext handlerContext, TaskCompletionSource<bool> disconnectCompletionSource)
        {
            handlerContext.Disconnect(disconnectCompletionSource);
        }

        public virtual void Close(IChannelHandlerContext handlerContext, TaskCompletionSource<bool> closeCompletionSource)
        {
            handlerContext.Close(closeCompletionSource);
        }

        public virtual void Write(IChannelHandlerContext handlerContext, NetworkData message, TaskCompletionSource<bool> writeCompletionSource)
        {
            handlerContext.Write(message, writeCompletionSource);
        }

        public virtual void Flush(IChannelHandlerContext handlerContext)
        {
            handlerContext.Flush();
        }

        public void Read(IChannelHandlerContext handlerContext)
        {
            handlerContext.Read();
        }
    }
}