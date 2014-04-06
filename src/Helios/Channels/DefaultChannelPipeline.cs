using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Helios.Channels.Extensions;
using Helios.Net;
using Helios.Ops;
using Helios.Topology;
using Helios.Util.Collections;

namespace Helios.Channels
{
    public sealed class DefaultChannelPipeline : IChannelPipeline
    {
        private readonly AbstractChannel _channel;
        private static readonly IDictionary<Type, string> nameCache = new Dictionary<Type, string>();
        private static readonly object NameCacheLock = new object();

        private object _nameCtxLock = new object();
        private readonly IDictionary<string, IChannelHandlerContext> _name2Ctx = new Dictionary<string, IChannelHandlerContext>(4);
        private readonly IDictionary<IEventLoop, IChannelHandlerInvoker> _childInvokers = new Dictionary<IEventLoop, IChannelHandlerInvoker>();

        public DefaultChannelPipeline(AbstractChannel channel)
        {
            _channel = channel;

            var tailHandler = new TailHandler();
            Tail = new DefaultChannelHandlerContext(this, null, GenerateName(tailHandler), tailHandler);

            var headHandler = new HeadHandler(channel.Unsafe);
            Head = new DefaultChannelHandlerContext(this, null, GenerateName(headHandler), headHandler);

            Head.next = Tail;
            Tail.prev = Head;
        }

        internal readonly DefaultChannelHandlerContext Head;
        internal readonly DefaultChannelHandlerContext Tail;

        #region IChannelPipeline manipulation methods
        public IChannelPipeline AddFirst(string name, IChannelHandler handler)
        {
            return AddFirst(default(IChannelHandlerInvoker), name, handler);
        }

        public IChannelPipeline AddFirst(IEventLoop loop, string name, IChannelHandler handler)
        {
            return AddFirst(FindInvoker(loop), name, handler);
        }

        public IChannelPipeline AddFirst(IChannelHandlerInvoker invoker, string name, IChannelHandler handler)
        {
            lock (_nameCtxLock)
            {
                CheckDuplicateName(name);

                var newCtx = new DefaultChannelHandlerContext(this, invoker, name, handler);
                AddFirstInternal(name, newCtx);
            }

            return this;
        }

        private void AddFirstInternal(string name, DefaultChannelHandlerContext newCtx)
        {
            CheckMultiplicity(newCtx);
            var nextCtx = Head.next;
            newCtx.prev = Head;
            newCtx.next = nextCtx;
            Head.next = newCtx;
            nextCtx.prev = newCtx;

            _name2Ctx.Add(name, newCtx);

            CallHandlerAdded(newCtx);
        }

        public IChannelPipeline AddLast(string name, IChannelHandler handler)
        {
            return AddLast(default(IChannelHandlerInvoker), name, handler);
        }

        public IChannelPipeline AddLast(IEventLoop loop, string name, IChannelHandler handler)
        {
            return AddLast(FindInvoker(loop), name, handler);
        }

        public IChannelPipeline AddLast(IChannelHandlerInvoker invoker, string name, IChannelHandler handler)
        {
            lock (_nameCtxLock)
            {
                CheckDuplicateName(name);

                var newCtx = new DefaultChannelHandlerContext(this, invoker, name, handler);
                AddLastInternal(name, newCtx);
            }

            return this;
        }

        private void AddLastInternal(string name, DefaultChannelHandlerContext newCtx)
        {
            CheckMultiplicity(newCtx);
            var prev = Tail.prev;
            newCtx.prev = prev;
            newCtx.next = Tail;
            prev.next = newCtx;
            Tail.prev = newCtx;

            _name2Ctx.Add(name, newCtx);

            CallHandlerAdded(newCtx);
        }

        public IChannelPipeline AddBefore(string baseName, string name, IChannelHandler handler)
        {
            return AddBefore(default(IChannelHandlerInvoker), baseName, name, handler);
        }

        public IChannelPipeline AddBefore(IEventLoop loop, string baseName, string name, IChannelHandler handler)
        {
            return AddBefore(FindInvoker(loop), baseName, name, handler);
        }

        public IChannelPipeline AddBefore(IChannelHandlerInvoker invoker, string baseName, string name, IChannelHandler handler)
        {
            lock (_nameCtxLock)
            {
                var ctx = GetContextOrDie(baseName);

                CheckDuplicateName(name);

                var newCtx = new DefaultChannelHandlerContext(this, invoker, name, handler);
                AddBeforeInternal(name, ctx, newCtx);
            }

            return this;
        }

        private void AddBeforeInternal(string name, DefaultChannelHandlerContext original,
            DefaultChannelHandlerContext newCtx)
        {
            CheckMultiplicity(newCtx);
            newCtx.prev = original.prev;
            newCtx.next = original;
            original.prev.next = newCtx;
            original.prev = newCtx;

            _name2Ctx.Add(name, newCtx);

            CallHandlerAdded(newCtx);
        }

        public IChannelPipeline AddAfter(string baseName, string name, IChannelHandler handler)
        {
            return AddAfter(default(IChannelHandlerInvoker), baseName, name, handler);
        }

        public IChannelPipeline AddAfter(IEventLoop loop, string baseName, string name, IChannelHandler handler)
        {
            return AddAfter(FindInvoker(loop), baseName, name, handler);
        }

        public IChannelPipeline AddAfter(IChannelHandlerInvoker invoker, string baseName, string name, IChannelHandler handler)
        {
            lock (_nameCtxLock)
            {
                var ctx = GetContextOrDie(baseName);

                CheckDuplicateName(name);

                var newCtx = new DefaultChannelHandlerContext(this, invoker, name, handler);
                AddAfterInternal(name, ctx, newCtx);
            }

            return this;
        }

        private void AddAfterInternal(string name, DefaultChannelHandlerContext ctx, DefaultChannelHandlerContext newCtx)
        {
            CheckMultiplicity(newCtx);

            newCtx.prev = ctx;
            newCtx.next = ctx.next;
            ctx.next.prev = newCtx;
            ctx.next = newCtx;

            _name2Ctx.Add(name, newCtx);

            CallHandlerAdded(newCtx);
        }

        public IChannelHandler Remove(string name)
        {
            return Remove(GetContextOrDie(name)).Handler;
        }

        public IChannelPipeline Remove(IChannelHandler handler)
        {
            Remove(GetContextOrDie(handler));
            return this;
        }

        public IChannelPipeline Remove<T>() where T : IChannelHandler
        {
            Remove(GetContextOrDie<T>());
            return this;
        }

        internal DefaultChannelHandlerContext Remove(DefaultChannelHandlerContext ctx)
        {
            Debug.Assert(ctx != Head && ctx != Tail);

            DefaultChannelHandlerContext context;
            Task waitable;

            lock (_nameCtxLock)
            {
                if (!ctx.Channel.IsRegistered || ctx.Executor.IsInEventLoop())
                {
                    RemoveInternal(ctx);
                    return ctx;
                }
                else
                {
                    waitable = new Task(() =>
                    {
                        lock (_nameCtxLock)
                        {
                            RemoveInternal(ctx);
                        }
                    });
                    
                    context = ctx;
                }
            }

            ctx.Executor.Execute(waitable);
            //Wait outside the lock in order to avoid deadlock
            waitable.Wait();

            return context;
        }

        private void RemoveInternal(DefaultChannelHandlerContext ctx)
        {
            var prev = ctx.prev;
            var next = ctx.next;
            prev.next = next;
            next.prev = prev;
            _name2Ctx.Remove(ctx.Name);
            CallHandlerRemoved(ctx);
        }

        public IChannelHandler RemoveFirst()
        {
            if(Head.next == Tail) throw new KeyNotFoundException("Cannot remove head or tail from pipeline");

            return Remove(Head.next).Handler;
        }

        public IChannelHandler RemoveLast()
        {
            if (Head.next == Tail) throw new KeyNotFoundException("Cannot remove head or tail from pipeline");

            return Remove(Tail.prev).Handler;
        }

        public IChannelPipeline Replace(IChannelHandler oldHandler, string newName, IChannelHandler newHandler)
        {
            Replace(GetContextOrDie(oldHandler), newName, newHandler);
            return this;
        }

        public IChannelHandler Replace(string oldName, string newName, IChannelHandler newHandler)
        {
            return Replace(GetContextOrDie(oldName), newName, newHandler);
        }

        public IChannelHandler Replace<T>(string newName, IChannelHandler newHandler) where T : IChannelHandler
        {
            return Replace(GetContextOrDie<T>(), newName, newHandler);
        }

        private IChannelHandler Replace(DefaultChannelHandlerContext ctx, string newName, IChannelHandler newHandler)
        {
            Debug.Assert(ctx != Head && ctx != Tail);

            Task waitable;
            lock (_nameCtxLock)
            {
                var sameName = ctx.Name.Equals(newName);
                if (!sameName)
                {
                    CheckDuplicateName(newName);
                }

                var newCtx = new DefaultChannelHandlerContext(this, ctx.Invoker, newName, newHandler);
                if (!newCtx.Channel.IsRegistered || newCtx.Executor.IsInEventLoop())
                {
                    ReplaceInternal(ctx, newName, newCtx);
                    return ctx.Handler;
                }
                else
                {
                    waitable = new Task(() =>
                    {
                        lock (_nameCtxLock)
                        {
                            ReplaceInternal(ctx, newName, newCtx);
                        }
                    });
                    
                }

            }

            ctx.Executor.Execute(waitable);
            //Wait outside the lock in order to avoid deadlock
            waitable.Wait();

            return ctx.Handler;
        }

        private void ReplaceInternal(DefaultChannelHandlerContext oldCtx, string newName,
            DefaultChannelHandlerContext newCtx)
        {
            CheckMultiplicity(newCtx);

            var prev = oldCtx.prev;
            var next = oldCtx.next;
            newCtx.prev = prev;
            newCtx.next = next;

            //Finish the replacement of oldCtx with newCtx in the linked list
            prev.next = newCtx;
            next.prev = newCtx;

            if (!oldCtx.Name.Equals(newName))
            {
                _name2Ctx.Remove(oldCtx.Name);
            }
            _name2Ctx.AddOrSet(newName, newCtx);

            //update the reference to the replacement so forward of buffered content will work correctly
            oldCtx.prev = newCtx;
            oldCtx.next = newCtx;

            //Invoke newHandler.HandlerAdded first
            CallHandlerAdded(newCtx);
            CallHandlerRemoved(oldCtx);
        }

        public IChannelHandler First()
        {
            var first = FirstContext();
            return first == null ? null : first.Handler;
        }

        public IChannelHandlerContext FirstContext()
        {
            var first = Head.next;
            return first == Tail ? null : Head.next;
        }

        public IChannelHandler Last()
        {
            var last = LastContext();
            return last == null ? null : last.Handler;
        }

        public IChannelHandlerContext LastContext()
        {
            var last = Tail.prev;
            return last == Head ? null : last;
        }

        public IChannelHandler Get(string name)
        {
            var ctx = Context(name);
            return ctx == null ? null : ctx.Handler;
        }

        public IChannelHandler Get<T>() where T : IChannelHandler
        {
            var ctx = Context<T>();
            return ctx == null ? null : ctx.Handler;
        }

        public IChannelHandlerContext Context(IChannelHandler handler)
        {
            if(handler == null) throw new ArgumentNullException("handler");

            var ctx = Head.next;
            for (;;)
            {
                if (ctx == null) return null;

                if (ctx.Handler == handler) return ctx;

                ctx = ctx.next;
            }
        }

        public IChannelHandlerContext Context(string name)
        {
            if(string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            lock (_nameCtxLock)
            {
                IChannelHandlerContext context = null;
                _name2Ctx.TryGetValue(name, out context);
                return context;
            }
        }

        public IChannelHandlerContext Context<T>() where T : IChannelHandler
        {
            var handlerType = typeof (T);
            var ctx = Head.next;
            for (;;)
            {
                if (ctx == null) return null;

                if (handlerType.IsInstanceOfType(ctx.Handler))
                {
                    return ctx;
                }

                ctx = ctx.next;
            }
        }



        #endregion

        public List<string> Names
        {
            get
            {
                var list = new List<string>();
                var ctx = Head.next;
                for (;;)
                {
                    if (ctx == null)
                        return list;
                    list.Add(ctx.Name);
                    ctx = ctx.next;
                }
            }
        }

        public Dictionary<string, IChannelHandler> ToDictionary()
        {
            var dict = new Dictionary<string, IChannelHandler>();
            var ctx = Head.next;
            for (;;)
            {
                if (ctx == Tail) return dict;
                dict.Add(ctx.Name, ctx.Handler);
                ctx = ctx.next;
            }
        }

        public IChannel Channel { get { return _channel; } }

        #region IEnumerable<ChannelHandlerAssociation> members

        public IEnumerator<ChannelHandlerAssociation> GetEnumerator()
        {
            return ToDictionary().Select(x => new ChannelHandlerAssociation(x.Key, x.Value)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        #endregion

        #region IChannel Events

        public IChannelPipeline FireChannelRegistered()
        {
            Head.FireChannelRegistered();
            return this;
        }

        public IChannelPipeline FireChannelActive()
        {
            Head.FireChannelActive();

            if (Channel.Config.IsAutoRead)
            {
                Channel.Read();
            }
            return this;
        }

        public IChannelPipeline FireChannelInactive()
        {
            Head.FireChannelInactive();
            TeardownAll();
            return this;
        }

        public IChannelPipeline FireExceptionCaught(Exception ex)
        {
            Head.FireExceptionCaught(ex);
            return this;
        }

        public IChannelPipeline FireChannelRead(NetworkData message)
        {
            Head.FireChannelRead(message);
            return this;
        }

        public IChannelPipeline FireUserEventTriggered(object evt)
        {
            Head.FireUserEventTriggered(evt);
            return this;
        }

        public IChannelPipeline FireChannelReadComplete()
        {
            Head.FireChannelReadComplete();
            if (Channel.Config.IsAutoRead) { Read(); }
            return this;
        }

        public IChannelPipeline FireChannelWritabilityChanged()
        {
            Head.FireChannelWritabilityChanged();
            return this;
        }

        public ChannelFuture<bool> Bind(INode localAddress)
        {
            return Tail.Bind(localAddress);
        }

        public ChannelFuture<bool> Bind(INode localAddress, ChannelPromise<bool> bindCompletionSource)
        {
            return Tail.Bind(localAddress, bindCompletionSource);
        }

        public ChannelFuture<bool> Connect(INode remoteAddress)
        {
            return Tail.Connect(remoteAddress);
        }

        public ChannelFuture<bool> Connect(INode remoteAddress, INode localAddress)
        {
            return Tail.Connect(remoteAddress, localAddress);
        }

        public ChannelFuture<bool> Connect(INode remoteAddress, ChannelPromise<bool> connectCompletionSource)
        {
            return Tail.Connect(remoteAddress, connectCompletionSource);
        }

        public ChannelFuture<bool> Connect(INode remoteAddress, INode localAddress, ChannelPromise<bool> connectCompletionSource)
        {
            return Tail.Connect(remoteAddress, localAddress, connectCompletionSource);
        }

        public ChannelFuture<bool> Disconnect()
        {
            return Tail.Disconnect();
        }

        public ChannelFuture<bool> Disconnect(ChannelPromise<bool> disconnectCompletionSource)
        {
            return Tail.Disconnect(disconnectCompletionSource);
        }

        public ChannelFuture<bool> Close()
        {
            return Tail.Close();
        }

        public ChannelFuture<bool> Close(ChannelPromise<bool> closeCompletionSource)
        {
            return Tail.Close(closeCompletionSource);
        }

        public IChannelPipeline Read()
        {
            Tail.Read();
            return this;
        }

        public ChannelFuture<bool> Write(NetworkData message)
        {
            return Tail.Write(message);
        }

        public ChannelFuture<bool> Write(NetworkData message, ChannelPromise<bool> writeCompletionSource)
        {
            return Tail.Write(message, writeCompletionSource);
        }

        public IChannelPipeline Flush()
        {
            Tail.Flush();
            return this;
        }

        public ChannelFuture<bool> WriteAndFlush(NetworkData message, ChannelPromise<bool> writeCompletionSource)
        {
            return Tail.WriteAndFlush(message, writeCompletionSource);
        }

        public ChannelFuture<bool> WriteAndFlush(NetworkData message)
        {
            return Tail.WriteAndFlush(message);
        }

        #endregion

        #region Internal class definitions

        /// <summary>
        /// Default <see cref="IChannelHandler"/> that sits at the front of the <see cref="IChannelPipeline"/>
        /// </summary>
        sealed class HeadHandler : ChannelHandlerAdapter
        {
            private readonly IUnsafe _unsafe;

            public HeadHandler(IUnsafe @unsafe)
            {
                _unsafe = @unsafe;
            }

            public override void Bind(IChannelHandlerContext handlerContext, INode localAddress, ChannelPromise<bool> bindCompletionSource)
            {
                _unsafe.Bind(localAddress, bindCompletionSource);
            }

            public override void Connect(IChannelHandlerContext handlerContext, INode remoteAddress, INode localAddress, ChannelPromise<bool> connectCompletionSource)
            {
                _unsafe.Connect(remoteAddress, localAddress, connectCompletionSource);
            }

            public override void Disconnect(IChannelHandlerContext handlerContext, ChannelPromise<bool> disconnectCompletionSource)
            {
                _unsafe.Disconnect(disconnectCompletionSource);
            }

            public override void Close(IChannelHandlerContext handlerContext, ChannelPromise<bool> closeCompletionSource)
            {
                _unsafe.Close(closeCompletionSource);
            }

            public override void Read(IChannelHandlerContext handlerContext)
            {
                _unsafe.BeginRead();
            }

            public override void Write(IChannelHandlerContext handlerContext, NetworkData message, ChannelPromise<bool> writeCompletionSource)
            {
                _unsafe.Write(message, writeCompletionSource);
            }

            public override void Flush(IChannelHandlerContext handlerContext)
            {
                _unsafe.Flush();
            }
        }

        /// <summary>
        /// Default <see cref="IChannelHandler"/> that sits at the end of the <see cref="IChannelPipeline"/>
        /// </summary>
        sealed class TailHandler : ChannelHandlerAdapter
        {
            public override void ChannelRegistered(IChannelHandlerContext handlerContext)
            {
                //NO-OP
            }

            public override void ChannelActive(IChannelHandlerContext handlerContext)
            {
                //NO-OP
            }

            public override void ChannelInactive(IChannelHandlerContext handlerContext)
            {
                //NO-OP
            }

            public override void ChannelWritabilityChanged(IChannelHandlerContext handlerContext)
            {
                //NO-OP
            }

            public override void UserEventTriggered(IChannelHandlerContext handlerContext, object evt)
            {
                //NO-OP
            }

            public override void ExceptionCaught(IChannelHandlerContext handlerContext, Exception ex)
            {
                //NO-OP
            }

            public override void ChannelRead(IChannelHandlerContext handlerContext, NetworkData message)
            {
                //NO-OP
            }

            public override void ChannelReadComplete(IChannelHandlerContext handlerContext)
            {
                //NO-OP
            }
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Removes all handlers from the pipeline one by one from tail to head to trigger <see cref="IChannelHandler.HandlerRemoved"/>.
        /// </summary>
        private void TeardownAll()
        {
            Tail.prev.Teardown();
        }

        private DefaultChannelHandlerContext GetContextOrDie(string name)
        {
            var ctx = (DefaultChannelHandlerContext) Context(name);
            if(ctx == null)
                throw new KeyNotFoundException(name);
            return ctx;
        }

        private DefaultChannelHandlerContext GetContextOrDie(IChannelHandler handler)
        {
            var ctx = (DefaultChannelHandlerContext) Context(handler);
            if (ctx == null)
                throw new KeyNotFoundException(handler.GetType().FullName);
            return ctx;
        }

        private DefaultChannelHandlerContext GetContextOrDie<T>() where T : IChannelHandler
        {
            var ctx = (DefaultChannelHandlerContext)Context<T>();
            if (ctx == null)
                throw new KeyNotFoundException(typeof(T).FullName);
            return ctx;
        }

        private void CheckDuplicateName(string name)
        {
            if(_name2Ctx.ContainsKey(name)) throw new ArgumentException(string.Format("Duplicate handler name {0}.", name),"name");
        }

        private IChannelHandlerInvoker FindInvoker(IEventLoop loop)
        {
            if (loop == null) return null;

            // Pin one of the child executors once and remember it so that the same child executor is used
            // to fire events for the same channel
            IChannelHandlerInvoker invoker;
            if (!_childInvokers.TryGetValue(loop, out invoker))
            {
                var executor = loop.Next();
                invoker = new DefaultChannelHandlerInvoker(executor);
                _childInvokers.Add(loop, invoker);
            }

            return invoker;
        }

        private static void CheckMultiplicity(IChannelHandlerContext ctx)
        {
            var handler = ctx.Handler;
            var adapter = handler as ChannelHandlerAdapter;
            if (adapter != null)
            {
                var h = adapter;
                if (h.Added) //this adapter has already been added
                {
                    throw new HeliosChannelPipelineException(string.Format("Can't add handler {0} to pipeline multiple times", ctx.GetType()));
                }
                h.Added = true;
            }
        }

        private void CallHandlerAdded(DefaultChannelHandlerContext ctx)
        {
            if (ctx.Channel.IsRegistered && !ctx.Executor.IsInEventLoop())
            {
                ctx.Executor.Execute(() => CallHandlerAddedInternal(ctx));
                return;
            }

            CallHandlerAddedInternal(ctx);
        }

        private void CallHandlerAddedInternal(IChannelHandlerContext ctx)
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
                    Remove(ctx.Handler);
                    removed = true;
                }
                catch (Exception ex2)
                {
                    Debug.Write(string.Format("Failed to remove handler {0} {1}", ctx.Name, ex2));
                }

                if (removed)
                {
                    FireExceptionCaught(
                        new HeliosChannelPipelineException(
                            string.Format("{0}.HandlerAdded has thrown an exception; removed.", ctx.Handler.GetType()),
                            ex));
                }
                else
                {
                    FireExceptionCaught(
                        new HeliosChannelPipelineException(
                            string.Format("{0}.HandlerAdded has thrown an exception; also failed to remove.", ctx.Handler.GetType()),
                            ex));
                }
            }
        }

        private void CallHandlerRemoved(DefaultChannelHandlerContext ctx)
        {
            if (ctx.Channel.IsRegistered && !ctx.Executor.IsInEventLoop())
            {
                ctx.Executor.Execute(() => CallHandlerRemovedInternal(ctx));
                return;
            }

            CallHandlerRemovedInternal(ctx);
        }

        private void CallHandlerRemovedInternal(DefaultChannelHandlerContext ctx)
        {
            try
            {
                ctx.Handler.HandlerRemoved(ctx);
                ctx.SetRemoved();
            }
            catch (Exception ex)
            {
                FireExceptionCaught(
                    new HeliosChannelPipelineException(
                        string.Format("{0}.HandlerRemoved has thrown an exception.", ctx.Handler.GetType()), ex));
            }
        }

        internal string GenerateName(IChannelHandler handler)
        {
            var handlerType = handler.GetType();
            string name;
            if (nameCache.ContainsKey(handlerType)) name = nameCache[handlerType];
            else
            {
                lock (NameCacheLock)
                {
                    //double-check the lock
                    if (nameCache.ContainsKey(handlerType))
                    {
                        name = nameCache[handlerType];
                    }
                    else
                    {
                        name = string.Format("{0}#0", handlerType);
                        nameCache.Add(handlerType, name);
                    }
                }
            }

            lock (_nameCtxLock)
            {
                //It's unlikely for a user to put more than one handler of the same type, but make sure to avoid
                // any name conflicts.
                if (_name2Ctx.ContainsKey(name))
                {
                    var baseName = name.Substring(0, name.Length - 1); //Strip the trailing '0'
                    for (var i = 0; ; i++)
                    {
                        var newName = baseName + i;
                        if (!_name2Ctx.ContainsKey(newName))
                        {
                            name = newName;
                            break;
                        }
                    }
                }
            }

            return name;
        }

        #endregion
    }
}