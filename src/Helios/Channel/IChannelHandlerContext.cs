namespace Helios.Channel
{
    /// <summary>
    /// Maintains context for a <see cref="IChannelHandler"/> in the course of processing requests
    /// within the scope of its <see cref="IChannelPipeline"/>.
    /// 
    /// Provides event invokers to direct the execution of subsequent steps in the <see cref="IChannelPipeline"/> and signal 
    /// what needs to be done next in the course of fulflling a request.
    /// 
    /// ChannelHandlerContexts are ephemeral and are specific to each <see cref="IChannelHandler"/> in the <see cref="IChannelPipeline"/>
    /// for the processing of each read or write on the channel.
    /// </summary>
    public interface IChannelHandlerContext
    {
        #region Properties

        /// <summary>
        /// Reference to the current <see cref="IChannel"/>
        /// </summary>
        IChannel Channel { get; }

        /// <summary>
        /// Reference to the current <see cref="IChannelPipeline"/> for this <see cref="IChannel"/>
        /// </summary>
        IChannelPipeline Pipeline { get; }

        #endregion

        #region Event invokers - used to propagate network events to the next IChannelHandler in the IChannelPipeline

        /// <summary>
        /// Invoke the <see cref="IChannelHandler.ChannelRead(IChannelHandlerContext, object)"/> method of the the next 
        /// <see cref="IChannelHandler"/> in the <see cref="IChannelPipeline"/>.
        /// </summary>
        /// <param name="message">The data to be read by the next <see cref="IChannelHandler"/>.</param>
        IChannelHandlerContext FireChannelRead(object message);

        /// <summary>
        /// Invoke the <see cref="IChannelHandler.UserEventTriggered(IChannelHandlerContext, object)"/> method of the the next 
        /// <see cref="IChannelHandler"/> in the <see cref="IChannelPipeline"/>.
        /// 
        /// Built-in <see cref="IChannelHandler"/> instances typically do not handle this event, given that it deals exclusively
        /// with user-defined events.
        /// </summary>
        /// <param name="message">The data to be read by the next <see cref="IChannelHandler"/>.</param>
        IChannelHandlerContext FireUserEvent(object message);

        #endregion

        #region Transport methods

        /// <summary>
        /// Request to read data from the <see cref="IChannel"/> into memory.
        /// 
        /// Triggers an <see cref="IChannelHandler.ChannelRead(IChannelHandlerContext, object)"/> event if data was read
        /// from the underlying <see cref="IChannel"/> and triggers a <see cref="IChannelHandler.ChannelReadComplete(IChannelHandlerContext)"/> so
        /// the handler can decide to continue reading. If there's a pending read operation already, this method does nothing.
        /// 
        /// This will result in having the <see cref="IChannelHandler.Read(IChannelHandlerContext)"/> method of the next <see cref="IChannelHandler"/> in the 
        /// <see cref="IChannelPipeline"/> of the <see cref="IChannel"/> called.
        /// </summary>
        IChannelHandlerContext Read();

        #endregion
    }
}
