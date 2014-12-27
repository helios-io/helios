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
        /// Signal to the next <see cref="IChannelHandler"/> in the <see cref="IChannelPipeline"/> that there's data to be read
        /// in the processing of this request.
        /// </summary>
        /// <param name="message">The data to be read by the next <see cref="IChannelHandler"/>.</param>
        /// <returns>An updated <see cref="IChannelHandlerContext"/>.</returns>
        IChannelHandlerContext FireChannelRead(object message);

        /// <summary>
        /// Signal to the next <see cref="IChannelHandler"/> in the <see cref="IChannelPipeline"/> that a new user-defined
        /// event has occurred. Built-in <see cref="IChannelHandler"/> instances typically do not handle this event.
        /// </summary>
        /// <param name="message">The data to be read by the next <see cref="IChannelHandler"/>.</param>
        /// <returns>An updated <see cref="IChannelHandlerContext"/>.</returns>
        IChannelHandlerContext FireUserEvent(object message);

        #endregion
    }
}
