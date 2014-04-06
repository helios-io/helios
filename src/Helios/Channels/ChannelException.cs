using System;
namespace Helios.Channels
{
    public class HeliosChannelException : HeliosException
    {
        public HeliosChannelException(string message, Exception inner = null) : base(message, inner)
        {
        }

        public HeliosChannelException(Exception ex) : base(ex.Message, ex) { }
}

    public class HeliosChannelPipelineException : HeliosException
    {
        public HeliosChannelPipelineException(string message, Exception cause = null) : base(message, cause) { }
    }
}
