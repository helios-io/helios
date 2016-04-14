using System;
using System.Net;
using System.Threading.Tasks;
using Helios.Ops;

namespace Helios.Channels
{
    public interface IChannelHandlerInvoker
    {
        IExecutor Executor { get; }

        void InvokeChannelRegistered(IChannelHandlerContext ctx);

        void InvokeChannelUnregistered(IChannelHandlerContext ctx);

        void InvokeChannelActive(IChannelHandlerContext ctx);

        void InvokeChannelInactive(IChannelHandlerContext ctx);

        void InvokeExceptionCaught(IChannelHandlerContext ctx, Exception cause);

        void InvokeUserEventTriggered(IChannelHandlerContext ctx, object evt);

        void InvokeChannelRead(IChannelHandlerContext ctx, object msg);

        void InvokeChannelReadComplete(IChannelHandlerContext ctx);

        void InvokeChannelWritabilityChanged(IChannelHandlerContext ctx);

        Task InvokeBindAsync(IChannelHandlerContext ctx, EndPoint localAddress);

        Task InvokeConnectAsync(
            IChannelHandlerContext ctx, EndPoint remoteAddress, EndPoint localAddress);

        Task InvokeDisconnectAsync(IChannelHandlerContext ctx);

        Task InvokeCloseAsync(IChannelHandlerContext ctx);

        Task InvokeDeregisterAsync(IChannelHandlerContext ctx);

        void InvokeRead(IChannelHandlerContext ctx);

        Task InvokeWriteAsync(IChannelHandlerContext ctx, object msg);

        void InvokeFlush(IChannelHandlerContext ctx);
    }
}