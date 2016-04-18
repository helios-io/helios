using System;
using Helios.Buffers;

namespace Helios.Channels
{
    public interface IChannelConfiguration
    {
        TimeSpan ConnectTimeout { get; set; }

        int MaxMessagesPerRead { get; set; }

        IByteBufAllocator Allocator { get; set; }

        IRecvByteBufAllocator RecvByteBufAllocator { get; set; }

        IMessageSizeEstimator MessageSizeEstimator { get; set; }

        bool AutoRead { get; set; }

        int WriteBufferHighWaterMark { get; set; }

        int WriteBufferLowWaterMark { get; set; }

        int WriteSpinCount { get; set; }
        T GetOption<T>(ChannelOption<T> option);

        bool SetOption(ChannelOption option, object value);

        bool SetOption<T>(ChannelOption<T> option, T value);
    }
}