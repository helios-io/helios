using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Helios.Concurrency;
using Helios.Logging;

namespace Helios.Channels
{
    sealed class DefaultChannelPipeline : IChannelPipeline
    {
        internal static readonly ILogger Logger = LoggingFactory.GetLogger<DefaultChannelPipeline>();

        static readonly ConditionalWeakTable<Type, string>[] NameCaches = CreateNameCaches();

        static ConditionalWeakTable<Type, string>[] CreateNameCaches()
        {
            int processorCount = Environment.ProcessorCount;
            var caches = new ConditionalWeakTable<Type, string>[processorCount];
            for (int i = 0; i < processorCount; i++)
            {
                caches[i] = new ConditionalWeakTable<Type, string>();
            }
            return caches;
        }

        private readonly IChannel _channel;

        private readonly AbstractChannelHandlerContext _head;
        private readonly AbstractChannelHandlerContext _tail;

        private readonly Dictionary<string, AbstractChannelHandlerContext> _nameContextMap;

        public DefaultChannelPipeline(IChannel channel)
        {
            Contract.Requires(channel != null);
            Contract.Requires(channel.EventLoop is IPausableEventExecutor);

            _nameContextMap = new Dictionary<string, AbstractChannelHandlerContext>(4);
        }

        public IEnumerator<IChannelHandler> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IChannelPipeline AddFirst(string name, IChannelHandler handler)
        {
            throw new NotImplementedException();
        }

        public IChannelPipeline AddFirst(IChannelHandlerInvoker invoker, string name, IChannelHandler handler)
        {
            throw new NotImplementedException();
        }

        public IChannelPipeline AddLast(string name, IChannelHandler handler)
        {
            throw new NotImplementedException();
        }

        public IChannelPipeline AddLast(IChannelHandlerInvoker invoker, string name, IChannelHandler handler)
        {
            throw new NotImplementedException();
        }

        public IChannelPipeline AddBefore(string baseName, string name, IChannelHandler handler)
        {
            throw new NotImplementedException();
        }

        public IChannelPipeline AddBefore(IChannelHandlerInvoker invoker, string baseName, string name, IChannelHandler handler)
        {
            throw new NotImplementedException();
        }

        public IChannelPipeline AddAfter(string baseName, string name, IChannelHandler handler)
        {
            throw new NotImplementedException();
        }

        public IChannelPipeline AddAfter(IChannelHandlerInvoker invoker, string baseName, string name, IChannelHandler handler)
        {
            throw new NotImplementedException();
        }

        public IChannelPipeline AddFirst(params IChannelHandler[] handlers)
        {
            throw new NotImplementedException();
        }

        public IChannelPipeline AddFirst(IChannelHandlerInvoker invoker, params IChannelHandler[] handlers)
        {
            throw new NotImplementedException();
        }

        public IChannelPipeline AddLast(params IChannelHandler[] handlers)
        {
            throw new NotImplementedException();
        }

        public IChannelPipeline AddLast(IChannelHandlerInvoker invoker, params IChannelHandler[] handlers)
        {
            throw new NotImplementedException();
        }

        public IChannelPipeline Remove(IChannelHandler handler)
        {
            throw new NotImplementedException();
        }

        public IChannelHandler Remove(string name)
        {
            throw new NotImplementedException();
        }

        public T Remove<T>() where T : class, IChannelHandler
        {
            throw new NotImplementedException();
        }

        public IChannelHandler RemoveFirst()
        {
            throw new NotImplementedException();
        }

        public IChannelHandler RemoveLast()
        {
            throw new NotImplementedException();
        }

        public IChannelPipeline Replace(IChannelHandler oldHandler, string newName, IChannelHandler newHandler)
        {
            throw new NotImplementedException();
        }

        public IChannelHandler Replace(string oldName, string newName, IChannelHandler newHandler)
        {
            throw new NotImplementedException();
        }

        public T Replace<T>(string newName, IChannelHandler newHandler) where T : class, IChannelHandler
        {
            throw new NotImplementedException();
        }

        public IChannelHandler First()
        {
            throw new NotImplementedException();
        }

        public IChannelHandlerContext FirstContext()
        {
            throw new NotImplementedException();
        }

        public IChannelHandler Last()
        {
            throw new NotImplementedException();
        }

        public IChannelHandlerContext LastContext()
        {
            throw new NotImplementedException();
        }

        public IChannelHandler Get(string name)
        {
            throw new NotImplementedException();
        }

        public T Get<T>() where T : class, IChannelHandler
        {
            throw new NotImplementedException();
        }

        public IChannelHandlerContext Context(IChannelHandler handler)
        {
            throw new NotImplementedException();
        }

        public IChannelHandlerContext Context(string name)
        {
            throw new NotImplementedException();
        }

        public IChannelHandlerContext Context<T>() where T : class, IChannelHandler
        {
            throw new NotImplementedException();
        }

        public IChannel Channel()
        {
            return _channel;
        }

        public IChannelPipeline FireChannelRegistered()
        {
            throw new NotImplementedException();
        }

        public IChannelPipeline FireChannelUnregistered()
        {
            throw new NotImplementedException();
        }

        public IChannelPipeline FireChannelActive()
        {
            throw new NotImplementedException();
        }

        public IChannelPipeline FireChannelInactive()
        {
            throw new NotImplementedException();
        }

        public IChannelPipeline FireExceptionCaught(Exception cause)
        {
            throw new NotImplementedException();
        }

        public IChannelPipeline FireUserEventTriggered(object evt)
        {
            throw new NotImplementedException();
        }

        public IChannelPipeline FireChannelRead(object msg)
        {
            throw new NotImplementedException();
        }

        public IChannelPipeline FireChannelReadComplete()
        {
            throw new NotImplementedException();
        }

        public IChannelPipeline FireChannelWritabilityChanged()
        {
            throw new NotImplementedException();
        }

        public Task BindAsync(EndPoint localAddress)
        {
            throw new NotImplementedException();
        }

        public Task ConnectAsync(EndPoint remoteAddress)
        {
            throw new NotImplementedException();
        }

        public Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress)
        {
            throw new NotImplementedException();
        }

        public Task DisconnectAsync()
        {
            throw new NotImplementedException();
        }

        public Task CloseAsync()
        {
            throw new NotImplementedException();
        }

        public Task DeregisterAsync()
        {
            throw new NotImplementedException();
        }

        public IChannelPipeline Read()
        {
            throw new NotImplementedException();
        }

        public Task WriteAsync(object msg)
        {
            throw new NotImplementedException();
        }

        public IChannelPipeline Flush()
        {
            throw new NotImplementedException();
        }

        public Task WriteAndFlushAsync(object msg)
        {
            throw new NotImplementedException();
        }

        #region Head and Tail context

        sealed class TailContext : AbstractChannelHandlerContext, IChannelHandler
        {
            private static readonly int SkipFlags = CalculateSkipPropagationFlags(typeof (TailContext));

            public TailContext(IChannelPipeline pipeline) : base(pipeline, null, "null", SkipFlags)
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

            public void ChannelRead(IChannelHandlerContext context, object message)
            {
                
            }

            public void ChannelReadComplete(IChannelHandlerContext context)
            {
                
            }

            public void ChannelWritabilityChanged(IChannelHandlerContext context)
            {
                
            }

            public void HandlerAdded(IChannelHandlerContext context)
            {
                
            }

            public void HandlerRemoved(IChannelHandlerContext context)
            {
                throw new NotImplementedException();
            }

            public Task WriteAsync(IChannelHandlerContext context, object message)
            {
                return context.WriteAsync(message);
            }

            public void Flush(IChannelHandlerContext context)
            {
                throw new NotImplementedException();
            }

            public Task BindAsync(IChannelHandlerContext context, EndPoint localAddress)
            {
                throw new NotImplementedException();
            }

            public Task ConnectAsync(IChannelHandlerContext context, EndPoint remoteAddress, EndPoint localAddress)
            {
                throw new NotImplementedException();
            }

            public Task DisconnectAsync(IChannelHandlerContext context)
            {
                throw new NotImplementedException();
            }

            public Task CloseAsync(IChannelHandlerContext context)
            {
                throw new NotImplementedException();
            }

            public void ExceptionCaught(IChannelHandlerContext context, Exception exception)
            {
                throw new NotImplementedException();
            }

            public Task DeregisterAsync(IChannelHandlerContext context)
            {
                throw new NotImplementedException();
            }

            public void Read(IChannelHandlerContext context)
            {
                throw new NotImplementedException();
            }

            public void UserEventTriggered(IChannelHandlerContext context, object evt)
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
