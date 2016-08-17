// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Helios.Concurrency;
using Helios.Logging;
using Helios.Util;
using Helios.Util.Concurrency;

namespace Helios.Channels
{
    internal sealed class DefaultChannelPipeline : IChannelPipeline
    {
        internal static readonly ILogger Logger = LoggingFactory.GetLogger<DefaultChannelPipeline>();
        private readonly IChannel _channel;

        private readonly AbstractChannelHandlerContext _head;

        private readonly Dictionary<string, AbstractChannelHandlerContext> _nameContextMap;
        private readonly AbstractChannelHandlerContext _tail;
        private long _nextRandomName;

        public DefaultChannelPipeline(IChannel channel)
        {
            Contract.Requires(channel != null);
            Contract.Requires(channel.EventLoop is IPausableEventExecutor);

            _nameContextMap = new Dictionary<string, AbstractChannelHandlerContext>(4);
            _channel = channel;
            _tail = new TailContext(this);
            _head = new HeadContext(this);
            _head.Next = _tail;
            _tail.Prev = _head;
        }

        public IEnumerator<IChannelHandler> GetEnumerator()
        {
            var current = _head;
            while (current != null)
            {
                yield return current.Handler;
                current = current.Next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IChannelPipeline AddFirst(string name, IChannelHandler handler)
        {
            return AddFirst(null, name, handler);
        }

        public IChannelPipeline AddFirst(IChannelHandlerInvoker invoker, string name, IChannelHandler handler)
        {
            Contract.Requires(handler != null);
            lock (_head)
            {
                name = FilterName(name, handler);
                var newContext = new DefaultChannelHandlerContext(this, invoker, name, handler);

                var next = _head.Next;
                newContext.Prev = _head;
                newContext.Next = next;
                _head.Next = newContext;
                next.Prev = newContext;

                _nameContextMap.Add(name, newContext);
                CallHandlerAdded(newContext);
            }
            return this;
        }

        public IChannelPipeline AddLast(string name, IChannelHandler handler)
        {
            return AddLast(null, name, handler);
        }

        public IChannelPipeline AddLast(IChannelHandlerInvoker invoker, string name, IChannelHandler handler)
        {
            Contract.Requires(handler != null);
            lock (_head)
            {
                name = FilterName(name, handler);
                var newContext = new DefaultChannelHandlerContext(this, invoker, name, handler);

                var prev = _tail.Prev;
                newContext.Prev = prev;
                newContext.Next = _tail;
                prev.Next = newContext;
                _tail.Prev = newContext;

                _nameContextMap.Add(name, newContext);
                CallHandlerAdded(newContext);
            }
            return this;
        }

        public IChannelPipeline AddFirst(params IChannelHandler[] handlers)
        {
            return AddFirst(null, handlers);
        }

        public IChannelPipeline AddFirst(IChannelHandlerInvoker invoker, params IChannelHandler[] handlers)
        {
            Contract.Requires(handlers != null);
            foreach (var handler in handlers)
            {
                AddFirst(invoker, (string) null, handler);
            }
            return this;
        }

        public IChannelPipeline AddLast(params IChannelHandler[] handlers)
        {
            return AddLast(null, handlers);
        }

        public IChannelPipeline AddLast(IChannelHandlerInvoker invoker, params IChannelHandler[] handlers)
        {
            Contract.Requires(handlers != null);
            foreach (var handler in handlers)
            {
                AddLast(invoker, (string) null, handler);
            }
            return this;
        }

        public IChannelPipeline Remove(IChannelHandler handler)
        {
            Remove(GetContextOrThrow(handler));
            return this;
        }

        public IChannelHandler Remove(string name)
        {
            return Remove(GetContextOrThrow(name)).Handler;
        }

        public T Remove<T>() where T : class, IChannelHandler
        {
            return (T) Remove(GetContextOrThrow<T>()).Handler;
        }

        public IChannelHandler RemoveFirst()
        {
            if (_head.Next == _tail)
            {
                throw new InvalidOperationException("Pipeline is empty.");
            }
            return Remove(_head.Next).Handler;
        }

        public IChannelHandler RemoveLast()
        {
            if (_head.Next == _tail)
            {
                throw new InvalidOperationException("Pipeline is empty.");
            }
            return Remove(_tail.Prev).Handler;
        }


        public IChannelHandler First()
        {
            return FirstContext()?.Handler;
        }

        public IChannelHandlerContext FirstContext()
        {
            var first = _head.Next;
            if (first == _tail)
            {
                return null;
            }
            return first;
        }

        public IChannelHandler Last()
        {
            var last = LastContext();
            return last?.Handler;
        }

        public IChannelHandlerContext LastContext()
        {
            var last = _tail.Prev;
            if (last == _head)
            {
                return null;
            }
            return last;
        }

        public IChannelHandler Get(string name)
        {
            var ctx = Context(name);
            return ctx?.Handler;
        }

        public T Get<T>() where T : class, IChannelHandler
        {
            var ctx = Context<T>();
            return (T) ctx?.Handler;
        }

        public IChannelHandlerContext Context(IChannelHandler handler)
        {
            Contract.Requires(handler != null);

            var ctx = _head.Next;
            while (true)
            {
                if (ctx == null)
                {
                    return null;
                }

                if (ctx.Handler == handler)
                {
                    return ctx;
                }

                ctx = ctx.Next;
            }
        }

        public IChannelHandlerContext Context(string name)
        {
            Contract.Requires(name != null);
            lock (_head)
            {
                AbstractChannelHandlerContext result;
                _nameContextMap.TryGetValue(name, out result);
                return result;
            }
        }

        public IChannelHandlerContext Context<T>() where T : class, IChannelHandler
        {
            var ctx = _head.Next;
            while (true)
            {
                if (ctx == null)
                {
                    return null;
                }
                if (ctx.Handler is T)
                {
                    return ctx;
                }
                ctx = ctx.Next;
            }
        }

        public IChannel Channel()
        {
            return _channel;
        }

        public IChannelPipeline FireChannelRegistered()
        {
            _head.FireChannelRegistered();
            return this;
        }

        public IChannelPipeline FireChannelUnregistered()
        {
            _head.FireChannelUnregistered();

            if (!Channel().IsOpen)
            {
                Destroy();
            }
            return this;
        }

        public IChannelPipeline FireChannelActive()
        {
            _head.FireChannelActive();
            if (_channel.Configuration.AutoRead)
            {
                _channel.Read();
            }
            return this;
        }

        public IChannelPipeline FireChannelInactive()
        {
            _head.FireChannelInactive();
            return this;
        }

        public IChannelPipeline FireExceptionCaught(Exception cause)
        {
            _head.FireExceptionCaught(cause);
            return this;
        }

        public IChannelPipeline FireUserEventTriggered(object evt)
        {
            _head.FireUserEventTriggered(evt);
            return this;
        }

        public IChannelPipeline FireChannelRead(object msg)
        {
            _head.FireChannelRead(msg);
            return this;
        }

        public IChannelPipeline FireChannelReadComplete()
        {
            _head.FireChannelReadComplete();
            if (_channel.Configuration.AutoRead)
            {
                Read();
            }
            return this;
        }

        public IChannelPipeline FireChannelWritabilityChanged()
        {
            _head.FireChannelWritabilityChanged();
            return this;
        }

        public Task BindAsync(EndPoint localAddress)
        {
            return _tail.BindAsync(localAddress);
        }

        public Task ConnectAsync(EndPoint remoteAddress)
        {
            return _tail.ConnectAsync(remoteAddress);
        }

        public Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress)
        {
            return _tail.ConnectAsync(remoteAddress, localAddress);
        }

        public Task DisconnectAsync()
        {
            return _tail.DisconnectAsync();
        }

        public Task CloseAsync()
        {
            return _tail.CloseAsync();
        }

        public Task DeregisterAsync()
        {
            return _tail.DeregisterAsync();
        }

        public IChannelPipeline Read()
        {
            _tail.Read();
            return this;
        }

        public Task WriteAsync(object msg)
        {
            return _tail.WriteAsync(msg);
        }

        public IChannelPipeline Flush()
        {
            _tail.Flush();
            return this;
        }

        public Task WriteAndFlushAsync(object msg)
        {
            return _tail.WriteAndFlushAsync(msg);
        }

        public IChannelPipeline AddBefore(string baseName, string name, IChannelHandler handler)
        {
            return AddBefore(null, baseName, name, handler);
        }

        public IChannelPipeline AddBefore(IChannelHandlerInvoker invoker, string baseName, string name,
            IChannelHandler handler)
        {
            lock (_head)
            {
                var ctx = GetContextOrThrow(baseName);
                name = FilterName(name, handler);
                AddBeforeUnsafe(name, ctx, new DefaultChannelHandlerContext(this, invoker, name, handler));
            }
            return this;
        }

        private void AddBeforeUnsafe(string name, AbstractChannelHandlerContext ctx,
            AbstractChannelHandlerContext newCtx)
        {
            newCtx.Prev = ctx.Prev;
            newCtx.Next = ctx;
            ctx.Prev.Next = newCtx;
            ctx.Prev = newCtx;

            _nameContextMap.Add(name, newCtx);

            CallHandlerAdded(newCtx);
        }

        public IChannelPipeline AddAfter(string baseName, string name, IChannelHandler handler)
        {
            return AddAfter(null, baseName, name, handler);
        }

        public IChannelPipeline AddAfter(IChannelHandlerInvoker invoker, string baseName, string name,
            IChannelHandler handler)
        {
            lock (_head)
            {
                var ctx = GetContextOrThrow(baseName);
                name = FilterName(name, handler);
                AddAfterUnsafe(name, ctx, new DefaultChannelHandlerContext(this, invoker, name, handler));
            }
            return this;
        }

        private void AddAfterUnsafe(string name, AbstractChannelHandlerContext ctx, AbstractChannelHandlerContext newCtx)
        {
            newCtx.Prev = ctx;
            newCtx.Next = ctx.Next;
            ctx.Next.Prev = newCtx;
            ctx.Next = newCtx;

            _nameContextMap.Add(name, newCtx);

            CallHandlerAdded(newCtx);
        }

        public override string ToString()
        {
            var buf = new StringBuilder()
                .Append(GetType().Name)
                .Append('{');
            var ctx = _head.Next;
            while (true)
            {
                if (ctx == _tail)
                {
                    break;
                }

                buf.Append('(')
                    .Append(ctx.Name)
                    .Append(" = ")
                    .Append(ctx.Handler.GetType().Name)
                    .Append(')');

                ctx = ctx.Next;
                if (ctx == _tail)
                {
                    break;
                }

                buf.Append(", ");
            }
            buf.Append('}');
            return buf.ToString();
        }

        public void Destroy()
        {
            DestroyUp(_head.Next);
        }

        private void DestroyUp(AbstractChannelHandlerContext ctx)
        {
            var currentThread = Thread.CurrentThread;
            var tailContext = _tail;
            while (true)
            {
                if (ctx == tailContext)
                {
                    DestroyDown(currentThread, tailContext.Prev);
                    break;
                }

                var executor = ctx.Executor;
                if (!executor.IsInEventLoop(currentThread))
                {
                    executor.Unwrap()
                        .Execute(
                            (self, c) => ((DefaultChannelPipeline) self).DestroyUp((AbstractChannelHandlerContext) c),
                            this, ctx);
                    break;
                }

                ctx = ctx.Next;
            }
        }

        private void DestroyDown(Thread currentThread, AbstractChannelHandlerContext ctx)
        {
            // We have reached at tail; now traverse backwards.
            var headContext = _head;
            while (true)
            {
                if (ctx == headContext)
                {
                    break;
                }

                var executor = ctx.Executor;
                if (executor.IsInEventLoop(currentThread))
                {
                    lock (_head)
                    {
                        RemoveUnsafe(ctx);
                    }
                }
                else
                {
                    executor.Unwrap()
                        .Execute(
                            (self, c) =>
                                ((DefaultChannelPipeline) self).DestroyDown(Thread.CurrentThread,
                                    (AbstractChannelHandlerContext) c), this, ctx);
                    break;
                }

                ctx = ctx.Prev;
            }
        }

        private string FilterName(string name, IChannelHandler handler)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = handler.GetType().Name + "$" + Interlocked.Increment(ref _nextRandomName);
            }
            if (!_nameContextMap.ContainsKey(name))
                return name;

            throw new ArgumentException($"Duplciate handler name: {name}", nameof(name));
        }

        private AbstractChannelHandlerContext Remove(AbstractChannelHandlerContext context)
        {
            Contract.Requires(context != _head && context != _tail);

            Task future;

            lock (_head)
            {
                if (!context.Channel.Registered || context.Executor.InEventLoop)
                {
                    RemoveUnsafe(context);
                    return context;
                }
                future = context.Executor.SubmitAsync(
                    () =>
                    {
                        lock (_head)
                        {
                            RemoveUnsafe(context);
                        }
                        return 0;
                    });
            }

            // Run the following 'waiting' code outside of the above synchronized block
            // in order to avoid deadlock

            future.Wait();

            return context;
        }

        private void RemoveUnsafe(AbstractChannelHandlerContext context)
        {
            var prev = context.Prev;
            var next = context.Next;
            prev.Next = next;
            next.Prev = prev;
            _nameContextMap.Remove(context.Name);
            CallHandlerRemoved(context);
        }

        private void CallHandlerRemoved(AbstractChannelHandlerContext ctx)
        {
            if ((ctx.SkipPropagationFlags & AbstractChannelHandlerContext.MASK_HANDLER_REMOVED) != 0)
            {
                return;
            }

            if (ctx.Channel.Registered && !ctx.Executor.InEventLoop)
            {
                ctx.Executor.Execute(
                    (self, c) =>
                        ((DefaultChannelPipeline) self).CallHandlerRemovedUnsafe((AbstractChannelHandlerContext) c),
                    this, ctx);
                return;
            }
            CallHandlerRemovedUnsafe(ctx);
        }

        private void CallHandlerRemovedUnsafe(AbstractChannelHandlerContext ctx)
        {
            // Notify the complete removal.
            try
            {
                ctx.Handler.HandlerRemoved(ctx);
                ctx.Removed = true;
            }
            catch (Exception ex)
            {
                FireExceptionCaught(new ChannelPipelineException(
                    ctx.Handler.GetType().Name + ".handlerRemoved() has thrown an exception.", ex));
            }
        }

        private void CallHandlerAdded(AbstractChannelHandlerContext ctx)
        {
            if ((ctx.SkipPropagationFlags & AbstractChannelHandlerContext.MASK_HANDLER_ADDED) != 0)
            {
                return;
            }

            if (ctx.Channel.Registered && !ctx.Executor.InEventLoop)
            {
                ctx.Executor.Execute(
                    (self, c) =>
                        ((DefaultChannelPipeline) self).CallHandlerAddedUnsafe((AbstractChannelHandlerContext) c), this,
                    ctx);
                return;
            }
            CallHandlerAddedUnsafe(ctx);
        }

        private void CallHandlerAddedUnsafe(AbstractChannelHandlerContext ctx)
        {
            try
            {
                ctx.Handler.HandlerAdded(ctx);
            }
            catch (Exception ex)
            {
                var removed = false;
                try
                {
                    Remove(ctx);
                    removed = true;
                }
                catch (Exception ex2)
                {
                    if (Logger.IsWarningEnabled)
                    {
                        Logger.Warning("Failed to remove a handler: {0}; Cause {1}", ctx.Name, ex2);
                    }
                }

                if (removed)
                {
                    FireExceptionCaught(new ChannelPipelineException(
                        ctx.Handler.GetType().Name +
                        ".HandlerAdded() has thrown an exception; removed.", ex));
                }
                else
                {
                    FireExceptionCaught(new ChannelPipelineException(
                        ctx.Handler.GetType().Name +
                        ".HandlerAdded() has thrown an exception; also failed to remove.", ex));
                }
            }
        }

        public AbstractChannelHandlerContext GetContextOrThrow(string name)
        {
            var ctx = (AbstractChannelHandlerContext) Context(name);
            if (ctx == null)
            {
                throw new ArgumentException($"Handler with a name `{name}` could not be found in the pipeline.");
            }

            return ctx;
        }

        private AbstractChannelHandlerContext GetContextOrThrow(IChannelHandler handler)
        {
            var ctx = (AbstractChannelHandlerContext) Context(handler);
            if (ctx == null)
            {
                throw new ArgumentException(
                    $"Handler of type `{handler.GetType().Name}` could not be found in the pipeline.");
            }

            return ctx;
        }

        public AbstractChannelHandlerContext GetContextOrThrow<T>() where T : class, IChannelHandler
        {
            var ctx = (AbstractChannelHandlerContext) Context<T>();
            if (ctx == null)
            {
                throw new ArgumentException($"Handler of type `{typeof(T).Name}` could not be found in the pipeline.");
            }

            return ctx;
        }

        #region Head and Tail context

        internal sealed class TailContext : AbstractChannelHandlerContext, IChannelHandler
        {
            private static readonly int SkipFlags = CalculateSkipPropagationFlags(typeof(TailContext));

            public TailContext(DefaultChannelPipeline pipeline) : base(pipeline, null, "null", SkipFlags)
            {
            }

            public override IChannelHandler Handler => this;

            public void ChannelRegistered(IChannelHandlerContext context)
            {
            }

            public void ChannelUnregistered(IChannelHandlerContext context)
            {
            }

            public void ChannelActive(IChannelHandlerContext context)
            {
            }

            public void ChannelInactive(IChannelHandlerContext context)
            {
            }

            public void ExceptionCaught(IChannelHandlerContext context, Exception exception)
            {
                try
                {
                    Logger.Warning(exception,
                        "An ExceptionCaught() event was fired, and it reached at the tail of the pipeline. " +
                        "It usually means that no handler in the pipeline could handle the exception.");
                }
                finally
                {
                    ReferenceCountUtil.Release(exception);
                }
            }

            [Skip]
            public Task DeregisterAsync(IChannelHandlerContext context)
            {
                return context.DeregisterAsync();
            }

            public void ChannelRead(IChannelHandlerContext context, object message)
            {
                try
                {
                    Logger.Debug(
                        "Discarded inbound message {0} that reached at the tail of the pipeline. " +
                        "Please check your pipeline configuration.", message?.ToString());
                }
                finally
                {
                    ReferenceCountUtil.Release(message);
                }
            }

            public void ChannelReadComplete(IChannelHandlerContext context)
            {
            }

            public void ChannelWritabilityChanged(IChannelHandlerContext context)
            {
            }

            [Skip]
            public void HandlerAdded(IChannelHandlerContext context)
            {
            }

            [Skip]
            public void HandlerRemoved(IChannelHandlerContext context)
            {
            }

            [Skip]
            public Task DisconnectAsync(IChannelHandlerContext context)
            {
                return context.DisconnectAsync();
            }

            [Skip]
            public Task CloseAsync(IChannelHandlerContext context)
            {
                return context.CloseAsync();
            }

            [Skip]
            public void Read(IChannelHandlerContext context)
            {
                context.Read();
            }

            public void UserEventTriggered(IChannelHandlerContext context, object evt)
            {
                ReferenceCountUtil.Release(evt);
            }

            [Skip]
            public Task WriteAsync(IChannelHandlerContext ctx, object message)
            {
                return ctx.WriteAsync(message);
            }

            [Skip]
            public void Flush(IChannelHandlerContext context)
            {
                context.Flush();
            }

            [Skip]
            public Task BindAsync(IChannelHandlerContext context, EndPoint localAddress)
            {
                return context.BindAsync(localAddress);
            }

            [Skip]
            public Task ConnectAsync(IChannelHandlerContext context, EndPoint remoteAddress, EndPoint localAddress)
            {
                return context.ConnectAsync(remoteAddress, localAddress);
            }
        }

        internal sealed class HeadContext : AbstractChannelHandlerContext, IChannelHandler
        {
            private static readonly int SkipFlags = CalculateSkipPropagationFlags(typeof(HeadContext));

            private readonly IChannel _channel;

            public HeadContext(DefaultChannelPipeline pipeline)
                : base(pipeline, null, "<null>", SkipFlags)
            {
                _channel = pipeline.Channel();
            }

            public override IChannelHandler Handler => this;

            public void Flush(IChannelHandlerContext context)
            {
                _channel.Unsafe.Flush();
            }

            public Task BindAsync(IChannelHandlerContext context, EndPoint localAddress)
            {
                return _channel.Unsafe.BindAsync(localAddress);
            }

            public Task ConnectAsync(IChannelHandlerContext context, EndPoint remoteAddress, EndPoint localAddress)
            {
                return _channel.Unsafe.ConnectAsync(remoteAddress, localAddress);
            }

            public Task DisconnectAsync(IChannelHandlerContext context)
            {
                return _channel.Unsafe.DisconnectAsync();
            }

            public Task CloseAsync(IChannelHandlerContext context)
            {
                return _channel.Unsafe.CloseAsync();
            }

            public Task DeregisterAsync(IChannelHandlerContext context)
            {
                Contract.Assert(!((IPausableEventExecutor) context.Channel.EventLoop).IsAcceptingNewTasks);

                // submit deregistration task
                var promise = new TaskCompletionSource();
                context.Channel.EventLoop.Unwrap().Execute(
                    (u, p) => ((IChannelUnsafe) u).DeregisterAsync().LinkOutcome((TaskCompletionSource) p),
                    _channel.Unsafe,
                    promise);
                return promise.Task;
            }

            public void Read(IChannelHandlerContext context)
            {
                _channel.Unsafe.BeginRead();
            }

            public Task WriteAsync(IChannelHandlerContext context, object message)
            {
                return _channel.Unsafe.WriteAsync(message);
            }

            [Skip]
            public void ChannelWritabilityChanged(IChannelHandlerContext context)
            {
                context.FireChannelWritabilityChanged();
            }

            [Skip]
            public void HandlerAdded(IChannelHandlerContext context)
            {
            }

            [Skip]
            public void HandlerRemoved(IChannelHandlerContext context)
            {
            }

            [Skip]
            public void ExceptionCaught(IChannelHandlerContext ctx, Exception exception)
            {
                ctx.FireExceptionCaught(exception);
            }

            [Skip]
            public void ChannelRegistered(IChannelHandlerContext context)
            {
                context.FireChannelRegistered();
            }

            [Skip]
            public void ChannelUnregistered(IChannelHandlerContext context)
            {
                context.FireChannelUnregistered();
            }

            [Skip]
            public void ChannelActive(IChannelHandlerContext context)
            {
                context.FireChannelActive();
            }

            [Skip]
            public void ChannelInactive(IChannelHandlerContext context)
            {
                context.FireChannelInactive();
            }

            [Skip]
            public void ChannelRead(IChannelHandlerContext ctx, object msg)
            {
                ctx.FireChannelRead(msg);
            }

            [Skip]
            public void ChannelReadComplete(IChannelHandlerContext ctx)
            {
                ctx.FireChannelReadComplete();
            }

            [Skip]
            public void UserEventTriggered(IChannelHandlerContext context, object evt)
            {
            }
        }

        #endregion
    }
}