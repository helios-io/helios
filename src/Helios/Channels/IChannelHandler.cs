using System;
using System.Threading.Tasks;
using Helios.Net;
using Helios.Topology;

namespace Helios.Channels
{
    /// <summary>
    /// Handles I/O events that arrive from a <see cref="IChannel"/> and forwards the request onto the next
    /// <see cref="IChannelHandler"/> in the chain.
    /// </summary>
    public interface IChannelHandler
    {
        #region Lifecycle methods

        /// <summary>
        /// Called immediately after the <see cref="IChannelHandler"/> was added to the underlying <see cref="IChannel"/> and is ready to handle events.
        /// </summary>
        /// <param name="context">Control context for this <see cref="IChannel"/></param>
        void HandlerAdded(IChannelContext context);

        /// <summary>
        /// Called when the <see cref="IChannelHandler"/> is removed from the underlying <see cref="IChannel"/>.
        /// </summary>
        /// <param name="context">Control context for this <see cref="IChannel"/></param>
        void HandlerRemoved(IChannelContext context);

        #endregion

        #region Inbound Event Handler Methods

        /// <summary>
        /// Called if an Exception is thrown by the underlying <see cref="IChannel"/>
        /// </summary>
        void ExceptionCaught(IChannelContext context, Exception ex);

        /// <summary>
        /// Invoked when the this <see cref="IChannelHandler"/> completes registration inside the <see cref="IChannel"/> event loop
        /// </summary>
        void ChannelRegistered(IChannelContext context);

        /// <summary>
        /// Invoked when the underlying <see cref="IChannel"/> becomes active
        /// </summary>
        void ChannelActive(IChannelContext context);

        /// <summary>
        /// The channel has now reached the end of its lifetime and is no longer active;
        /// can be due to the remote endpoint shutting down or a number of other reasons.
        /// </summary>
        void ChannelInactive(IChannelContext context);

        /// <summary>
        /// Received inbound data from the channel
        /// </summary>
        void ChannelRead(IChannelContext context, object message);

        /// <summary>
        /// Gets called when the writable state of the underlying <see cref="IChannel"/> has changed. You can get the
        /// state by calling <see cref="IChannel.IsWritable"/>.
        /// </summary>
        void ChannelWriteabilityChanged(IChannelContext context);

        #endregion

        #region Outbound Event Handler Methods

        /// <summary>
        /// Called once a bind operation is made to a local address. Typically used in combination with UDP.
        /// </summary>
        /// <param name="context">The context used to make the binding operation</param>
        /// <param name="localAddress">The local address to bind on</param>
        /// <param name="bindCompletionSource">A task completion source to notify once the operation finishes</param>
        void Bind(IChannelContext context, INode localAddress, TaskCompletionSource<bool> bindCompletionSource);

        /// <summary>
        /// Called once a connect operation is made from a local address to a remote one. Typically used in combination with TCP.
        /// </summary>
        /// <param name="context">The context used to make the connect operation.</param>
        /// <param name="remoteAddress">The remote address to connect to.</param>
        /// <param name="localAddress">The local address to connect with.</param>
        /// <param name="connectCompletionSource">A task completion source to notify once the operation finishes.</param>
        void Connect(IChannelContext context, INode remoteAddress, INode localAddress, TaskCompletionSource<bool> connectCompletionSource);

        /// <summary>
        /// Called once a disconnect operation is made. Typically used in combination with TCP.
        /// </summary>
        /// <param name="context">The context used to make the disconnect operation.</param>
        /// <param name="disconnectCompletionSource">A task completion source to notify once the operation finishes.</param>
        void Disconnect(IChannelContext context, TaskCompletionSource<bool> disconnectCompletionSource);

        /// <summary>
        /// Called once a close operation is made.
        /// </summary>
        /// <param name="context">The context used to make the close operation.</param>
        /// <param name="closeCompletionSource">A task completion source to notify once the operation finishes.</param>
        void Close(IChannelContext context, TaskCompletionSource<bool> closeCompletionSource);

        /// <summary>
        /// Called once a write operation is made. The write operation will write all messages directly onto the underlying
        /// <see cref="IChannel"/> and <see cref="IConnection"/> but may not be flushed to the network immediately.
        /// </summary>
        /// <param name="context">The context used to make the write operation.</param>
        /// <param name="message">The message being written by the <see cref="IChannelContext"/> to the <see cref="IChannel"/></param>
        /// <param name="writeCompletionSource">A task completion source to notify once the operation finishes.</param>
        void Write(IChannelContext context, object message, TaskCompletionSource<bool> writeCompletionSource);

        /// <summary>
        /// Called once a flush operation is made to the underlying <see cref="IConnection"/>. This operation tries to push any
        /// pending writes directly onto the network.
        /// </summary>
        /// <param name="context">The contect used to make the flush operation.</param>
        void Flush(IChannelContext context);

        #endregion
    }
}
