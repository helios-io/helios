using System;
using System.Threading.Tasks;
using Helios.Net;
using Helios.Ops;
using Helios.Topology;

namespace Helios.Channels
{
    /// <summary>
    /// Exposes control and I/O capabilites to the methods implemented inside a <see cref="IChannelHandler"/>
    /// </summary>
    public interface IChannelHandlerContext
    {
        /// <summary>
        /// Exposes the underlying <see cref="IChannel"/> that is bound to this context.
        /// </summary>
        IChannel Channel { get; }

        /// <summary>
        /// Returns the <see cref="IExecutor"/> used to invoke operations which occur on the <see cref="IChannelHandler"/>.
        /// 
        /// <remarks>
        /// We strongly don't recommend calling this method unless you know what you're doing.
        /// </remarks>
        /// </summary>
        IExecutor Invoker { get; }

        /// <summary>
        /// Provides a link to the underlying <see cref="IChannelHandler"/> that is bound to this <see cref="IChannelHandlerContext"/>.
        /// </summary>
        IChannelHandler Handler { get; }

        /// <summary>
        /// The unique name of this <see cref="IChannelHandlerContext"/>.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// A <see cref="IChannel"/> was regiestered to its event loop.
        /// 
        /// This will result in the <see cref="IChannelHandler.ChannelRegistered"/> method being called
        /// </summary>
        IChannelHandlerContext FireChannelRegistered();

        /// <summary>
        /// A <see cref="IChannel"/> is active now, which means it is connected.
        /// 
        /// This will result in the <see cref="IChannelHandler.ChannelActive"/> method being called.
        /// </summary>
        IChannelHandlerContext FireChannelActive();

        /// <summary>
        /// A <see cref="IChannel"/> is inactive now, which means it is closed.
        /// 
        /// This will result in the <see cref="IChannelHandler.ChannelInactive"/> method being called.
        /// </summary>
        IChannelHandlerContext FireChannelInactive();

        /// <summary>
        /// A <see cref="IChannel"/> received an <see cref="Exception"/> in one of its inbound operations.
        /// 
        /// This will result in having the <see cref="IChannelHandler.ExceptionCaught"/> called.
        /// </summary>
        IChannelHandlerContext FireExceptionCaught(Exception cause);

        /// <summary>
        /// A <see cref="IChannel"/> received a message.
        /// 
        /// This will result in having the <see cref="IChannelHandler.ChannelRead"/> method called.
        /// </summary>
        IChannelHandlerContext FireChannelRead(NetworkData message);

        /// <summary>
        /// Triggers a <see cref="IChannelHandler.ChannelWritabilityChanged"/> event on the <see cref="IChannelHandler"/>.
        /// </summary>
        /// <returns></returns>
        IChannelHandlerContext FireChannelWritabilityChanged();

        /// <summary>
        /// Request to bind to a given <see cref="INode"/> address and notify the <see cref="Task{T}"/> once
        /// the operation completes, either because the operation was successful or because of an error.
        /// <remarks>
        /// This will result in having the <see cref="IChannelHandler.Bind"/> method called.
        /// </remarks>
        /// </summary>
        Task<bool> Bind(INode localAddress);

        Task<bool> Bind(INode localAddress, TaskCompletionSource<bool> bindCompletionSource);

        Task<bool> Connect(INode remoteAddress);

        Task<bool> Connect(INode remoteAddress, TaskCompletionSource<bool> connectionCompletionSource);

        Task<bool> Connect(INode remoteAddress, INode localAddress);

        Task<bool> Connect(INode remoteAddress, INode localAddress, TaskCompletionSource<bool> connectCompletionSource);

        Task<bool> Disconnect(TaskCompletionSource<bool> disconnectCompletionSource);

        Task<bool> Disconnection(TaskCompletionSource<bool> disconnectCompletionSource);

        Task<bool> Close();

        Task<bool> Close(TaskCompletionSource<bool> closeCompletionSource);

        IChannelHandlerContext Read();

        Task<bool> Write(NetworkData message);

        Task<bool> Write(NetworkData message, TaskCompletionSource<bool> writeCompletionSource);

        IChannelHandlerContext Flush();

        Task<bool> WriteAndFlush(NetworkData message, TaskCompletionSource<bool> writeCompletionSource);

        Task<bool> WriteAndFlush(NetworkData message);

        TaskCompletionSource<bool> NewCompletionSource();

        Task<bool> NewSucceededTask();

        Task<bool> NewFailedTask();
    }
}
