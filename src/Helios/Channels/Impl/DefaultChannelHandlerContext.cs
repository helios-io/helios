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

        internal volatile Task<bool> invokeChannelReadCompleteTask;
        internal volatile Task<bool> invokeReadTask;
        internal volatile Task<bool> invokeFlushTask;
        internal volatile Task<bool> invokeChannelWritableStateChangedTask;

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
            throw new NotImplementedException();
        }

        public IChannelHandlerContext FireChannelActive()
        {
            throw new NotImplementedException();
        }

        public IChannelHandlerContext FireChannelInactive()
        {
            throw new NotImplementedException();
        }

        public IChannelHandlerContext FireExceptionCaught(Exception cause)
        {
            throw new NotImplementedException();
        }

        public IChannelHandlerContext FireChannelRead(NetworkData message)
        {
            throw new NotImplementedException();
        }

        public IChannelHandlerContext FireUserEventTriggered(object evt)
        {
            throw new NotImplementedException();
        }

        public IChannelHandlerContext FireChannelWritabilityChanged()
        {
            throw new NotImplementedException();
        }

        public IChannelHandlerContext FireChannelReadComplete()
        {
            throw new NotImplementedException();
        }

        public Task<bool> Bind(INode localAddress)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Bind(INode localAddress, TaskCompletionSource<bool> bindCompletionSource)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Connect(INode remoteAddress)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Connect(INode remoteAddress, TaskCompletionSource<bool> connectionCompletionSource)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Connect(INode remoteAddress, INode localAddress)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Connect(INode remoteAddress, INode localAddress, TaskCompletionSource<bool> connectCompletionSource)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Disconnect(TaskCompletionSource<bool> disconnectCompletionSource)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Disconnection(TaskCompletionSource<bool> disconnectCompletionSource)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Close()
        {
            throw new NotImplementedException();
        }

        public Task<bool> Close(TaskCompletionSource<bool> closeCompletionSource)
        {
            throw new NotImplementedException();
        }

        public IChannelHandlerContext Read()
        {
            throw new NotImplementedException();
        }

        public Task<bool> Write(NetworkData message)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Write(NetworkData message, TaskCompletionSource<bool> writeCompletionSource)
        {
            throw new NotImplementedException();
        }

        public IChannelHandlerContext Flush()
        {
            throw new NotImplementedException();
        }

        public Task<bool> WriteAndFlush(NetworkData message, TaskCompletionSource<bool> writeCompletionSource)
        {
            throw new NotImplementedException();
        }

        public Task<bool> WriteAndFlush(NetworkData message)
        {
            throw new NotImplementedException();
        }

        public TaskCompletionSource<bool> NewCompletionSource()
        {
            throw new NotImplementedException();
        }

        public Task<bool> NewSucceededTask()
        {
            throw new NotImplementedException();
        }

        public Task<bool> NewFailedTask()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}