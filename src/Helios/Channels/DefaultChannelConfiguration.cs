// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Diagnostics.Contracts;
using System.Threading;
using Helios.Buffers;
using Helios.Channels.Sockets;

namespace Helios.Channels
{
    public class DefaultChannelConfiguration : IChannelConfiguration
    {
        private static readonly TimeSpan DefaultConnectTimeout = TimeSpan.FromSeconds(30);
        protected readonly IChannel Channel;
        private volatile IByteBufAllocator _allocator = UnpooledByteBufAllocator.Default;
        private volatile int _autoRead = 1;
        private TimeSpan _connectTimeout = DefaultConnectTimeout;
        private volatile int _maxMessagesPerRead;
        private volatile IMessageSizeEstimator _messageSizeEstimator = DefaultMessageSizeEstimator.Default;
        private volatile IRecvByteBufAllocator _recvByteBufAllocator = FixedRecvByteBufAllocator.Default;
        private volatile int _writeBufferHighWaterMark = 64*1024;
        private volatile int _writeBufferLowWaterMark = 32*1024;
        private volatile int _writeSpinCount = 16;

        public DefaultChannelConfiguration(IChannel channel)
        {
            Channel = channel;
            if (channel is IServerChannel || channel is AbstractSocketByteChannel)
            {
                _maxMessagesPerRead = 16;
            }
            else
            {
                _maxMessagesPerRead = 1;
            }
        }

        public virtual T GetOption<T>(ChannelOption<T> option)
        {
            Contract.Requires(option != null);

            if (ChannelOption.ConnectTimeout.Equals(option))
            {
                return (T) (object) ConnectTimeout; // no boxing will happen, compiler optimizes away such casts
            }
            if (ChannelOption.MaxMessagesPerRead.Equals(option))
            {
                return (T) (object) MaxMessagesPerRead;
            }
            if (ChannelOption.WriteSpinCount.Equals(option))
            {
                return (T) (object) WriteSpinCount;
            }
            if (ChannelOption.Allocator.Equals(option))
            {
                return (T) Allocator;
            }
            if (ChannelOption.RcvbufAllocator.Equals(option))
            {
                return (T) RecvByteBufAllocator;
            }
            if (ChannelOption.AutoRead.Equals(option))
            {
                return (T) (object) AutoRead;
            }
            if (ChannelOption.WriteBufferHighWaterMark.Equals(option))
            {
                return (T) (object) WriteBufferHighWaterMark;
            }
            if (ChannelOption.WriteBufferLowWaterMark.Equals(option))
            {
                return (T) (object) WriteBufferLowWaterMark;
            }
            if (ChannelOption.MessageSizeEstimator.Equals(option))
            {
                return (T) MessageSizeEstimator;
            }
            return default(T);
        }

        public bool SetOption(ChannelOption option, object value)
        {
            return option.Set(this, value);
        }

        public virtual bool SetOption<T>(ChannelOption<T> option, T value)
        {
            Validate(option, value);

            if (ChannelOption.ConnectTimeout.Equals(option))
            {
                ConnectTimeout = (TimeSpan) (object) value;
            }
            else if (ChannelOption.MaxMessagesPerRead.Equals(option))
            {
                MaxMessagesPerRead = (int) (object) value;
            }
            else if (ChannelOption.WriteSpinCount.Equals(option))
            {
                WriteSpinCount = (int) (object) value;
            }
            else if (ChannelOption.Allocator.Equals(option))
            {
                Allocator = (IByteBufAllocator) value;
            }
            else if (ChannelOption.RcvbufAllocator.Equals(option))
            {
                RecvByteBufAllocator = (IRecvByteBufAllocator) value;
            }
            else if (ChannelOption.AutoRead.Equals(option))
            {
                AutoRead = (bool) (object) value;
            }
            else if (ChannelOption.WriteBufferHighWaterMark.Equals(option))
            {
                _writeBufferHighWaterMark = (int) (object) value;
            }
            else if (ChannelOption.WriteBufferLowWaterMark.Equals(option))
            {
                WriteBufferLowWaterMark = (int) (object) value;
            }
            else if (ChannelOption.MessageSizeEstimator.Equals(option))
            {
                MessageSizeEstimator = (IMessageSizeEstimator) value;
            }
            else
            {
                return false;
            }

            return true;
        }

        public TimeSpan ConnectTimeout
        {
            get
            {
                var result = _connectTimeout;
                Thread.MemoryBarrier();
                return result;
            }
            set
            {
                Contract.Requires(value >= TimeSpan.Zero);
                Thread.MemoryBarrier();
                _connectTimeout = value;
            }
        }

        public IByteBufAllocator Allocator
        {
            get { return _allocator; }
            set
            {
                Contract.Requires(value != null);
                _allocator = value;
            }
        }

        public IRecvByteBufAllocator RecvByteBufAllocator
        {
            get { return _recvByteBufAllocator; }
            set
            {
                Contract.Requires(value != null);
                _recvByteBufAllocator = value;
            }
        }

        public IMessageSizeEstimator MessageSizeEstimator
        {
            get { return _messageSizeEstimator; }
            set
            {
                Contract.Requires(value != null);
                _messageSizeEstimator = value;
            }
        }

        protected virtual void Validate<T>(ChannelOption<T> option, T value)
        {
            Contract.Requires(option != null);
            option.Validate(value);
        }

        public bool AutoRead
        {
            get { return _autoRead == 1; }
            set
            {
#pragma warning disable 420 // atomic exchange is ok
                var oldAutoRead = Interlocked.Exchange(ref _autoRead, value ? 1 : 0) == 1;
#pragma warning restore 420
                if (value && !oldAutoRead)
                {
                    Channel.Read();
                }
                else if (!value && oldAutoRead)
                {
                    AutoReadCleared();
                }
            }
        }

        protected virtual void AutoReadCleared()
        {
        }

        public int MaxMessagesPerRead
        {
            get { return _maxMessagesPerRead; }
            set
            {
                Contract.Requires(value >= 1);
                _maxMessagesPerRead = value;
            }
        }

        public int WriteBufferHighWaterMark
        {
            get { return _writeBufferHighWaterMark; }
            set
            {
                Contract.Requires(value >= 0);
                Contract.Requires(value >= _writeBufferLowWaterMark);

                _writeBufferHighWaterMark = value;
            }
        }

        public int WriteBufferLowWaterMark
        {
            get { return _writeBufferLowWaterMark; }
            set
            {
                Contract.Requires(value >= 0);
                Contract.Requires(value <= _writeBufferHighWaterMark);

                _writeBufferLowWaterMark = value;
            }
        }

        public int WriteSpinCount
        {
            get { return _writeSpinCount; }
            set
            {
                Contract.Requires(value >= 1);

                _writeSpinCount = value;
            }
        }
    }
}