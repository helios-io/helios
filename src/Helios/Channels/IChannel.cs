using System;
using System.Threading.Tasks;
using Helios.Net;
using Helios.Ops;
using Helios.Topology;

namespace Helios.Channels
{
    /// <summary>
    /// Composable wrapper interface over a <see cref="IConnection"/> - designed to enable
    /// buffering of messages, asynchronous operations, and dispatch available socket data to multiple workers
    /// </summary>
    public interface IChannel : IComparable<IChannel>
    {
        /// <summary>
        /// The unique ID for this channel
        /// </summary>
        IChannelId Id { get; }

        /// <summary>
        /// The eventloop responsible for executing commands on this channel
        /// </summary>
        IEventLoop EventLoop { get; }

        /// <summary>
        /// The configuration for this channel
        /// </summary>
        IChannelConfig Config { get; }

        /// <summary>
        /// The pipeline of <see cref="IChannelHandler"/> instances responsible for handling this channel
        /// </summary>
        IChannelPipeline Pipeline { get; }

        /// <summary>
        /// Gets the parent <see cref="IChannel"/> responsible for this channel.
        /// 
        /// For instance, if this connection was created by an inbound TCP connnection, the Server channel
        /// responsible for creating this client connection is the Parent.
        /// </summary>
        IChannel Parent { get; }

        /// <summary>
        /// INTERNAL USE ONLY
        /// </summary>
        IUnsafe Unsafe { get; }

        /// <summary>
        /// If the channel is open and might be <see cref="IsActive"/> later
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// If this channel has successfully received or sent any data
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Returns true if this channel is registered with an <see cref="IEventLoop"/>
        /// </summary>
        bool IsRegistered { get; }

        /// <summary>
        /// The local address that this channel is bound to
        /// </summary>
        INode LocalAddress { get; }

        /// <summary>
        /// The remote address that this channel is bound to
        /// </summary>
        INode RemoteAddress { get; }

        /// <summary>
        /// Returns the Task which will be completed once this channel is closed. This method
        /// always returns the same Task.
        /// </summary>
        ChannelFuture CloseTask { get; }

        /// <summary>
        /// Returns true if the I/O thread will peform the requested write operation immediately.
        /// Any write requests made when this method returns false are queued until the I/O thread
        /// is ready to process the queued write requests.
        /// </summary>
        bool IsWritable { get; }

        ChannelFuture<bool> Bind(INode localAddress);

        ChannelFuture<bool> Bind(INode localAddress, ChannelPromise<bool> bindCompletionSource);

        ChannelFuture<bool> Connect(INode remoteAddress);

        ChannelFuture<bool> Connect(INode remoteAddress, ChannelPromise<bool> connectCompletionSource);

        ChannelFuture<bool> Connect(INode remoteAddress, INode localAddress);

        ChannelFuture<bool> Connect(INode remoteAddress, INode localAddress, ChannelPromise<bool> connectCompletionSource);

        ChannelFuture<bool> Disconnect();

        ChannelFuture<bool> Disconnect(ChannelPromise<bool> disconnectCompletionSource);

        ChannelFuture<bool> Close(ChannelPromise<bool> closeCompletionSource);

        IChannel Read();

        ChannelFuture<bool> Write(NetworkData message);

        ChannelFuture<bool> Write(NetworkData message, ChannelPromise<bool> writeCompletionSource);

        IChannel Flush();

        ChannelFuture<bool> WriteAndFlush(NetworkData message, ChannelPromise<bool> writeCompletionSource);

        ChannelFuture<bool> WriteAndFlush(NetworkData message);

        ChannelPromise<bool> NewPromise();

        ChannelFuture<bool> NewFailedFuture(Exception cause);

        ChannelFuture<bool> NewSucceededFuture();
            
        VoidChannelPromise VoidPromise();

    }


    #region Unsafe interface

    /// <summary>
    /// Unsafe operations that should never be called from user-code. Used to implement the actual underlying transport.
    /// </summary>
    public interface IUnsafe
    {
        IChannelHandlerInvoker Invoker
        {
            get;
        }

        INode LocalAddress { get; }

        INode RemoteAddress { get; }

        /// <summary>
        /// Register the <see cref="IChannel"/> of the <see cref="TaskCompletionSource{T}"/> and notify
        /// the <see cref="Task{T}"/> once the registration is complete.
        /// </summary>
        /// <param name="registerPromise"></param>
        void Register(ChannelPromise<bool> registerPromise);

        void Bind(INode localAddress, ChannelPromise<bool> bindCompletionSource);

        void Connect(INode remoteAddress, INode localAddress, ChannelPromise<bool> connectCompletionSource);

        void Disconnect(ChannelPromise<bool> disconnectCompletionSource);

        void Close(ChannelPromise<bool> closeCompletionSource);

        void CloseForcibly();

        void BeginRead();

        void Write(NetworkData msg, ChannelPromise<bool> writeCompletionSource);

        void Flush();

        ChannelOutboundBuffer OutboundBuffer { get; }

        VoidChannelPromise VoidPromise();
    }

    #endregion
}
