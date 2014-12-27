using Helios.Net;
using Helios.Ops;

namespace Helios.Channel
{
    /// <summary>
    /// Represents a duplex connection over an <see cref="IConnection"/>.
    /// 
    /// Contains a <see cref="IChannelPipeline"/> consisting of one or more <see cref="IChannelHandler"/>s, used to process
    /// inbound reads / accepts and outbound writes / connects.
    /// </summary>
    public interface IChannel
    {
        /// <summary>
        /// <see cref="IEventLoop"/> used for processing <see cref="IChannelPipeline"/> operations on this <see cref="IChannel"/>.
        /// </summary>
        IEventLoop EventLoop { get; }

        /// <summary>
        /// The pipeline of <see cref="IChannelHandler"/> instances used to process inbound and outbound messages on this <see cref="IChannel"/>
        /// </summary>
        IChannelPipeline Pipeline { get; }
    }
}
