using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Helios.Concurrency;
using System.Threading;

namespace Helios.Channels.Embedded
{
    sealed class EmbeddedEventLoop : AbstractEventExecutor, IChannelHandlerInvoker, IEventLoop
    {
        readonly Queue<IRunnable> _tasks = new Queue<IRunnable>(2);

        public IEventExecutor Executor
        {
            get { return this; }
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

        public override IEventExecutor Unwrap()
        {
            return this;
        }

        IEventLoop IEventLoop.Unwrap()
        {
            return this;
        }

        public override bool IsShuttingDown
        {
            get { return false; }
        }

        public override bool IsShutDown { get { return false; } }


        public override bool IsTerminated
        {
            get { return false; }
        }

        public override bool IsInEventLoop(Thread thread)
        {
            return true;
        }

        public override Task TerminationTask { get { throw new NotSupportedException(); } }

        IEventExecutor IEventExecutor.Unwrap()
        {
            return this.Unwrap();
        }

        public override void Execute(IRunnable command)
        {
            if (command == null)
            {
                throw new NullReferenceException("command");
            }
            this._tasks.Enqueue(command);
        }

        internal void RunTasks()
        {
            for (;;)
            {
                // have to perform an additional check since Queue<T> throws upon empty dequeue in .NET
                if (this._tasks.Count == 0)
                {
                    break;
                }
                IRunnable task = this._tasks.Dequeue();
                if (task == null)
                {
                    break;
                }
                task.Run();
            }
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
    }
}