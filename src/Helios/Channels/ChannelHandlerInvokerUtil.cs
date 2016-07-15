// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Helios.Concurrency;

namespace Helios.Channels
{
    public static class ChannelHandlerInvokerUtil
    {
        // todo: use "nameof" once available
        private static readonly string ExceptionCaughtMethodName =
            ((MethodCallExpression) ((Expression<Action<IChannelHandler>>) (_ => _.ExceptionCaught(null, null))).Body)
                .Method.Name;

        public static void InvokeChannelRegisteredNow(IChannelHandlerContext ctx)
        {
            try
            {
                ctx.Handler.ChannelRegistered(ctx);
            }
            catch (Exception ex)
            {
                NotifyHandlerException(ctx, ex);
            }
        }

        public static void InvokeChannelUnregisteredNow(IChannelHandlerContext ctx)
        {
            try
            {
                ctx.Handler.ChannelUnregistered(ctx);
            }
            catch (Exception ex)
            {
                NotifyHandlerException(ctx, ex);
            }
        }

        public static void InvokeChannelActiveNow(IChannelHandlerContext ctx)
        {
            try
            {
                ctx.Handler.ChannelActive(ctx);
            }
            catch (Exception ex)
            {
                NotifyHandlerException(ctx, ex);
            }
        }

        public static void InvokeChannelInactiveNow(IChannelHandlerContext ctx)
        {
            try
            {
                ctx.Handler.ChannelInactive(ctx);
            }
            catch (Exception ex)
            {
                NotifyHandlerException(ctx, ex);
            }
        }

        public static void InvokeExceptionCaughtNow(IChannelHandlerContext ctx, Exception cause)
        {
            try
            {
                ctx.Handler.ExceptionCaught(ctx, cause);
            }
            catch (Exception ex)
            {
                if (DefaultChannelPipeline.Logger.IsWarningEnabled)
                {
                    DefaultChannelPipeline.Logger.Warning(
                        "An exception was thrown by a user handler's exceptionCaught() method: {0}", ex);
                    DefaultChannelPipeline.Logger.Warning(".. and the cause of the exceptionCaught() was: {0}", cause);
                }
            }
        }

        public static void InvokeUserEventTriggeredNow(IChannelHandlerContext ctx, object evt)
        {
            try
            {
                ctx.Handler.UserEventTriggered(ctx, evt);
            }
            catch (Exception ex)
            {
                NotifyHandlerException(ctx, ex);
            }
        }

        public static void InvokeChannelReadNow(IChannelHandlerContext ctx, object msg)
        {
            try
            {
                ctx.Handler.ChannelRead(ctx, msg);
            }
            catch (Exception ex)
            {
                NotifyHandlerException(ctx, ex);
            }
        }

        public static void InvokeChannelReadCompleteNow(IChannelHandlerContext ctx)
        {
            try
            {
                ctx.Handler.ChannelReadComplete(ctx);
            }
            catch (Exception ex)
            {
                NotifyHandlerException(ctx, ex);
            }
        }

        public static void InvokeChannelWritabilityChangedNow(IChannelHandlerContext ctx)
        {
            try
            {
                ctx.Handler.ChannelWritabilityChanged(ctx);
            }
            catch (Exception ex)
            {
                NotifyHandlerException(ctx, ex);
            }
        }

        public static Task InvokeBindAsyncNow(
            IChannelHandlerContext ctx, EndPoint localAddress)
        {
            try
            {
                return ctx.Handler.BindAsync(ctx, localAddress);
            }
            catch (Exception ex)
            {
                return ComposeExceptionTask(ex);
            }
        }

        public static Task InvokeConnectAsyncNow(
            IChannelHandlerContext ctx,
            EndPoint remoteAddress, EndPoint localAddress)
        {
            try
            {
                return ctx.Handler.ConnectAsync(ctx, remoteAddress, localAddress);
            }
            catch (Exception ex)
            {
                return ComposeExceptionTask(ex);
            }
        }

        public static Task InvokeDisconnectAsyncNow(IChannelHandlerContext ctx)
        {
            try
            {
                return ctx.Handler.DisconnectAsync(ctx);
            }
            catch (Exception ex)
            {
                return ComposeExceptionTask(ex);
            }
        }

        public static Task InvokeCloseAsyncNow(IChannelHandlerContext ctx)
        {
            try
            {
                return ctx.Handler.CloseAsync(ctx);
            }
            catch (Exception ex)
            {
                return ComposeExceptionTask(ex);
            }
        }

        public static Task InvokeDeregisterAsyncNow(IChannelHandlerContext ctx)
        {
            try
            {
                return ctx.Handler.DeregisterAsync(ctx);
            }
            catch (Exception ex)
            {
                return ComposeExceptionTask(ex);
            }
        }

        public static void InvokeReadNow(IChannelHandlerContext ctx)
        {
            try
            {
                ctx.Handler.Read(ctx);
            }
            catch (Exception ex)
            {
                NotifyHandlerException(ctx, ex);
            }
        }

        public static Task InvokeWriteAsyncNow(IChannelHandlerContext ctx, object msg)
        {
            try
            {
                return ctx.Handler.WriteAsync(ctx, msg);
            }
            catch (Exception ex)
            {
                return ComposeExceptionTask(ex);
            }
        }

        public static void InvokeFlushNow(IChannelHandlerContext ctx)
        {
            try
            {
                ctx.Handler.Flush(ctx);
            }
            catch (Exception ex)
            {
                NotifyHandlerException(ctx, ex);
            }
        }

        private static void NotifyHandlerException(IChannelHandlerContext ctx, Exception cause)
        {
            if (InExceptionCaught(cause))
            {
                if (DefaultChannelPipeline.Logger.IsWarningEnabled)
                {
                    DefaultChannelPipeline.Logger.Warning(
                        "An exception was thrown by a user handler " +
                        "while handling an exceptionCaught event {0}", cause);
                }
                return;
            }

            InvokeExceptionCaughtNow(ctx, cause);
        }

        private static Task ComposeExceptionTask(Exception cause)
        {
            var tcs = new TaskCompletionSource();
            tcs.TrySetException(cause);
            return tcs.Task;
        }

        private static bool InExceptionCaught(Exception cause)
        {
            do
            {
                var trace = new StackTrace(cause);
                for (var index = 0; index < trace.FrameCount; index++)
                {
                    var frame = trace.GetFrame(index);
                    if (frame == null)
                    {
                        break;
                    }

                    if (ExceptionCaughtMethodName.Equals(frame.GetMethod().Name, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }

                cause = cause.InnerException;
            } while (cause != null);

            return false;
        }
    }
}