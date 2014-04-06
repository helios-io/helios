using System;
using System.Threading.Tasks;
using Helios.Channels.Extensions;
using Helios.Net;
using Helios.Ops;
using Helios.Topology;

namespace Helios.Channels.Impl
{
    public class DefaultChannelHandlerContext : IChannelHandlerContext
    {
        private readonly AbstractChannel _channel;
        private readonly DefaultChannelPipeline _channelPipeline;
        private readonly string _name;
        private readonly IChannelHandler _channelHandler;

        internal volatile DefaultChannelHandlerContext next;
        internal volatile DefaultChannelHandlerContext prev;

        // Will be set to null if no child executor, otherwise it will be set to the child executor
        private readonly IChannelHandlerInvoker _invoker;
        private object m_lock = new object();

        public DefaultChannelHandlerContext(DefaultChannelPipeline channelPipeline, IChannelHandlerInvoker invoker, string name, IChannelHandler channelHandler)
        {
            if(string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if(channelHandler == null) throw new ArgumentNullException("channelHandler");

            _channelPipeline = channelPipeline;
            _name = name;
            _channelHandler = channelHandler;
            _channel = (AbstractChannel)_channelPipeline.Channel;

            _invoker = invoker ?? _channel.Unsafe.Invoker;
        }

        internal volatile Task invokeChannelReadCompleteTask;
        internal volatile Task invokeReadTask;
        internal volatile Task invokeFlushTask;
        internal volatile Task invokeChannelWritableStateChangedTask;

        public IChannel Channel { get { return _channel; } }
        public IExecutor Executor { get { return _invoker.Executor; } }
        public IChannelHandlerInvoker Invoker { get { return _invoker; } }
        public IChannelHandler Handler { get { return _channelHandler; } }
        public string Name { get { return _name; } }
        public bool IsRemoved { get; protected set; }

        public void Teardown()
        {
            var executor = Executor;
            if (executor.IsInEventLoop())
            {
                TeardownInternal();
            }
            else
            {
                executor.Execute(TeardownInternal);
            }
        }

        private void TeardownInternal()
        {
            var nextPrev = prev;
            if (prev != null)
            {
                lock (m_lock)
                {
                    _channelPipeline.RemoveInternal(this);
                }
                nextPrev.Teardown();
            }
        }

        private DefaultChannelHandlerContext FindContextInbound()
        {
            var ctx = this;
            return ctx.next;
        }

        private DefaultChannelHandlerContext FindContextOutbound()
        {
            var ctx = this;
            return ctx.prev;
        }

        #region IChannelHandlerContext methods

        public IChannelHandlerContext FireChannelRegistered()
        {
            var nextContext = FindContextInbound();
            nextContext.Invoker.InvokeChannelRegistered(nextContext);
            return this;
        }

        public IChannelHandlerContext FireChannelActive()
        {
            var nextContext = FindContextInbound();
            nextContext.Invoker.InvokeChannelActive(nextContext);
            return this;
        }

        public IChannelHandlerContext FireChannelInactive()
        {
            var nextContext = FindContextInbound();
            nextContext.Invoker.InvokeChannelInactive(nextContext);
            return this;
        }

        public IChannelHandlerContext FireExceptionCaught(Exception cause)
        {
            var nextContext = FindContextInbound();
            nextContext.Invoker.InvokeExceptionCaught(nextContext, cause);
            return this;
        }

        public IChannelHandlerContext FireChannelRead(NetworkData message)
        {
            var nextContext = FindContextInbound();
            nextContext.Invoker.InvokeChannelRead(nextContext, message);
            return this;
        }

        public IChannelHandlerContext FireUserEventTriggered(object evt)
        {
            var nextContext = FindContextInbound();
            nextContext.Invoker.InvokeUserEventTriggered(nextContext, evt);
            return this;
        }

        public IChannelHandlerContext FireChannelWritabilityChanged()
        {
            var nextContext = FindContextInbound();
            nextContext.Invoker.InvokeChannelWritabilityChanged(nextContext);
            return this;
        }

        public IChannelHandlerContext FireChannelReadComplete()
        {
            var nextContext = FindContextInbound();
            nextContext.Invoker.InvokeChannelReadComplete(nextContext);
            return this;
        }

        public Task<bool> Bind(INode localAddress)
        {
            return Bind(localAddress, NewCompletionSource());
        }

        public Task<bool> Bind(INode localAddress, TaskCompletionSource<bool> bindCompletionSource)
        {
            var nextContext = FindContextOutbound();
            nextContext.Invoker.InvokeBind(nextContext, localAddress, bindCompletionSource);
            return bindCompletionSource.Task;
        }

        public Task<bool> Connect(INode remoteAddress)
        {
            return Connect(remoteAddress, NewCompletionSource());
        }

        public Task<bool> Connect(INode remoteAddress, TaskCompletionSource<bool> connectionCompletionSource)
        {
            var nextContext = FindContextOutbound();
            nextContext.Invoker.InvokeConnect(nextContext, remoteAddress, null, connectionCompletionSource);
            return connectionCompletionSource.Task;
        }

        public Task<bool> Connect(INode remoteAddress, INode localAddress)
        {
            return Connect(remoteAddress, localAddress, NewCompletionSource());
        }

        public Task<bool> Connect(INode remoteAddress, INode localAddress, TaskCompletionSource<bool> connectCompletionSource)
        {
            var nextContext = FindContextOutbound();
            nextContext.Invoker.InvokeConnect(nextContext, remoteAddress, localAddress, connectCompletionSource);
            return connectCompletionSource.Task;
        }

        public Task<bool> Disconnect(TaskCompletionSource<bool> disconnectCompletionSource)
        {
            var nextContext = FindContextOutbound();
            nextContext.Invoker.InvokeDisconnect(nextContext, disconnectCompletionSource);
            return disconnectCompletionSource.Task;
        }

        public Task<bool> Disconnect()
        {
            return Disconnect(NewCompletionSource());
        }

        public Task<bool> Close()
        {
            return Close(NewCompletionSource());
        }

        public Task<bool> Close(TaskCompletionSource<bool> closeCompletionSource)
        {
            var nextContext = FindContextOutbound();
            nextContext.Invoker.InvokeClose(nextContext, closeCompletionSource);
            return closeCompletionSource.Task;
        }

        public IChannelHandlerContext Read()
        {
            var nextContext = FindContextOutbound();
            nextContext.Invoker.InvokeRead(nextContext);
            return this;
        }

        public Task<bool> Write(NetworkData message)
        {
            return Write(message, NewCompletionSource());
        }

        public Task<bool> Write(NetworkData message, TaskCompletionSource<bool> writeCompletionSource)
        {
            var nextContext = FindContextOutbound();
            nextContext.Invoker.InvokeWrite(nextContext, message, writeCompletionSource);
            return writeCompletionSource.Task;
        }

        public IChannelHandlerContext Flush()
        {
            var nextContext = FindContextOutbound();
            nextContext.Invoker.InvokeFlush(nextContext);
            return this;
        }

        public Task<bool> WriteAndFlush(NetworkData message, TaskCompletionSource<bool> writeCompletionSource)
        {
            var nextContext = FindContextOutbound();
            nextContext.Invoker.InvokeWrite(nextContext, message, writeCompletionSource);
            nextContext.Invoker.InvokeFlush(nextContext);
            return writeCompletionSource.Task;
        }

        public Task<bool> WriteAndFlush(NetworkData message)
        {
            return WriteAndFlush(message, NewCompletionSource());
        }

        public TaskCompletionSource<bool> NewCompletionSource()
        {
            return new TaskCompletionSource<bool>();
        }

        public Task<bool> NewSucceededTask()
        {
            var newSource = NewCompletionSource();
            newSource.TrySetResult(true);
            return newSource.Task;
        }

        public Task<bool> NewFailedTask()
        {
            var newSource = NewCompletionSource();
            newSource.TrySetResult(false);
            return newSource.Task;
        }

        #endregion
    }
}