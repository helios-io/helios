using System;

namespace Helios.Channels
{
    public class ChannelException : Exception
    {
        public ChannelException(string message) : base(message)
        {
        }

        public ChannelException(string connectionRefused, Exception exception) : base(connectionRefused, exception)
        {
            
        }
    }
}