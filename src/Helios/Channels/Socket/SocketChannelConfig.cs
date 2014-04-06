using System;

namespace Helios.Channels.Socket
{
    /// <summary>
    /// <see cref="IChannelConfig"/> for a <see cref="ISocketChannel"/>
    /// </summary>
    public interface ISocketChannelConfig : IChannelConfig
    {
        /// <summary>
        /// Disables Nagle algorithm when set to true.
        /// 
        /// Defaults to true.
        /// </summary>
        bool TcpNoDelay { get; }

        ISocketChannelConfig SetTcpNoDelay(bool tcpNoDelay);

        int SendBufferSize { get; }

        ISocketChannelConfig SetSendBufferSize(int sendBufferSize);

        int ReceiveBufferSize { get; }

        ISocketChannelConfig SetReceiveBufferSize(int receiveBufferSize);

        bool KeepAlive { get; }

        ISocketChannelConfig SetKeepAlive(bool keepAlive);

        bool ReuseAddress { get; }

        ISocketChannelConfig SetReuseAddress(bool reuseAddress);

        new ISocketChannelConfig SetConnectTimeout(TimeSpan timeout);

        new ISocketChannelConfig SetAutoRead(bool autoRead);

        new ISocketChannelConfig SetWriteBufferHighWaterMark(int waterMark);

        new ISocketChannelConfig SetWriteBufferLowWaterMark(int waterMark);

        new ISocketChannelConfig SetMaxMessagesPerRead(int maxMessagesPerRead);
    }
}
