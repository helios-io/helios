using System;
using System.Threading.Tasks;
using Helios.Channels.Extensions;
using Helios.Net;
using Helios.Ops;
using Helios.Topology;

namespace Helios.Channels.Impl
{
    public class DefaultChannelHandlerInvoker : IChannelHandlerInvoker
    {
        public DefaultChannelHandlerInvoker(IExecutor executor)
        {
            if(executor == null) throw new ArgumentNullException("executor");
            Executor = executor;
        }

        public IExecutor Executor { get; private set; }
        public void InvokeChannelRegistered(IChannelHandlerContext handlerContext)
        {
            if (Executor.IsInEventLoop())
            {
                ChannelHandlerInvokerUtil.InvokeChannelRegisteredNow(handlerContext);
            }
            else
            {
                Executor.Execute(() => ChannelHandlerInvokerUtil.InvokeChannelRegisteredNow(handlerContext));
            }
        }

        public void InvokeChannelActive(IChannelHandlerContext handlerContext)
        {
            if (Executor.IsInEventLoop())
            {
                ChannelHandlerInvokerUtil.InvokeChannelActiveNow(handlerContext);
            }
            else
            {
                Executor.Execute(() => ChannelHandlerInvokerUtil.InvokeChannelActiveNow(handlerContext));
            }
        }

        public void InvokeChannelInactive(IChannelHandlerContext handlerContext)
        {
            if (Executor.IsInEventLoop())
            {
                ChannelHandlerInvokerUtil.InvokeChannelInactiveNow(handlerContext);
            }
            else
            {
                Executor.Execute(() => ChannelHandlerInvokerUtil.InvokeChannelInactiveNow(handlerContext));
            }
        }

        public void InvokeExceptionCaught(IChannelHandlerContext handlerContext, Exception cause)
        {
            if(cause == null) throw new ArgumentNullException("cause");

            if (Executor.IsInEventLoop())
            {
                ChannelHandlerInvokerUtil.InvokeExceptionCaughtNow(handlerContext, cause);
            }
            else
            {
                Executor.Execute(() => ChannelHandlerInvokerUtil.InvokeExceptionCaughtNow(handlerContext, cause));
            }
        }

        public void InvokeUserEventTriggered(IChannelHandlerContext handlerContext, object evt)
        {
            if(evt == null) throw new ArgumentNullException("evt");

            if (Executor.IsInEventLoop())
            {
                ChannelHandlerInvokerUtil.InvokeUserEventTriggeredNow(handlerContext, evt);
            }
            else
            {
                Executor.Execute(() => ChannelHandlerInvokerUtil.InvokeUserEventTriggeredNow(handlerContext, evt));
            }
        }

        public void InvokeChannelRead(IChannelHandlerContext handlerContext, NetworkData message)
        {
            if(message == null) throw new ArgumentNullException("message");

            if (Executor.IsInEventLoop())
            {
                ChannelHandlerInvokerUtil.InvokeChannelReadNow(handlerContext, message);
            }
            else
            {
                Executor.Execute(() => ChannelHandlerInvokerUtil.InvokeChannelReadNow(handlerContext, message));
            }
        }

        public void InvokeChannelReadComplete(IChannelHandlerContext handlerContext)
        {
            if (Executor.IsInEventLoop())
            {
                ChannelHandlerInvokerUtil.InvokeChannelReadCompleteNow(handlerContext);
            }
            else
            {
                var dctx = (DefaultChannelHandlerContext) handlerContext;
                var task = dctx.invokeChannelReadCompleteTask;
                if (task == null)
                {
                    //Executor.Execute(() => ChannelHandlerInvokerUtil.InvokeChannelReadCompleteNow(handlerContext));
                }
                
            }
        }

        public void InvokeChannelWritabilityChanged(IChannelHandlerContext handlerContext)
        {
            if (Executor.IsInEventLoop())
            {
                ChannelHandlerInvokerUtil.InvokeChannelWritabilityChangedNow(handlerContext);
            }
            else
            {
                Executor.Execute(() => ChannelHandlerInvokerUtil.InvokeChannelWritabilityChangedNow(handlerContext));
            }
        }

        public void InvokeBind(IChannelHandlerContext handlerContext, INode localAddress, TaskCompletionSource<bool> bindCompletionSource)
        {
            if (Executor.IsInEventLoop())
            {
                ChannelHandlerInvokerUtil.InvokeBindNow(handlerContext, localAddress, bindCompletionSource);
            }
            else
            {
                Executor.Execute(() => ChannelHandlerInvokerUtil.InvokeBindNow(handlerContext, localAddress, bindCompletionSource));
            }
        }

        public void InvokeConnect(IChannelHandlerContext handlerContext, INode remoteAddress, INode localAddress,
            TaskCompletionSource<bool> connectCompletionSource)
        {
            if (Executor.IsInEventLoop())
            {
                ChannelHandlerInvokerUtil.InvokeConnectNow(handlerContext, remoteAddress, localAddress, connectCompletionSource);
            }
            else
            {
                Executor.Execute(() => ChannelHandlerInvokerUtil.InvokeConnectNow(handlerContext, remoteAddress, localAddress, connectCompletionSource));
            }
        }

        public void InvokeDisconnect(IChannelHandlerContext handlerContext, TaskCompletionSource<bool> disconnectCompletionSource)
        {
            throw new NotImplementedException();
        }

        public void InvokeClose(IChannelHandlerContext handlerContext, TaskCompletionSource<bool> closeCompletionSource)
        {
            throw new NotImplementedException();
        }

        public void InvokeRead(IChannelHandlerContext handlerContext)
        {
            throw new NotImplementedException();
        }

        public void InvokeWrite(IChannelHandlerContext handlerContext, NetworkData message, TaskCompletionSource<bool> writeCompletionSource)
        {
            throw new NotImplementedException();
        }

        public void InvokeFlush(IChannelHandlerContext handlerContext)
        {
            throw new NotImplementedException();
        }
    }
}