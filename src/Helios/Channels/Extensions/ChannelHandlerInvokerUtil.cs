using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Helios.Net;
using Helios.Topology;
using Newtonsoft.Json.Bson;

namespace Helios.Channels.Extensions
{
    /// <summary>
    /// Static helper class used for working with the <see cref="IChannelHandlerInvoker"/>
    /// </summary>
    public static class ChannelHandlerInvokerUtil
    {
        public static Action<Exception> ExecutorExceptionHandler(this IChannelHandlerContext context)
        {
            return ex =>
            {
                try
                {
                    context.FireExceptionCaught(ex);
                }
                catch (Exception e)
                {
                    Debug.Write("Unable to handle exception {0}", e.Message);
                }
            };
        }

        public static void InvokeChannelRegisteredNow(IChannelHandlerContext context)
        {
            try
            {
                context.Handler.ChannelRegistered(context);
            }
            catch (Exception ex)
            {
                NotifyHandlerException(context, ex);
            }
        }

        public static void InvokeChannelActiveNow(IChannelHandlerContext context)
        {
            try
            {
                context.Handler.ChannelActive(context);
            }
            catch (Exception ex)
            {
                NotifyHandlerException(context, ex);
            }
        }

        public static void InvokeChannelInactiveNow(IChannelHandlerContext context)
        {
            try
            {
                context.Handler.ChannelInactive(context);
            }
            catch (Exception ex)
            {
                NotifyHandlerException(context, ex);
            }
        }

        public static void InvokeExceptionCaughtNow(IChannelHandlerContext context, Exception e)
        {
            try
            {
                context.Handler.ExceptionCaught(context, e);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to handle exception {0}", ex.Message);
            }
        }

        public static void InvokeUserEventTriggeredNow(IChannelHandlerContext context, object evt)
        {
            try
            {
                context.Handler.UserEventTriggered(context, evt);
            }
            catch (Exception ex)
            {
                NotifyHandlerException(context, ex);
            }
        }

        public static void InvokeChannelReadNow(IChannelHandlerContext context, NetworkData message)
        {
            try
            {
                context.Handler.ChannelRead(context, message);
            }
            catch (Exception ex)
            {
                NotifyHandlerException(context, ex);
            }
        }

        public static void InvokeChannelReadCompleteNow(IChannelHandlerContext context)
        {
            try
            {
                context.Handler.ChannelReadComplete(context);
            }
            catch (Exception ex)
            {
                NotifyHandlerException(context, ex);
            }
        }

        public static void InvokeChannelWritabilityChangedNow(IChannelHandlerContext context)
        {
            try
            {
                context.Handler.ChannelWritabilityChanged(context);
            }
            catch (Exception ex)
            {
                NotifyHandlerException(context, ex);
            }
        }

        public static void InvokeBindNow(IChannelHandlerContext context, INode localAddress,
            TaskCompletionSource<bool> bindCompletionSource)
        {
            try
            {
                context.Handler.Bind(context, localAddress, bindCompletionSource);
            }
            catch (Exception ex)
            {
                NotifyOutboundHandlerException(ex, bindCompletionSource);
            }
        }

        public static void InvokeConnectNow(IChannelHandlerContext context, INode remoteAddress, INode localAddress,
            TaskCompletionSource<bool> connectCompletionSource)
        {
            try
            {
                context.Handler.Connect(context, remoteAddress, localAddress, connectCompletionSource);
            }
            catch (Exception ex)
            {
                NotifyOutboundHandlerException(ex, connectCompletionSource);
            }
        }

        public static void InvokeDisconnectNow(IChannelHandlerContext context,
            TaskCompletionSource<bool> disconnectCompletionSource)
        {
            try
            {
                context.Handler.Disconnect(context, disconnectCompletionSource);   
            }
            catch (Exception ex)
            {
                NotifyOutboundHandlerException(ex, disconnectCompletionSource);
            }
        }

        public static void InvokeCloseNow(IChannelHandlerContext context,
            TaskCompletionSource<bool> closeCompletionSource)
        {
            try
            {
                context.Handler.Close(context, closeCompletionSource);
            }
            catch (Exception ex)
            {
                NotifyOutboundHandlerException(ex, closeCompletionSource);
            }
        }

        public static void InvokeReadNow(IChannelHandlerContext context)
        {
            try
            {
                context.Handler.Read(context);
            }
            catch (Exception ex)
            {
                NotifyHandlerException(context, ex);
            }
        }

        public static void InvokeWriteNow(IChannelHandlerContext context, NetworkData message,
            TaskCompletionSource<bool> writeCompletionSource)
        {
            try
            {
                context.Handler.Write(context, message, writeCompletionSource);   
            }
            catch (Exception ex)
            {
                NotifyOutboundHandlerException(ex, writeCompletionSource);
            }
        }

        public static void InvokeFlushNow(IChannelHandlerContext context)
        {
            try
            {
                context.Handler.Flush(context);
            }
            catch (Exception ex)
            {
                NotifyHandlerException(context, ex);
            }
        }

        public static void InvokeWriteAndFlushNow(IChannelHandlerContext context, NetworkData message,
            TaskCompletionSource<bool> writeCompletionSource)
        {
            try
            {
                context.Handler.Write(context, message, writeCompletionSource);
                context.Handler.Flush(context);
            }
            catch (Exception ex)
            {
                NotifyOutboundHandlerException(ex, writeCompletionSource);
            }
        }

        #region Exception propagators

        private static void NotifyHandlerException(IChannelHandlerContext context, Exception ex)
        {
            InvokeExceptionCaughtNow(context, ex);
        }

        private static void NotifyOutboundHandlerException(Exception cause, TaskCompletionSource<bool> completionSource)
        {
            if (completionSource is VoidChannelPromise) return; //do nothing - don't propogate the error further

            if (!completionSource.TrySetException(cause))
            {
                Debug.Write(string.Format("Failed to fail task completion source because it's done already {0} {1}", completionSource, cause));
            }
        }

        #endregion
    }
}