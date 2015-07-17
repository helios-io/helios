using Helios.Buffers;
using System;
using System.Collections.Generic;

namespace Helios.Channel
{
    /// <summary>
    /// The default <see cref="SocketChannelConfig"/> implementation.
    /// </summary>
    public class DefaultChannelConfig : IChannelConfig
    {
        private static readonly IRecvByteBufAllocator DEFAULT_RCVBUF_ALLOCATOR = AdaptiveRecvByteBufAllocator.Default;
        private static readonly IMessageSizeEstimator DEFAULT_MSG_SIZE_ESTIMATOR = DefaultMessageSizeEstimator.Default;

        // ReSharper disable once InconsistentNaming
        private const int DEFAULT_CONNECT_TIMEOUT = 30000; //30s

        protected readonly IChannel Channel;

        private volatile IByteBufAllocator _allocator = UnpooledByteBufAllocator.Default;
        private volatile IRecvByteBufAllocator _recvAllocator = DEFAULT_RCVBUF_ALLOCATOR;
        private volatile IMessageSizeEstimator _msgSizeEstimator = DEFAULT_MSG_SIZE_ESTIMATOR;

        private IDictionary<ChannelOption<object>, object> _options;
        private volatile int _connectTimeoutMillis = DEFAULT_CONNECT_TIMEOUT;
        private volatile int _maxMessagesPerRead;
        private volatile int _writeSpinCount = 16;
        private volatile bool _autoRead = true;
        private volatile int _writeBufferHighWaterMark = 64 * 1024;
        private volatile int _writeBufferLowWaterMark = 32 * 1024;

        public DefaultChannelConfig(IChannel channel)
        {
            if (channel == null)
            {
                throw new ArgumentNullException("channel");
            }
            Channel = channel;

            //TODO: add server vs. client maxMessagesPerRead settings https://github.com/netty/netty/blob/dec53cff86f9144c16ae446662bd4ea6e13e0a6d/transport/src/main/java/io/netty/channel/DefaultChannelConfig.java#L70
            _maxMessagesPerRead = 16;
        }


        public IDictionary<ChannelOption<object>, object> Options
        {
            get { return _options; }
        }

        public bool SetOptions(IDictionary<ChannelOption<object>, object> options)
        {
            throw new System.NotImplementedException();
        }

        public T GetOption<T>(ChannelOption<T> option)
        {
            if (option == null)
                throw new ArgumentNullException("option");
            if (option == ChannelOption.CONNECT_TIMEOUT_MILLIS)
                return (T)(object)ConnectTimeoutMillis;
            if (option == ChannelOption.MAX_MESSAGES_PER_READ)
                return (T)(object)MaxMessagesPerRead;
            if (option == ChannelOption.WRITE_SPIN_COUNT)
                return (T)(object)WriteSpinCount;
            if (option == ChannelOption.ALLOCATOR)
                return (T)(object)Allocator;
            if (option == ChannelOption.RCVBUF_ALLOCATOR)
                return (T)RecvAllocator;
            if (option == ChannelOption.AUTO_READ)
                return (T)(object)AutoRead;
            if (option == ChannelOption.WRITE_BUFFER_HIGH_WATER_MARK)
                return (T)(object)WriteBufferHighWaterMark;
            if (option == ChannelOption.WRITE_BUFFER_LOW_WATER_MARK)
                return (T)(object)WriteBufferLowWaterMark;
            if (option == ChannelOption.MESSAGE_SIZE_ESTIMATOR)
                return (T)MessageSizeEstimator;
            return default(T);
        }

        public bool SetOption<T>(ChannelOption<T> option, T value)
        {
            Validate(option, value);
            if (option == ChannelOption.CONNECT_TIMEOUT_MILLIS)
            {
                SetConnectTimeoutMillis((int)(object)value);
            }
            else if (option == ChannelOption.MAX_MESSAGES_PER_READ)
            {
                SetMaxMessagesPerRead((int)(object)value);
            }
            else if (option == ChannelOption.WRITE_SPIN_COUNT)
            {
                SetWriteSpinCount((int)(object)value);
            }
            else if (option == ChannelOption.ALLOCATOR)
            {
                SetAllocator((IByteBufAllocator)value);
            }
            else if (option == ChannelOption.RCVBUF_ALLOCATOR)
            {
                SetRecvAllocator((IRecvByteBufAllocator)value);
            }
            else if (option == ChannelOption.AUTO_READ)
            {
                SetAutoRead((bool)(object)value);
            }
            else if (option == ChannelOption.WRITE_BUFFER_HIGH_WATER_MARK)
            {
                SetWriteBufferHighWaterMark((int)(object)value);
            }
            else if (option == ChannelOption.WRITE_BUFFER_LOW_WATER_MARK)
            {
                SetWriteBufferLowWaterMark((int)(object)value);
            }
            else if (option == ChannelOption.MESSAGE_SIZE_ESTIMATOR)
            {
                SetMessageSizeEstimator((IMessageSizeEstimator)value);
            }
            else
            {
                return false;
            }
            return true;
        }

        protected void Validate<T>(ChannelOption<T> option, T value)
        {
            if (option == null)
                throw new ArgumentNullException("option");
            option.Validate(value);
        }

        public int ConnectTimeoutMillis
        {
            get { return _connectTimeoutMillis; }
        }

        public IChannelConfig SetConnectTimeoutMillis(int connectTimeoutMillis)
        {
            if (connectTimeoutMillis < 0)
                throw new ArgumentOutOfRangeException("connectTimeoutMillis", "connectTimeoutMillis: " + connectTimeoutMillis + " (expected >= 0)");
            _connectTimeoutMillis = connectTimeoutMillis;
            return this;
        }

        public int MaxMessagesPerRead
        {
            get { return _maxMessagesPerRead; }
        }

        public IChannelConfig SetMaxMessagesPerRead(int maxMessagesPerRead)
        {
            if (maxMessagesPerRead <= 0)
                throw new ArgumentOutOfRangeException("maxMessagesPerRead", "maxMessagesPerRead: " + maxMessagesPerRead + " (expected > 0)");
            _maxMessagesPerRead = maxMessagesPerRead;
            return this;
        }

        public int WriteSpinCount
        {
            get { return _writeSpinCount; }
        }

        public IChannelConfig SetWriteSpinCount(int spinCount)
        {
            if (spinCount <= 0)
                throw new ArgumentOutOfRangeException("spinCount", "spinCount: " + spinCount + " (expected > 0)");
            _writeSpinCount = spinCount;
            return this;
        }

        public IByteBufAllocator Allocator
        {
            get { return _allocator; }
        }

        public IChannelConfig SetAllocator(IByteBufAllocator allocator)
        {
            if (allocator == null)
                throw new ArgumentNullException("allocator");
            _allocator = allocator;
            return this;
        }

        public IRecvByteBufAllocator RecvAllocator
        {
            get { return _recvAllocator; }
        }

        public IChannelConfig SetRecvAllocator(IRecvByteBufAllocator allocator)
        {
            if (allocator == null)
                throw new ArgumentNullException("allocator");
            _recvAllocator = allocator;
            return this;
        }

        public bool AutoRead
        {
            get { return _autoRead; }
        }

        public IChannelConfig SetAutoRead(bool autoRead)
        {
            var oldAutoRead = _autoRead;
            _autoRead = autoRead;
            if (_autoRead && !oldAutoRead)
            {
                Channel.Read();
            }
            else if (!AutoRead && oldAutoRead)
            {
                AutoReadCleared();
            }
            return this;
        }

        /// <summary>
        /// Called once <see cref="SetAutoRead"/> is called with <c>false</c> and <see cref="AutoRead"/> was
        /// <c>true</c> before.
        /// </summary>
        protected void AutoReadCleared() { }

        public int WriteBufferHighWaterMark
        {
            get { return _writeBufferHighWaterMark; }
        }

        public IChannelConfig SetWriteBufferHighWaterMark(int writeBufferHighWaterMark)
        {
            if (writeBufferHighWaterMark < WriteBufferLowWaterMark)
            {
                throw new ArgumentOutOfRangeException("writeBufferHighWaterMark", "writeBufferHighWaterMark cannot be less than" +
                                                                                  "writeBufferLowWaterMark (" + WriteBufferLowWaterMark + "): "
                                                                                  + writeBufferHighWaterMark);
            }

            if (writeBufferHighWaterMark < 0)
                throw new ArgumentOutOfRangeException("writeBufferHighWaterMark", "writeBufferHighWaterMark must be >= 0");
            _writeBufferHighWaterMark = writeBufferHighWaterMark;
            return this;
        }

        public int WriteBufferLowWaterMark
        {
            get { return _writeBufferLowWaterMark; }
        }

        public IChannelConfig SetWriteBufferLowWaterMark(int writeBufferLowWaterMark)
        {
            if (_writeBufferLowWaterMark > _writeBufferHighWaterMark)
            {
                throw new ArgumentOutOfRangeException("writeBufferLowWaterMark", "writeBufferLowWaterMark cannot be greater than " +
                                                                                 "writeBufferHighWaterMark (" + WriteBufferHighWaterMark + "): " +
                                                                                 writeBufferLowWaterMark);
            }

            if (writeBufferLowWaterMark < 0)
                throw new ArgumentOutOfRangeException("writeBufferLowWaterMark", "writeBufferLowWaterMark must be >= 0");
            _writeBufferLowWaterMark = writeBufferLowWaterMark;
            return this;
        }

        public IMessageSizeEstimator MessageSizeEstimator
        {
            get { return _msgSizeEstimator; }
        }

        public IChannelConfig SetMessageSizeEstimator(IMessageSizeEstimator estimator)
        {
            if (estimator == null)
            {
                throw new ArgumentNullException("estimator");
            }

            _msgSizeEstimator = estimator;
            return this;
        }
    }
}