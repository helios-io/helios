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
                    dctx.invokeChannelReadCompleteTask = task = new Task(() => InvokeChannelReadComplete(handlerContext));
                }
                Executor.Execute(task);
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
                var dctx = (DefaultChannelHandlerContext)handlerContext;
                var task = dctx.invokeChannelWritableStateChangedTask;
                if (task == null)
                {
                    dctx.invokeChannelWritableStateChangedTask = task = new Task(() => ChannelHandlerInvokerUtil.InvokeChannelWritabilityChangedNow(handlerContext));
                }
                Executor.Execute(task);
            }
        }

        public void InvokeBind(IChannelHandlerContext handlerContext, INode localAddress, TaskCompletionSource<bool> bindCompletionSource)
        {
            if(localAddress == null) throw new ArgumentNullException("localAddress");

            if (!ValidatePromise(handlerContext, bindCompletionSource, false)) return; //promise cancelled

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
            if(!ValidatePromise(handlerContext, disconnectCompletionSource, false)) return; //promise cancelled

            if (Executor.IsInEventLoop())
            {
                ChannelHandlerInvokerUtil.InvokeDisconnectNow(handlerContext, disconnectCompletionSource);
            }
            else
            {
                Executor.Execute(() => ChannelHandlerInvokerUtil.InvokeDisconnectNow(handlerContext, disconnectCompletionSource));
            }
        }

        public void InvokeClose(IChannelHandlerContext handlerContext, TaskCompletionSource<bool> closeCompletionSource)
        {
            if (!ValidatePromise(handlerContext, closeCompletionSource, false)) return; // promise cancelled

            if (Executor.IsInEventLoop())
            {
                ChannelHandlerInvokerUtil.InvokeCloseNow(handlerContext, closeCompletionSource);
            }
            else
            {
                Executor.Execute(() => ChannelHandlerInvokerUtil.InvokeCloseNow(handlerContext, closeCompletionSource));
            }
        }

        public void InvokeRead(IChannelHandlerContext handlerContext)
        {
            if (Executor.IsInEventLoop())
            {
                ChannelHandlerInvokerUtil.InvokeReadNow(handlerContext);
            }
            else
            {
                Executor.Execute(() => ChannelHandlerInvokerUtil.InvokeReadNow(handlerContext));
            }
        }

        public void InvokeWrite(IChannelHandlerContext handlerContext, NetworkData message, TaskCompletionSource<bool> writeCompletionSource)
        {
            if(message == null) throw new ArgumentNullException("message");
            if (!ValidatePromise(handlerContext, writeCompletionSource, true)) return; //promise cancelled

            if (Executor.IsInEventLoop())
            {
                ChannelHandlerInvokerUtil.InvokeWriteNow(handlerContext, message, writeCompletionSource);
            }
            else
            {
                Executor.Execute(() => ChannelHandlerInvokerUtil.InvokeWriteNow(handlerContext, message, writeCompletionSource));
            }
        }

        public void InvokeFlush(IChannelHandlerContext handlerContext)
        {
            if (Executor.IsInEventLoop())
            {
                ChannelHandlerInvokerUtil.InvokeFlushNow(handlerContext);
            }
            else
            {
                var dctx = (DefaultChannelHandlerContext) handlerContext;
                var task = dctx.invokeFlushTask;
                if (task == null)
                {
                    dctx.invokeFlushTask =
                        task = new Task(() => ChannelHandlerInvokerUtil.InvokeFlushNow(handlerContext));
                }
                Executor.Execute(task);
            }
        }

        #region Internal methods

        private static bool ValidatePromise(IChannelHandlerContext context, TaskCompletionSource<bool> promise,
            bool allowVoidPromise)
        {
            if(context == null) throw new ArgumentNullException("context");
            if(promise == null) throw new ArgumentNullException("promise");

            if (promise.Task.IsCanceled || promise.Task.IsFaulted)
            {
                return false;
            }

            if (promise.Task.IsCompleted)
            {
                throw new ArgumentException("task has already finished", "promise");
            }

            if (!allowVoidPromise && promise is VoidChannelPromise)
            {
                throw new ArgumentException("VoidPromise is now allowed for this operation");
            }

            return true;
        }

        #endregion
    }
}