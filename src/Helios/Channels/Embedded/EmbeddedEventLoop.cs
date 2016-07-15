// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Helios.Concurrency;

namespace Helios.Channels.Embedded
{
    internal sealed class EmbeddedEventLoop : AbstractEventExecutor, IChannelHandlerInvoker, IEventLoop
    {
        private readonly Queue<IRunnable> _tasks = new Queue<IRunnable>(20);

        public IEventExecutor Executor
        {
            get { return this; }
        }

        public void InvokeChannelRegistered(IChannelHandlerContext ctx)
        {
            ChannelHandlerInvokerUtil.InvokeChannelRegisteredNow(ctx);
        }

        public void InvokeChannelUnregistered(IChannelHandlerContext ctx)
        {
            ChannelHandlerInvokerUtil.InvokeChannelUnregisteredNow(ctx);
        }

        public void InvokeChannelActive(IChannelHandlerContext ctx)
        {
            ChannelHandlerInvokerUtil.InvokeChannelActiveNow(ctx);
        }

        public void InvokeChannelInactive(IChannelHandlerContext ctx)
        {
            ChannelHandlerInvokerUtil.InvokeChannelInactiveNow(ctx);
        }

        public void InvokeExceptionCaught(IChannelHandlerContext ctx, Exception cause)
        {
            ChannelHandlerInvokerUtil.InvokeExceptionCaughtNow(ctx, cause);
        }

        public void InvokeUserEventTriggered(IChannelHandlerContext ctx, object evt)
        {
            ChannelHandlerInvokerUtil.InvokeUserEventTriggeredNow(ctx, evt);
        }

        public void InvokeChannelRead(IChannelHandlerContext ctx, object msg)
        {
            ChannelHandlerInvokerUtil.InvokeChannelReadNow(ctx, msg);
        }

        public void InvokeChannelReadComplete(IChannelHandlerContext ctx)
        {
            ChannelHandlerInvokerUtil.InvokeChannelReadCompleteNow(ctx);
        }

        public void InvokeChannelWritabilityChanged(IChannelHandlerContext ctx)
        {
            ChannelHandlerInvokerUtil.InvokeChannelWritabilityChangedNow(ctx);
        }

        public Task InvokeBindAsync(IChannelHandlerContext ctx, EndPoint localAddress)
        {
            return ChannelHandlerInvokerUtil.InvokeBindAsyncNow(ctx, localAddress);
        }

        public Task InvokeConnectAsync(IChannelHandlerContext ctx, EndPoint remoteAddress, EndPoint localAddress)
        {
            return ChannelHandlerInvokerUtil.InvokeConnectAsyncNow(ctx, remoteAddress, localAddress);
        }

        public Task InvokeDisconnectAsync(IChannelHandlerContext ctx)
        {
            return ChannelHandlerInvokerUtil.InvokeDisconnectAsyncNow(ctx);
        }

        public Task InvokeCloseAsync(IChannelHandlerContext ctx)
        {
            return ChannelHandlerInvokerUtil.InvokeCloseAsyncNow(ctx);
        }

        public Task InvokeDeregisterAsync(IChannelHandlerContext ctx)
        {
            return ChannelHandlerInvokerUtil.InvokeDeregisterAsyncNow(ctx);
        }

        public void InvokeRead(IChannelHandlerContext ctx)
        {
            ChannelHandlerInvokerUtil.InvokeReadNow(ctx);
        }

        public Task InvokeWriteAsync(IChannelHandlerContext ctx, object msg)
        {
            return ChannelHandlerInvokerUtil.InvokeWriteAsyncNow(ctx, msg);
        }

        public void InvokeFlush(IChannelHandlerContext ctx)
        {
            ChannelHandlerInvokerUtil.InvokeFlushNow(ctx);
        }

        public IChannelHandlerInvoker Invoker
        {
            get { return this; }
        }

        public Task RegisterAsync(IChannel channel)
        {
            return channel.Unsafe.RegisterAsync(this);
        }

        public override Task GracefulShutdownAsync(TimeSpan quietPeriod, TimeSpan timeout)
        {
            throw new NotSupportedException();
        }

        IEventLoop IEventLoop.Unwrap()
        {
            return this;
        }

        public override bool IsShuttingDown
        {
            get { return false; }
        }

        public override bool IsShutDown
        {
            get { return false; }
        }


        public override bool IsTerminated
        {
            get { return false; }
        }

        public override bool IsInEventLoop(Thread thread)
        {
            return true;
        }

        public override Task TerminationTask
        {
            get { throw new NotSupportedException(); }
        }

        IEventExecutor IEventExecutor.Unwrap()
        {
            return Unwrap();
        }

        public override void Execute(IRunnable command)
        {
            if (command == null)
            {
                throw new NullReferenceException("command");
            }
            _tasks.Enqueue(command);
        }

        public override IEventExecutor Unwrap()
        {
            return this;
        }

        internal void RunTasks()
        {
            for (;;)
            {
                // have to perform an additional check since Queue<T> throws upon empty dequeue in .NET
                if (_tasks.Count == 0)
                {
                    break;
                }
                var task = _tasks.Dequeue();
                if (task == null)
                {
                    break;
                }
                task.Run();
            }
        }
    }
}