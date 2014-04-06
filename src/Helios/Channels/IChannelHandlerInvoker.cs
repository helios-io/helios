using System;
using System.Threading.Tasks;
using Helios.Net;
using Helios.Ops;
using Helios.Topology;

namespace Helios.Channels
{
    /// <summary>
    /// Executor responsible for invocating methods and events via the <see cref="IChannelHandlerContext"/>
    /// </summary>
    public interface IChannelHandlerInvoker
    {
        /// <summary>
        /// The <see cref="IExecutor"/> responsible for carrying out all invocations
        /// </summary>
        IExecutor Executor { get; }

        /// <summary>
        /// Invokes <see cref="IChannelHandlerContext.ChannelRegistered"/>. This method is not for a user but for
        /// the internal <see cref="IChannelHandlerContext"/> implementation.
        /// </summary>
        void InvokeChannelRegistered(IChannelHandlerContext handlerContext);

        void InvokeChannelActive(IChannelHandlerContext handlerContext);

        void InvokeChannelInactive(IChannelHandlerContext handlerContext);

        void InvokeExceptionCaught(IChannelHandlerContext handlerContext, Exception cause);

        void InvokeUserEventTriggered(IChannelHandlerContext handlerContext, object evt);

        void InvokeChannelRead(IChannelHandlerContext handlerContext, NetworkData message);

        void InvokeChannelReadComplete(IChannelHandlerContext handlerContext);

        void InvokeChannelWritabilityChanged(IChannelHandlerContext handlerContext);

        void InvokeBind(IChannelHandlerContext handlerContext, INode localAddress,
            ChannelPromise<bool> bindCompletionSource);

        void InvokeConnect(IChannelHandlerContext handlerContext, INode remoteAddress, INode localAddress,
            ChannelPromise<bool> connectCompletionSource);

        void InvokeDisconnect(IChannelHandlerContext handlerContext,
            ChannelPromise<bool> disconnectCompletionSource);

        void InvokeClose(IChannelHandlerContext handlerContext, ChannelPromise<bool> closeCompletionSource);

        void InvokeRead(IChannelHandlerContext handlerContext);

        void InvokeWrite(IChannelHandlerContext handlerContext, NetworkData message,
            ChannelPromise<bool> writeCompletionSource);

        void InvokeFlush(IChannelHandlerContext handlerContext);
    }
}
