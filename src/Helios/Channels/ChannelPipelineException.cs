using System;

namespace Helios.Channels
{
    /// <summary>
    ///     Exception thrown whenever there's an issue modifying the <see cref="IChannelPipeline" />
    /// </summary>
    public class ChannelPipelineException : Exception
    {
        public ChannelPipelineException(string message)
            : base(message)
        {
        }

        public ChannelPipelineException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}