using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Helios.Net;
using Helios.Ops;
using Helios.Topology;

namespace Helios.Channels
{
    /// <summary>
    /// Defines a chained, ordered pipeline for being able to process messages
    /// </summary>
    public interface IChannelPipeline : IEnumerable<ChannelHandlerAssociation>
    {
        IChannelPipeline AddFirst(string name, IChannelHandler handler);

        IChannelPipeline AddFirst(IChannelHandlerInvoker invoker, string name, IChannelHandler handler);

        IChannelPipeline AddFirst(IEventLoop loop, string name, IChannelHandler handler);

        IChannelPipeline AddLast(string name, IChannelHandler handler);

        IChannelPipeline AddLast(IEventLoop loop, string name, IChannelHandler handler);

        IChannelPipeline AddLast(IChannelHandlerInvoker invoker, string name, IChannelHandler handler);

        IChannelPipeline AddBefore(string baseName, string name, IChannelHandler handler);

        IChannelPipeline AddBefore(IEventLoop loop, string baseName, string name, IChannelHandler handler);

        IChannelPipeline AddBefore(IChannelHandlerInvoker invoker, string baseName, string name, IChannelHandler handler);

        IChannelPipeline AddAfter(string baseName, string name, IChannelHandler handler);

        IChannelPipeline AddAfter(IEventLoop loop, string baseName, string name, IChannelHandler handler);

        IChannelPipeline AddAfter(IChannelHandlerInvoker invoker, string baseName, string name, IChannelHandler handler);

        IChannelHandler Remove(string name);

        IChannelPipeline Remove(IChannelHandler handler);

        IChannelPipeline Remove<T>() where T : IChannelHandler;

        IChannelHandler RemoveFirst();

        IChannelHandler RemoveLast();

        IChannelPipeline Replace(IChannelHandler oldHandler, string newName, IChannelHandler newHandler);

        IChannelHandler Replace(string oldName, string newName, IChannelHandler newHandler);

        IChannelHandler Replace<T>(string newName, IChannelHandler newHandler) where T : IChannelHandler;

        IChannelHandler First();

        IChannelHandlerContext FirstContext();

        IChannelHandler Last();

        IChannelHandlerContext LastContext();

        IChannelHandler Get(string name);

        IChannelHandler Get<T>() where T : IChannelHandler;

        IChannelHandlerContext Context(IChannelHandler handler);

        IChannelHandlerContext Context(string name);

        IChannelHandlerContext Context<T>() where T : IChannelHandler;

        /// <summary>
        /// Returns all of the names associated with this <see cref="IChannelPipeline"/>
        /// </summary>
        List<string> Names { get; }

        Dictionary<string, IChannelHandler> ToDictionary();

        IChannel Channel { get; }

        /// <summary>
        /// A <see cref="IChannel"/> was registered to its <see cref="IEventLoop"/>
        /// 
        /// This will result in having the <see cref="IChannelHandler.ChannelRegistered"/> method called of the next
        /// <see cref="IChannelHandler"/> contained in the <see cref="IChannelPipeline"/> of the <see cref="IChannel"/>
        /// </summary>
        IChannelPipeline FireChannelRegistered();

        /// <summary>
        /// A <see cref="IChannel"/> is active now, which means there's a connection available for reads / writes.
        /// 
        /// This will result in having the <see cref="IChannelHandler.ChannelActive"/> method called of the next
        /// <see cref="IChannelHandler"/> contained in the <see cref="IChannelPipeline"/> of the <see cref="IChannel"/>
        /// </summary>
       IChannelPipeline FireChannelActive();

        /// <summary>
        /// A <see cref="IChannel"/> is inactive now, which means it's closed.
        /// 
        /// This will result in having the <see cref="IChannelHandler.ChannelInactive"/> method called of the next
        /// <see cref="IChannelHandler"/> contained in the <see cref="IChannelPipeline"/> of the <see cref="IChannel"/>
        /// </summary>
        IChannelPipeline FireChannelInactive();

        IChannelPipeline FireExceptionCaught(Exception ex);

        IChannelPipeline FireChannelRead(NetworkData message);

        IChannelPipeline FireUserEventTriggered(object evt);

        IChannelPipeline FireChannelReadComplete();

        IChannelPipeline FireChannelWritabilityChanged();

        Task<bool> Bind(INode localAddress);

        Task<bool> Bind(INode localAddress, TaskCompletionSource<bool> bindCompletionSource);

        Task<bool> Connect(INode remoteAddress);

        Task<bool> Connect(INode remoteAddress, INode localAddress);

        Task<bool> Connect(INode remoteAddress, TaskCompletionSource<bool> connectCompletionSource);

        Task<bool> Connect(INode remoteAddress, INode localAddress,
            TaskCompletionSource<bool> connectCompletionSource);

        Task<bool> Disconnect();

        Task<bool> Disconnect(TaskCompletionSource<bool> disconnectCompletionSource);

        Task<bool> Close();

        Task<bool> Close(TaskCompletionSource<bool> closeCompletionSource);

        IChannelPipeline Read();

        Task<bool> Write(NetworkData message);

        Task<bool> Write(NetworkData message, TaskCompletionSource<bool> writeCompletionSource);

        IChannelPipeline Flush();

        Task<bool> WriteAndFlush(NetworkData message, TaskCompletionSource<bool> writeCompletionSource);

        Task<bool> WriteAndFlush(NetworkData message);
    }

    /// <summary>
    /// Key / Value pair class for holding onto ordered channel pipelines
    /// </summary>
    public class ChannelHandlerAssociation
    {
        public ChannelHandlerAssociation(string name, IChannelHandler handler)
        {
            Handler = handler;
            Name = name;
        }

        public string Name { get; private set; }

        public IChannelHandler Handler { get; private set; }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + (Name == null ? 0 : Name.GetHashCode());
                hash = hash * 23 + (Handler == null ? 0 : Handler.GetHashCode());
                return hash;
            }
        }
    }
}
