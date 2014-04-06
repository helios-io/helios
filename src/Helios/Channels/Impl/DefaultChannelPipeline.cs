using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Helios.Net;
using Helios.Topology;

namespace Helios.Channels.Impl
{
    public class DefaultChannelPipeline : IChannelPipeline
    {



        #region IChannelPipeline manipulation methods
        public IChannelPipeline AddFirst(string name, IChannelHandler handler)
        {
            throw new NotImplementedException();
        }

        public IChannelPipeline AddLast(string name, IChannelHandler handler)
        {
            throw new NotImplementedException();
        }

        public IChannelPipeline AddBefore(string baseName, string name, IChannelHandler handler)
        {
            throw new NotImplementedException();
        }

        public IChannelPipeline AddAfter(string baseName, string name, IChannelHandler handler)
        {
            throw new NotImplementedException();
        }

        public IChannelHandler Remove(string name)
        {
            throw new NotImplementedException();
        }

        public IChannelPipeline Remove(IChannelHandler handler)
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

        public IChannelHandler Replace(string oldName, string newName, IChannelHandler handler)
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

        public IChannelHandlerContext Context(IChannelHandler handler)
        {
            throw new NotImplementedException();
        }

        #endregion

        public List<string> Names { get; private set; }
        public Dictionary<string, IChannelHandler> ToDictionary()
        {
            throw new NotImplementedException();
        }

        public IChannel Channel { get; private set; }

        #region IEnumerable<ChannelHandlerAssociation> members

        public IEnumerator<ChannelHandlerAssociation> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        #endregion

        #region IChannel Events

        public IChannelPipeline FireChannelRegistered()
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

        public IChannelPipeline FireExceptionCaught(Exception ex)
        {
            throw new NotImplementedException();
        }

        public IChannelPipeline FireChannelRead(NetworkData message)
        {
            throw new NotImplementedException();
        }

        public IChannelPipeline FireUserEventTriggered(object evt)
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

        public Task<bool> Connect(INode remoteAddress, INode localAddress)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Connect(INode remoteAddress, TaskCompletionSource<bool> connectCompletionSource)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Connect(INode remoteAddress, INode localAddress, TaskCompletionSource<bool> connectCompletionSource)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Disconnect()
        {
            throw new NotImplementedException();
        }

        public Task<bool> Disconnect(TaskCompletionSource<bool> disconnectCompletionSource)
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

        public IChannelPipeline Read()
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

        public IChannelPipeline Flush()
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

            public override void Bind(IChannelHandlerContext handlerContext, INode localAddress, TaskCompletionSource<bool> bindCompletionSource)
            {
                _unsafe.Bind(localAddress, bindCompletionSource);
            }

            public override void Connect(IChannelHandlerContext handlerContext, INode remoteAddress, INode localAddress, TaskCompletionSource<bool> connectCompletionSource)
            {
                _unsafe.Connect(remoteAddress, localAddress, connectCompletionSource);
            }

            public override void Disconnect(IChannelHandlerContext handlerContext, TaskCompletionSource<bool> disconnectCompletionSource)
            {
                _unsafe.Disconnect(disconnectCompletionSource);
            }

            public override void Close(IChannelHandlerContext handlerContext, TaskCompletionSource<bool> closeCompletionSource)
            {
                _unsafe.Close(closeCompletionSource);
            }

            public override void Read(IChannelHandlerContext handlerContext)
            {
                _unsafe.BeginRead();
            }

            public override void Write(IChannelHandlerContext handlerContext, NetworkData message, TaskCompletionSource<bool> writeCompletionSource)
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

        internal void RemoveInternal(DefaultChannelHandlerContext defaultChannelHandlerContext)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}