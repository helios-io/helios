// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Diagnostics.Contracts;
using System.Net;
using System.Threading.Tasks;
using Helios.Concurrency;
using Helios.Util;
using Helios.Util.Concurrency;

namespace Helios.Channels
{
    public class DefaultChannelHandlerInvoker : IChannelHandlerInvoker
    {
        private static readonly Action<object> InvokeChannelReadCompleteAction =
            ctx => ChannelHandlerInvokerUtil.InvokeChannelReadCompleteNow((IChannelHandlerContext) ctx);

        private static readonly Action<object> InvokeReadAction =
            ctx => ChannelHandlerInvokerUtil.InvokeReadNow((IChannelHandlerContext) ctx);

        private static readonly Action<object> InvokeChannelWritabilityChangedAction =
            ctx => ChannelHandlerInvokerUtil.InvokeChannelWritabilityChangedNow((IChannelHandlerContext) ctx);

        private static readonly Action<object> InvokeFlushAction =
            ctx => ChannelHandlerInvokerUtil.InvokeFlushNow((IChannelHandlerContext) ctx);

        private static readonly Action<object, object> InvokeUserEventTriggeredAction =
            (ctx, evt) => ChannelHandlerInvokerUtil.InvokeUserEventTriggeredNow((IChannelHandlerContext) ctx, evt);

        private static readonly Action<object, object> InvokeChannelReadAction =
            (ctx, msg) => ChannelHandlerInvokerUtil.InvokeChannelReadNow((IChannelHandlerContext) ctx, msg);

        private static readonly Action<object, object> InvokeWriteAsyncAction = (p, msg) =>
        {
            var promise = (TaskCompletionSource) p;
            var context = (IChannelHandlerContext) promise.Task.AsyncState;
            var channel = (AbstractChannel) context.Channel;
            // todo: size is counted twice. is that a problem?
            var size = channel.EstimatorHandle.Size(msg);
            if (size > 0)
            {
                var buffer = channel.Unsafe.OutboundBuffer;
                // Check for null as it may be set to null if the channel is closed already
                if (buffer != null)
                {
                    buffer.DecrementPendingOutboundBytes(size);
                }
            }
            ChannelHandlerInvokerUtil.InvokeWriteAsyncNow(context, msg).LinkOutcome(promise);
        };

        public DefaultChannelHandlerInvoker(IEventExecutor executor)
        {
            Contract.Requires(executor != null);
            Executor = executor;
        }

        public IEventExecutor Executor { get; }

        public void InvokeChannelRegistered(IChannelHandlerContext ctx)
        {
            if (Executor.InEventLoop)
            {
                ChannelHandlerInvokerUtil.InvokeChannelRegisteredNow(ctx);
            }
            else
            {
                Executor.Execute(c => ChannelHandlerInvokerUtil.InvokeChannelRegisteredNow((IChannelHandlerContext) c),
                    ctx);
            }
        }

        public void InvokeChannelUnregistered(IChannelHandlerContext ctx)
        {
            if (Executor.InEventLoop)
            {
                ChannelHandlerInvokerUtil.InvokeChannelUnregisteredNow(ctx);
            }
            else
            {
                Executor.Execute(
                    c => ChannelHandlerInvokerUtil.InvokeChannelUnregisteredNow((IChannelHandlerContext) c), ctx);
            }
        }

        public void InvokeChannelActive(IChannelHandlerContext ctx)
        {
            if (Executor.InEventLoop)
            {
                ChannelHandlerInvokerUtil.InvokeChannelActiveNow(ctx);
            }
            else
            {
                Executor.Execute(c => ChannelHandlerInvokerUtil.InvokeChannelActiveNow((IChannelHandlerContext) c), ctx);
            }
        }

        public void InvokeChannelInactive(IChannelHandlerContext ctx)
        {
            if (Executor.InEventLoop)
            {
                ChannelHandlerInvokerUtil.InvokeChannelInactiveNow(ctx);
            }
            else
            {
                Executor.Execute(c => ChannelHandlerInvokerUtil.InvokeChannelInactiveNow((IChannelHandlerContext) c),
                    ctx);
            }
        }

        public void InvokeExceptionCaught(IChannelHandlerContext ctx, Exception cause)
        {
            Contract.Requires(cause != null);

            if (Executor.InEventLoop)
            {
                ChannelHandlerInvokerUtil.InvokeExceptionCaughtNow(ctx, cause);
            }
            else
            {
                try
                {
                    Executor.Execute(
                        (c, e) =>
                            ChannelHandlerInvokerUtil.InvokeExceptionCaughtNow((IChannelHandlerContext) c, (Exception) e),
                        ctx, cause);
                }
                catch (Exception t)
                {
                    if (DefaultChannelPipeline.Logger.IsWarningEnabled)
                    {
                        DefaultChannelPipeline.Logger.Warning(
                            "Failed to submit an ExceptionCaught() event. Cause: {0}", t);
                        DefaultChannelPipeline.Logger.Warning(
                            "The ExceptionCaught() event that was failed to submit was: {0}", cause);
                    }
                }
            }
        }

        public void InvokeUserEventTriggered(IChannelHandlerContext ctx, object evt)
        {
            Contract.Requires(evt != null);

            if (Executor.InEventLoop)
            {
                ChannelHandlerInvokerUtil.InvokeUserEventTriggeredNow(ctx, evt);
            }
            else
            {
                SafeProcessInboundMessage(InvokeUserEventTriggeredAction, ctx, evt);
            }
        }

        public void InvokeChannelRead(IChannelHandlerContext ctx, object msg)
        {
            Contract.Requires(msg != null);

            if (Executor.InEventLoop)
            {
                ChannelHandlerInvokerUtil.InvokeChannelReadNow(ctx, msg);
            }
            else
            {
                SafeProcessInboundMessage(InvokeChannelReadAction, ctx, msg);
            }
        }

        public void InvokeChannelReadComplete(IChannelHandlerContext ctx)
        {
            if (Executor.InEventLoop)
            {
                ChannelHandlerInvokerUtil.InvokeChannelReadCompleteNow(ctx);
            }
            else
            {
                Executor.Execute(InvokeChannelReadCompleteAction, ctx);
            }
        }

        public void InvokeChannelWritabilityChanged(IChannelHandlerContext ctx)
        {
            if (Executor.InEventLoop)
            {
                ChannelHandlerInvokerUtil.InvokeChannelWritabilityChangedNow(ctx);
            }
            else
            {
                Executor.Execute(InvokeChannelWritabilityChangedAction, ctx);
            }
        }

        public Task InvokeBindAsync(
            IChannelHandlerContext ctx, EndPoint localAddress)
        {
            Contract.Requires(localAddress != null);
            // todo: check for cancellation
            //if (!validatePromise(ctx, promise, false)) {
            //    // promise cancelled
            //    return;
            //}

            if (Executor.InEventLoop)
            {
                return ChannelHandlerInvokerUtil.InvokeBindAsyncNow(ctx, localAddress);
            }
            return SafeExecuteOutboundAsync(() => ChannelHandlerInvokerUtil.InvokeBindAsyncNow(ctx, localAddress));
        }

        public Task InvokeConnectAsync(
            IChannelHandlerContext ctx,
            EndPoint remoteAddress, EndPoint localAddress)
        {
            Contract.Requires(remoteAddress != null);
            // todo: check for cancellation
            //if (!validatePromise(ctx, promise, false)) {
            //    // promise cancelled
            //    return;
            //}

            if (Executor.InEventLoop)
            {
                return ChannelHandlerInvokerUtil.InvokeConnectAsyncNow(ctx, remoteAddress, localAddress);
            }
            return
                SafeExecuteOutboundAsync(
                    () => ChannelHandlerInvokerUtil.InvokeConnectAsyncNow(ctx, remoteAddress, localAddress));
        }

        public Task InvokeDisconnectAsync(IChannelHandlerContext ctx)
        {
            // todo: check for cancellation
            //if (!validatePromise(ctx, promise, false)) {
            //    // promise cancelled
            //    return;
            //}

            if (Executor.InEventLoop)
            {
                return ChannelHandlerInvokerUtil.InvokeDisconnectAsyncNow(ctx);
            }
            return SafeExecuteOutboundAsync(() => ChannelHandlerInvokerUtil.InvokeDisconnectAsyncNow(ctx));
        }

        public Task InvokeCloseAsync(IChannelHandlerContext ctx)
        {
            // todo: check for cancellation
            //if (!validatePromise(ctx, promise, false)) {
            //    // promise cancelled
            //    return;
            //}

            if (Executor.InEventLoop)
            {
                return ChannelHandlerInvokerUtil.InvokeCloseAsyncNow(ctx);
            }
            return SafeExecuteOutboundAsync(() => ChannelHandlerInvokerUtil.InvokeCloseAsyncNow(ctx));
        }

        public Task InvokeDeregisterAsync(IChannelHandlerContext ctx)
        {
            // todo: check for cancellation
            //if (!validatePromise(ctx, promise, false)) {
            //    // promise cancelled
            //    return;
            //}

            if (Executor.InEventLoop)
            {
                return ChannelHandlerInvokerUtil.InvokeDeregisterAsyncNow(ctx);
            }
            return SafeExecuteOutboundAsync(() => ChannelHandlerInvokerUtil.InvokeDeregisterAsyncNow(ctx));
        }

        public void InvokeRead(IChannelHandlerContext ctx)
        {
            if (Executor.InEventLoop)
            {
                ChannelHandlerInvokerUtil.InvokeReadNow(ctx);
            }
            else
            {
                Executor.Execute(InvokeReadAction, ctx);
            }
        }

        public Task InvokeWriteAsync(IChannelHandlerContext ctx, object msg)
        {
            Contract.Requires(msg != null);
            // todo: check for cancellation
            //if (!validatePromise(ctx, promise, false)) {
            //    // promise cancelled
            //    return;
            //}

            if (Executor.InEventLoop)
            {
                return ChannelHandlerInvokerUtil.InvokeWriteAsyncNow(ctx, msg);
            }
            var channel = (AbstractChannel) ctx.Channel;
            var promise = new TaskCompletionSource(ctx);

            try
            {
                var size = channel.EstimatorHandle.Size(msg);
                if (size > 0)
                {
                    var buffer = channel.Unsafe.OutboundBuffer;
                    // Check for null as it may be set to null if the channel is closed already
                    if (buffer != null)
                    {
                        buffer.IncrementPendingOutboundBytes(size);
                    }
                }

                Executor.Execute(InvokeWriteAsyncAction, promise, msg);
            }
            catch (Exception cause)
            {
                ReferenceCountUtil.Release(msg);
                promise.TrySetException(cause);
            }
            return promise.Task;
        }

        public void InvokeFlush(IChannelHandlerContext ctx)
        {
            if (Executor.InEventLoop)
            {
                ChannelHandlerInvokerUtil.InvokeFlushNow(ctx);
            }
            else
            {
                Executor.Execute(InvokeFlushAction, ctx);
            }
        }

        private void SafeProcessInboundMessage(Action<object, object> action, object state, object msg)
        {
            var success = false;
            try
            {
                Executor.Execute(action, state, msg);
                success = true;
            }
            finally
            {
                if (!success)
                {
                    ReferenceCountUtil.Release(msg);
                }
            }
        }

        private Task SafeExecuteOutboundAsync(Func<Task> function)
        {
            var promise = new TaskCompletionSource();
            try
            {
                Executor.Execute((p, func) => ((Func<Task>) func)().LinkOutcome((TaskCompletionSource) p), promise,
                    function);
            }
            catch (Exception cause)
            {
                promise.TrySetException(cause);
            }
            return promise.Task;
        }
    }
}

