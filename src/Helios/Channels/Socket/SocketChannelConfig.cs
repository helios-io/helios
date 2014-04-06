using System;
using Helios.Net.Connections;

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

    public class DefaultSocketChannelConfig : DefaultChannelConfig, ISocketChannelConfig
    {
        protected readonly TcpConnection Connection;

        public DefaultSocketChannelConfig(ISocketChannel channel, TcpConnection connection) : base(channel)
        {
            if(connection == null) throw new ArgumentNullException("connection");
            Connection = connection;

            Connection.NoDelay = true;
        }

        public bool TcpNoDelay
        {
            get
            {
                try
                {
                    return Connection.NoDelay;
                }
                catch (Exception ex)
                {
                    throw new HeliosChannelException(ex);
                }
            }
        }

        public ISocketChannelConfig SetTcpNoDelay(bool tcpNoDelay)
        {
            Connection.NoDelay = TcpNoDelay;
            return this;
        }

        public int SendBufferSize
        {
            get
            {
                try
                {
                    return Connection.SendBufferSize;
                }
                catch (Exception ex)
                {
                    throw new HeliosChannelException(ex);
                }
            }
        }

        public ISocketChannelConfig SetSendBufferSize(int sendBufferSize)
        {
            Connection.SendBufferSize = sendBufferSize;
            return this;
        }

        public int ReceiveBufferSize
        {
            get
            {
                try
                {
                    return Connection.ReceiveBufferSize;
                }
                catch (Exception ex)
                {
                    throw new HeliosChannelException(ex);
                }
            }
        }

        public ISocketChannelConfig SetReceiveBufferSize(int receiveBufferSize)
        {
            Connection.ReceiveBufferSize = receiveBufferSize;
            return this;
        }

        public bool KeepAlive
        {
            get
            {
                try
                {
                    return Connection.KeepAlive;
                }
                catch (Exception ex)
                {
                    throw new HeliosChannelException(ex);
                }
            }
        }

        public ISocketChannelConfig SetKeepAlive(bool keepAlive)
        {
            Connection.KeepAlive = keepAlive;
            return this;
        }

        public bool ReuseAddress
        {
            get { return Connection.ReuseAddress; }
        }

        public ISocketChannelConfig SetReuseAddress(bool reuseAddress)
        {
            Connection.ReuseAddress = reuseAddress;
            return this;
        }

        public new ISocketChannelConfig SetConnectTimeout(TimeSpan timeout)
        {
            base.SetConnectTimeout(timeout);
            return this;
        }

        public new ISocketChannelConfig SetAutoRead(bool autoRead)
        {
            base.SetAutoRead(autoRead);
            return this;
        }

        public new ISocketChannelConfig SetWriteBufferHighWaterMark(int waterMark)
        {
            base.SetWriteBufferHighWaterMark(waterMark);
            return this;
        }

        public new ISocketChannelConfig SetWriteBufferLowWaterMark(int waterMark)
        {
            base.SetWriteBufferLowWaterMark(waterMark);
            return this;
        }

        public new ISocketChannelConfig SetMaxMessagesPerRead(int maxMessagesPerRead)
        {
            base.SetMaxMessagesPerRead(maxMessagesPerRead);
            return this;
        }
    }
}
