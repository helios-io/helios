using System;

namespace Helios.Channels
{
    /// <summary>
    /// Configuration interface associated with a given <see cref="IChannel"/>
    /// </summary>
    public interface IChannelConfig
    {
        int MaxMessagesPerRead { get; }

        IChannelConfig SetMaxMessagesPerRead(int maxMessagesPerRead);

        bool IsAutoRead { get; }

        IChannelConfig SetAutoRead(bool autoRead);

        TimeSpan ConnectTimeout { get; }

        IChannelConfig SetConnectTimeout(TimeSpan timeout);

        /// <summary>
        /// Returns the high water mark of the write buffer. If the number of bytes
        /// queued in the write buffer exceeds this value, <see cref="IChannel.IsWritable"/>
        /// will start to return false.
        /// </summary>
        int WriteBufferHighWaterMark { get; }

        IChannelConfig SetWriteBufferHighWaterMark(int waterMark);

        /// <summary>
        /// Returns the low water mark of the write buffer. If the number of bytes queued in the write
        /// buffer exceeded the <see cref="WriteBufferHighWaterMark"/> and now <see cref="IChannel.IsWritable"/>
        /// is false, <see cref="IChannel.IsWritable"/> will start returning true again if the bytes queued
        /// falls below this value.
        /// </summary>
        int WriteBufferLowWaterMark { get; }

        IChannelConfig SetWriteBufferLowWaterMark(int waterMark);

        IRecvByteBufAllocator RecvAllocator { get; }
       

        IChannelConfig SetRecvAllocator(IRecvByteBufAllocator recvByteBufAllocator);

        int WriteSpinCount { get; }

        IChannelConfig SetWriteSpinCount(int spinCount);
    }
}
