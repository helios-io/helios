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
        #region Event handlers

        /// <summary>
        /// Invoked when the <see cref="IChannel"/> has received a network message from a peer.
        /// </summary>
        /// <param name="context">The context of the given <see cref="IChannel"/>.</param>
        /// <param name="message">The message from the network.</param>
        void ChannelRead(IChannelHandlerContext context, object message);

        void ChannelWrite(IChannelHandlerContext context, object message);

        #endregion
    }
}
