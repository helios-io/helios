using System;
using Helios.Net;
using Helios.Ops;
using Helios.Topology;

namespace Helios.Channel
{
    /// <summary>
    /// Interface that is used to represent a Helios Channel - part of a duplex inbound / outbound
    /// communication pipeline over a given network socket.
    /// 
    /// Each pipeline consists of one or more channels, implemented via an <see cref="IChannelHandler"/> class.
    /// </summary>
    public interface IChannelHandler
    {
        #region IChannelHandler lifecycle methods

        /// <summary>
        /// Gets called after the <see cref="IChannelHandler"/> was added to the actual context and it's ready
        /// to handle events.
        /// </summary>
        void HandlerAdded(IChannelHandlerContext context);

        /// <summary>
        /// Gets called after the <see cref="IChannelHandler"/> is removed from the actual context and it 
        /// doesn't handle events anymore.
        /// </summary>
        void HandlerRemoved(IChannelHandlerContext context);

        #endregion

        #region Inbound event handlers

       /// <summary>
        /// Gets called if an <see cref="Exception"/> is thrown during the handling of a request.
        /// </summary>
        void ExceptionCaught(IChannelHandlerContext context, Exception cause);

        /// <summary>
        /// The <see cref="IChannel"/> of the <see cref="IChannelHandlerContext"/> was registered with its <see cref="IEventLoop"/>.
        /// </summary>
        void ChannelRegistered(IChannelHandlerContext context);

        /// <summary>
        /// The <see cref="IChannel"/> of the <see cref="IChannelHandlerContext"/> was unregistered from its <see cref="IEventLoop"/>.
        /// </summary>
        void ChannelUnregistered(IChannelHandlerContext context);

        /// <summary>
        /// The <see cref="IChannel"/> of the <see cref="IChannelHandlerContext"/> is now active.
        /// </summary>
        void ChannelActive(IChannelHandlerContext context);

        /// <summary>
        /// The <see cref="IChannel"/> of the <see cref="IChannelHandlerContext"/> is now inactive and has reached the end
        /// of its lifetime.
        /// </summary>
        void ChannelInactive(IChannelHandlerContext context);

        /// <summary>
        /// Invoked when the <see cref="IChannel"/> has received a network message from a peer.
        /// </summary>
        void ChannelRead(IChannelHandlerContext context, object message);

        /// <summary>
        /// Invoked when the last message read by the current read operation has been consumed by <see cref="ChannelRead(IChannelHandlerContext, object)"/>.
        /// If <see cref="ChannelConfig.AutoRead"/> is off, no further attempt to read an inboud data object form the current <see cref="IChannel"/> will be made
        /// until <see cref="IChannelHandlerContext.Read"/> is called.
        /// </summary>
        void ChannelReadComplete(IChannelHandlerContext context);

        /// <summary>
        /// Invoked when the application attempts to write to a given <see cref="IChannel"/>.
        /// </summary>
        void ChannelWrite(IChannelHandlerContext context, object message);

        /// <summary>
        /// Invoked when a user-defined event is rasied on the <see cref="IChannel"/>.
        /// </summary>
        void UserEventTriggered(IChannelHandlerContext context, object message);

        /// <summary>
        /// Invoked once the writability of the <see cref="IChannel"/> is changed. You can check this statu with <see cref="IChannel.IsWritable"/>.
        /// </summary>
        void ChannelWritabilityChanged(IChannelHandlerContext context);

        #endregion

        #region IO handlers

        /// <summary>
        /// Attempts to bind the <see cref="IChannel"/> and its underlying <see cref="IConnection"/> to the underlying <see cref="INode"/> address.
        /// 
        /// Throws a <see cref="HeliosException"/> if the <see cref="IChannel"/> or <see cref="IConnection"/> is already bound.
        /// </summary>
        /// <param name="context">The <see cref="IChannelHandlerContext"/> for the operation.</param>
        /// <param name="listenAddress">The address to which the <see cref="IConnection"/> will be bound.</param>
        /// <param name="promise">A promise that will be marked as complete once the bind operation finishes.</param>
        void Bind(IChannelHandlerContext context, INode listenAddress, ChannelPromise promise);

        /// <summary>
        /// Attempt to connect the current <see cref="IChannel"/> to a remote <see cref="INode"/> address.
        /// </summary>
        /// <param name="context">The <see cref="IChannelHandlerContext"/> for the operation.</param>
        /// <param name="remoteAddress">The address to which the <see cref="IConnection"/> will attempt to connect.</param>
        /// <param name="promise">A promise that will be marked as complete once the connect operation finishes.</param>
        void Connect(IChannelHandlerContext context, INode remoteAddress, ChannelPromise promise);

        /// <summary>
        /// Called once a disconnect operation is made.
        /// </summary>
        /// <param name="context">The <see cref="IChannelHandlerContext"/> for the operation.</param>
        /// <param name="promise">A promise that will be marked as complete once the disconnect operation finishes.</param>
        void Disconnect(IChannelHandlerContext context, ChannelPromise promise);

        /// <summary>
        /// Intercepts <see cref="IChannelHandlerContext.Read"/>
        /// </summary>
        void Read(IChannelHandlerContext context);

        /// <summary>
        /// Called once a write operation is made to the <see cref="IChannel"/>.
        /// </summary>
        /// <param name="context">The <see cref="IChannelHandlerContext"/> for the operation.</param>
        /// <param name="message">The datagram to be written to the underlying <see cref="IChannel"/> and <see cref="IConnection"/></param>
        /// <param name="promise">A waitable <see cref="ChannelPromise"/> that will be marked as completed once the write is executed.</param>
        void Write(IChannelHandlerContext context, object message, ChannelPromise promise);

        /// <summary>
        /// Called once a flush operation is made. Forces all pending outbound messages sitting inside the <see cref="IChannel"/>'s outbound
        /// buffer to be written to the underlying <see cref="IConnection"/>.
        /// </summary>
        void Flush(IChannelHandlerContext context);

        /// <summary>
        /// Called once a close operation is made.
        /// </summary>
        /// <param name="context">The <see cref="IChannelHandlerContext"/> for the operation.</param>
        /// <param name="promise">A promise that will be marked as complete once the close operation finishes.</param>
        void Close(IChannelHandlerContext context, ChannelPromise promise);



        #endregion
    }
}
