// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Diagnostics.Contracts;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Helios.Buffers;
using Helios.Concurrency;
using Helios.Util;

namespace Helios.Channels
{
    internal abstract class AbstractChannelHandlerContext : IChannelHandlerContext
    {
        internal const int MASK_HANDLER_ADDED = 1;
        internal const int MASK_HANDLER_REMOVED = 1 << 1;
        internal const int MASK_EXCEPTION_CAUGHT = 1 << 2;
        internal const int MASK_CHANNEL_REGISTERED = 1 << 3;
        internal const int MASK_CHANNEL_UNREGISTERED = 1 << 4;
        internal const int MASK_CHANNEL_ACTIVE = 1 << 5;
        internal const int MASK_CHANNEL_INACTIVE = 1 << 6;
        internal const int MASK_CHANNEL_READ = 1 << 7;
        internal const int MASK_CHANNEL_READ_COMPLETE = 1 << 8;
        internal const int MASK_CHANNEL_WRITABILITY_CHANGED = 1 << 9;
        internal const int MASK_USER_EVENT_TRIGGERED = 1 << 10;
        internal const int MASK_BIND = 1 << 11;
        internal const int MASK_CONNECT = 1 << 12;
        internal const int MASK_DISCONNECT = 1 << 13;
        internal const int MASK_CLOSE = 1 << 14;
        internal const int MASK_DEREGISTER = 1 << 15;
        internal const int MASK_READ = 1 << 16;
        internal const int MASK_WRITE = 1 << 17;
        internal const int MASK_FLUSH = 1 << 18;

        internal const int MASKGROUP_INBOUND = MASK_EXCEPTION_CAUGHT |
                                               MASK_CHANNEL_REGISTERED |
                                               MASK_CHANNEL_UNREGISTERED |
                                               MASK_CHANNEL_ACTIVE |
                                               MASK_CHANNEL_INACTIVE |
                                               MASK_CHANNEL_READ |
                                               MASK_CHANNEL_READ_COMPLETE |
                                               MASK_CHANNEL_WRITABILITY_CHANGED |
                                               MASK_USER_EVENT_TRIGGERED;

        internal const int MASKGROUP_OUTBOUND = MASK_BIND |
                                                MASK_CONNECT |
                                                MASK_DISCONNECT |
                                                MASK_CLOSE |
                                                MASK_DEREGISTER |
                                                MASK_READ |
                                                MASK_WRITE |
                                                MASK_FLUSH;

        private static readonly ConditionalWeakTable<Type, Tuple<int>> SkipTable =
            new ConditionalWeakTable<Type, Tuple<int>>();

        private readonly IChannelHandlerInvoker _invoker;
        internal readonly int SkipPropagationFlags;
        private volatile PausableChannelEventExecutor _wrappedEventLoop;
        internal volatile AbstractChannelHandlerContext Next;
        internal volatile AbstractChannelHandlerContext Prev;

        public IChannel Channel => Pipeline.Channel();
        public IByteBufAllocator Allocator => Channel.Allocator;

        public IEventExecutor Executor
        {
            get
            {
                if (_invoker == null)
                {
                    return Channel.EventLoop;
                }
                return WrappedEventLoop;
            }
        }

        protected static int GetSkipPropagationFlags(IChannelHandler handler)
        {
            var skipDirection = SkipTable.GetValue(
                handler.GetType(),
                handlerType => Tuple.Create(CalculateSkipPropagationFlags(handlerType)));

            return skipDirection == null ? 0 : skipDirection.Item1;
        }

        internal static int CalculateSkipPropagationFlags(Type handlerType)
        {
            var flags = 0;

            // this method should never throw
            if (IsSkippable(handlerType, "HandlerAdded"))
            {
                flags |= MASK_HANDLER_ADDED;
            }
            if (IsSkippable(handlerType, "HandlerRemoved"))
            {
                flags |= MASK_HANDLER_REMOVED;
            }
            if (IsSkippable(handlerType, "ExceptionCaught", typeof(Exception)))
            {
                flags |= MASK_EXCEPTION_CAUGHT;
            }
            if (IsSkippable(handlerType, "ChannelRegistered"))
            {
                flags |= MASK_CHANNEL_REGISTERED;
            }
            if (IsSkippable(handlerType, "ChannelUnregistered"))
            {
                flags |= MASK_CHANNEL_UNREGISTERED;
            }
            if (IsSkippable(handlerType, "ChannelActive"))
            {
                flags |= MASK_CHANNEL_ACTIVE;
            }
            if (IsSkippable(handlerType, "ChannelInactive"))
            {
                flags |= MASK_CHANNEL_INACTIVE;
            }
            if (IsSkippable(handlerType, "ChannelRead", typeof(object)))
            {
                flags |= MASK_CHANNEL_READ;
            }
            if (IsSkippable(handlerType, "ChannelReadComplete"))
            {
                flags |= MASK_CHANNEL_READ_COMPLETE;
            }
            if (IsSkippable(handlerType, "ChannelWritabilityChanged"))
            {
                flags |= MASK_CHANNEL_WRITABILITY_CHANGED;
            }
            if (IsSkippable(handlerType, "UserEventTriggered", typeof(object)))
            {
                flags |= MASK_USER_EVENT_TRIGGERED;
            }
            if (IsSkippable(handlerType, "BindAsync", typeof(EndPoint)))
            {
                flags |= MASK_BIND;
            }
            if (IsSkippable(handlerType, "ConnectAsync", typeof(EndPoint), typeof(EndPoint)))
            {
                flags |= MASK_CONNECT;
            }
            if (IsSkippable(handlerType, "DisconnectAsync"))
            {
                flags |= MASK_DISCONNECT;
            }
            if (IsSkippable(handlerType, "CloseAsync"))
            {
                flags |= MASK_CLOSE;
            }
            if (IsSkippable(handlerType, "DeregisterAsync"))
            {
                flags |= MASK_DEREGISTER;
            }
            if (IsSkippable(handlerType, "Read"))
            {
                flags |= MASK_READ;
            }
            if (IsSkippable(handlerType, "WriteAsync", typeof(object)))
            {
                flags |= MASK_WRITE;
            }
            if (IsSkippable(handlerType, "Flush"))
            {
                flags |= MASK_FLUSH;
            }
            return flags;
        }

        protected static bool IsSkippable(Type handlerType, string methodName, params Type[] paramTypes)
        {
            var newParamTypes = new Type[paramTypes.Length + 1];
            newParamTypes[0] = typeof(IChannelHandlerContext);
            Array.Copy(paramTypes, 0, newParamTypes, 1, paramTypes.Length);
            return handlerType.GetMethod(methodName, newParamTypes).GetCustomAttribute<SkipAttribute>(false) != null;
        }

        private PausableChannelEventExecutor WrappedEventLoop
        {
            get
            {
                var wrapped = _wrappedEventLoop;
                if (wrapped == null)
                {
                    wrapped = new PausableChannelEventExecutorImpl(this);
#pragma warning disable 420 // does not apply to Interlocked operations
                    if (Interlocked.CompareExchange(ref _wrappedEventLoop, wrapped, null) != null)
#pragma warning restore 420
                    {
                        // Set in the meantime so we need to issue another volatile read
                        return _wrappedEventLoop;
                    }
                }
                return wrapped;
            }
        }

        public IChannelHandlerInvoker Invoker
        {
            get
            {
                if (_invoker == null)
                    return Channel.EventLoop.Invoker;
                throw new NotImplementedException();
            }
        }

        public IChannelPipeline Pipeline { get; }

        public string Name { get; }
        public abstract IChannelHandler Handler { get; }
        public bool Removed { get; internal set; }

        protected AbstractChannelHandlerContext(DefaultChannelPipeline pipeline, IChannelHandlerInvoker invoker,
            string name,
            int skipPropagationFlags)
        {
            Contract.Requires(pipeline != null);
            Contract.Requires(name != null);

            Pipeline = pipeline;
            _invoker = invoker;
            Name = name;
            SkipPropagationFlags = skipPropagationFlags;
        }

        public IChannelHandlerContext FireChannelRegistered()
        {
            var next = FindContextInbound();
            next.Invoker.InvokeChannelRegistered(next);
            return this;
        }

        public IChannelHandlerContext FireChannelUnregistered()
        {
            var next = FindContextInbound();
            next.Invoker.InvokeChannelUnregistered(next);
            return this;
        }

        public IChannelHandlerContext FireChannelActive()
        {
            var next = FindContextInbound();
            next.Invoker.InvokeChannelActive(next);
            return this;
        }

        public IChannelHandlerContext FireChannelInactive()
        {
            var next = FindContextInbound();
            next.Invoker.InvokeChannelInactive(next);
            return this;
        }

        public IChannelHandlerContext FireChannelRead(object message)
        {
            var next = FindContextInbound();
            ReferenceCountUtil.Touch(message, next);
            next.Invoker.InvokeChannelRead(next, message);
            return this;
        }

        public IChannelHandlerContext FireChannelReadComplete()
        {
            var next = FindContextInbound();
            next.Invoker.InvokeChannelReadComplete(next);
            return this;
        }

        public IChannelHandlerContext FireChannelWritabilityChanged()
        {
            var next = FindContextInbound();
            next.Invoker.InvokeChannelWritabilityChanged(next);
            return this;
        }

        public IChannelHandlerContext FireExceptionCaught(Exception ex)
        {
            var next = FindContextInbound();
            next.Invoker.InvokeExceptionCaught(next, ex);
            return this;
        }

        public IChannelHandlerContext FireUserEventTriggered(object evt)
        {
            var next = FindContextInbound();
            next.Invoker.InvokeUserEventTriggered(next, evt);
            return this;
        }

        public IChannelHandlerContext Read()
        {
            var next = FindContextOutbound();
            next.Invoker.InvokeRead(next);
            return this;
        }

        public Task WriteAsync(object message)
        {
            var next = FindContextOutbound();
            ReferenceCountUtil.Touch(message, next);
            return next.Invoker.InvokeWriteAsync(next, message);
        }

        public IChannelHandlerContext Flush()
        {
            var next = FindContextOutbound();
            next.Invoker.InvokeFlush(next);
            return this;
        }

        public Task WriteAndFlushAsync(object message)
        {
            var target = FindContextOutbound();
            ReferenceCountUtil.Touch(message, target);
            var writeTask = target.Invoker.InvokeWriteAsync(target, message);
            target = FindContextOutbound();
            target.Invoker.InvokeFlush(target);
            return writeTask;
        }

        public Task BindAsync(EndPoint localAddress)
        {
            var target = FindContextOutbound();
            return target.Invoker.InvokeBindAsync(target, localAddress);
        }

        public Task ConnectAsync(EndPoint remoteAddress)
        {
            return ConnectAsync(remoteAddress, null);
        }

        public Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress)
        {
            var target = FindContextOutbound();
            return target.Invoker.InvokeConnectAsync(target, remoteAddress, localAddress);
        }

        public Task DisconnectAsync()
        {
            if (!Channel.DisconnectSupported)
            {
                return CloseAsync();
            }

            var target = FindContextOutbound();
            return target.Invoker.InvokeDisconnectAsync(target);
        }

        public Task CloseAsync()
        {
            var target = FindContextOutbound();
            return target.Invoker.InvokeCloseAsync(target);
        }

        public Task DeregisterAsync()
        {
            var next = FindContextOutbound();
            return next.Invoker.InvokeDeregisterAsync(next);
        }

        private AbstractChannelHandlerContext FindContextInbound()
        {
            var ctx = this;
            do
            {
                ctx = ctx.Next;
            } while ((ctx.SkipPropagationFlags & MASKGROUP_INBOUND) == MASKGROUP_INBOUND);
            return ctx;
        }

        private AbstractChannelHandlerContext FindContextOutbound()
        {
            var ctx = this;
            do
            {
                ctx = ctx.Prev;
            } while ((ctx.SkipPropagationFlags & MASKGROUP_OUTBOUND) == MASKGROUP_OUTBOUND);
            return ctx;
        }

        public override string ToString()
        {
            return $"{GetType().Name} ({Name}, {Channel})";
        }

        private class PausableChannelEventExecutorImpl : PausableChannelEventExecutor
        {
            private readonly AbstractChannelHandlerContext _context;

            public PausableChannelEventExecutorImpl(AbstractChannelHandlerContext context)
            {
                _context = context;
            }

            public override bool IsAcceptingNewTasks
                => ((PausableChannelEventExecutor) Channel.EventLoop).IsAcceptingNewTasks;

            internal override IChannel Channel => _context.Channel;

            public override IEventExecutor Unwrap()
            {
                return UnwrapInvoker().Executor;
            }

            public override void RejectNewTasks()
            {
                ((PausableChannelEventExecutor) Channel.EventLoop).RejectNewTasks();
            }

            public override void AcceptNewTasks()
            {
                ((PausableChannelEventExecutor) Channel.EventLoop).AcceptNewTasks();
            }

            private IChannelHandlerInvoker UnwrapInvoker()
            {
                return _context.Invoker;
            }
        }
    }
}