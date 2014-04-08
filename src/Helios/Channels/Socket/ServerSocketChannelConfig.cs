using System;

namespace Helios.Channels.Socket
{
    public interface IServerSocketChannelConfig : IChannelConfig
    {
        /// <summary>
        /// Gets the backlog value to specify when the channel binds to a
        /// local address
        /// </summary>
        int Backlog { get; }

        IServerSocketChannelConfig SetBacklog(int backLog);

        bool IsReuseAddress { get; }

        IServerSocketChannelConfig SetReuseAddress(bool reuseAddress);

        int ReceiveBufferSize { get; }

        IServerSocketChannelConfig SetRecieveBufferSize(int bufferSize);

        new IServerSocketChannelConfig SetConnectTimeout(TimeSpan timeout);

        new IServerSocketChannelConfig SetAutoRead(bool autoRead);

        new IServerSocketChannelConfig SetWriteBufferHighWaterMark(int waterMark);

        new IServerSocketChannelConfig SetWriteBufferLowWaterMark(int waterMark);

        new IServerSocketChannelConfig SetMaxMessagesPerRead(int maxMessagesPerRead);

        new IServerSocketChannelConfig SetRecvAllocator(IRecvByteBufAllocator recvByteBufAllocator);

        new IServerSocketChannelConfig SetWriteSpinCount(int spinCount);
    }
}
