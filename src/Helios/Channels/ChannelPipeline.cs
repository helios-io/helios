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
    public abstract class ChannelPipeline : LinkedList<ChannelHandlerAssociation>
    {
        /// <summary>
        /// Returns all of the names associated with this <see cref="ChannelPipeline"/>
        /// </summary>
        public List<string> Names
        {
            get { return this.Select(x => x.Name).ToList(); }
        }

        public Dictionary<string, IChannelHandler> ToDictionary()
        {
            return this.ToDictionary(x => x.Name, y => y.Handler);
        }

        public IChannel Channel { get; protected set; }

        /// <summary>
        /// A <see cref="IChannel"/> was registered to its <see cref="IEventLoop"/>
        /// 
        /// This will result in having the <see cref="IChannelHandler.ChannelRegistered"/> method called of the next
        /// <see cref="IChannelHandler"/> contained in the <see cref="ChannelPipeline"/> of the <see cref="IChannel"/>
        /// </summary>
        public abstract ChannelPipeline FireChannelRegistered();

        /// <summary>
        /// A <see cref="IChannel"/> is active now, which means there's a connection available for reads / writes.
        /// 
        /// This will result in having the <see cref="IChannelHandler.ChannelActive"/> method called of the next
        /// <see cref="IChannelHandler"/> contained in the <see cref="ChannelPipeline"/> of the <see cref="IChannel"/>
        /// </summary>
        public abstract ChannelPipeline FireChannelActive();

        /// <summary>
        /// A <see cref="IChannel"/> is inactive now, which means it's closed.
        /// 
        /// This will result in having the <see cref="IChannelHandler.ChannelInactive"/> method called of the next
        /// <see cref="IChannelHandler"/> contained in the <see cref="ChannelPipeline"/> of the <see cref="IChannel"/>
        /// </summary>
        public abstract ChannelPipeline FireChannelInactive();

        public abstract ChannelPipeline FireExceptionCaught(Exception ex);

        public abstract ChannelPipeline FireChannelRead(NetworkData message);

        public abstract ChannelPipeline FireChannelWritabilityChanged();

        public abstract Task<bool> Bind(INode localAddress);

        public abstract Task<bool> Bind(INode localAddress, TaskCompletionSource<bool> bindCompletionSource);

        public abstract Task<bool> Connect(INode remoteAddress);

        public abstract Task<bool> Connect(INode remoteAddress, INode localAddress);

        public abstract Task<bool> Connect(INode remoteAddress, TaskCompletionSource<bool> connectCompletionSource);

        public abstract Task<bool> Connect(INode remoteAddress, INode localAddress,
            TaskCompletionSource<bool> connectCompletionSource);

        public abstract Task<bool> Disconnect();

        public abstract Task<bool> Disconnect(TaskCompletionSource<bool> disconnectCompletionSource);

        public abstract Task<bool> Close();

        public abstract Task<bool> Close(TaskCompletionSource<bool> closeCompletionSource);

        public abstract ChannelPipeline Read();

        public abstract Task<bool> Write(NetworkData message);

        public abstract Task<bool> Write(NetworkData message, TaskCompletionSource<bool> writeCompletionSource);

        public abstract ChannelPipeline Flush();

        public abstract Task<bool> WriteAndFlush(NetworkData message, TaskCompletionSource<bool> writeCompletionSource);

        public abstract Task<bool> WriteAndFlush(NetworkData message);
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
