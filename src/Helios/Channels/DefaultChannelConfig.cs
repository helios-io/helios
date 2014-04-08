using System;

namespace Helios.Channels
{
    public class DefaultChannelConfig : IChannelConfig
    {
        private static readonly TimeSpan DefaultConnectTimeout = TimeSpan.FromMilliseconds(30000);

        public static IRecvByteBufAllocator DefaultAllocator = new FixedRecvByteBufAllocator(1024*4);

        protected readonly IChannel Channel;

        public DefaultChannelConfig(AbstractChannel channel)
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
            channel.Config = this;
           
            ConnectTimeout = DefaultConnectTimeout;
            WriteBufferHighWaterMark = 64*1024;
            WriteBufferLowWaterMark = 32*1024;
            WriteSpinCount = 16;
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

        public IRecvByteBufAllocator RecvAllocator { get; private set; }
        public IChannelConfig SetRecvAllocator(IRecvByteBufAllocator recvByteBufAllocator)
        {
            RecvAllocator = recvByteBufAllocator;
            return this;
        }

        public int WriteSpinCount { get; private set; }
        public IChannelConfig SetWriteSpinCount(int spinCount)
        {
            WriteSpinCount = spinCount;
            return this;
        }
    }
}