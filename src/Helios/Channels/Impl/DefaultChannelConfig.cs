using System;

namespace Helios.Channels.Impl
{
    public class DefaultChannelConfig : IChannelConfig
    {
        private static readonly TimeSpan DefaultConnectTimeout = TimeSpan.FromMilliseconds(30000);

        protected readonly IChannel Channel;

        public DefaultChannelConfig(IChannel channel)
        {
            Channel = channel;
            if (channel is IServerChannel)
            {
                MaxMessagesPerRead = 16;
            }
            else
            {
                MaxMessagesPerRead = 1;
            }
           
            ConnectTimeout = DefaultConnectTimeout;
            WriteBufferHighWaterMark = 64*1024;
            WriteBufferLowWaterMark = 32*1024;
        }

        public int MaxMessagesPerRead { get; private set; }
        public IChannelConfig SetMaxMessagesPerRead(int maxMessagesPerRead)
        {
            MaxMessagesPerRead = maxMessagesPerRead;
            return this;
        }

        public bool IsAutoRead { get; private set; }
        public IChannelConfig SetAutoRead(bool autoRead)
        {
            IsAutoRead = autoRead;
            return this;
        }

        public TimeSpan ConnectTimeout { get; private set; }
        public IChannelConfig SetConnectTimeout(TimeSpan timeout)
        {
            if(timeout.TotalMilliseconds < 0) throw new ArgumentOutOfRangeException("timeout", "Timeout must be greater than zero");
            ConnectTimeout = timeout;
            return this;
        }

        public int WriteBufferHighWaterMark { get; private set; }
        public IChannelConfig SetWriteBufferHighWaterMark(int waterMark)
        {
            WriteBufferHighWaterMark = waterMark;
            return this;
        }

        public int WriteBufferLowWaterMark { get; private set; }
        public IChannelConfig SetWriteBufferLowWaterMark(int waterMark)
        {
            WriteBufferLowWaterMark = waterMark;
            return this;
        }
    }
}