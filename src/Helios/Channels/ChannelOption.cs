// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Diagnostics.Contracts;
using Helios.Buffers;

namespace Helios.Channels
{
    /// <summary>
    ///     A strongly typed class representing a configuration option for a given channel.
    /// </summary>
    public abstract class ChannelOption
    {
        public static readonly ChannelOption<IByteBufAllocator> Allocator = new ChannelOption<IByteBufAllocator>();

        public static readonly ChannelOption<IRecvByteBufAllocator> RcvbufAllocator =
            new ChannelOption<IRecvByteBufAllocator>();

        public static readonly ChannelOption<IMessageSizeEstimator> MessageSizeEstimator =
            new ChannelOption<IMessageSizeEstimator>();

        public static readonly ChannelOption<TimeSpan> ConnectTimeout = new ChannelOption<TimeSpan>();
        public static readonly ChannelOption<int> MaxMessagesPerRead = new ChannelOption<int>();
        public static readonly ChannelOption<int> WriteSpinCount = new ChannelOption<int>();
        public static readonly ChannelOption<int> WriteBufferHighWaterMark = new ChannelOption<int>();
        public static readonly ChannelOption<int> WriteBufferLowWaterMark = new ChannelOption<int>();

        public static readonly ChannelOption<bool> AllowHalfClosure = new ChannelOption<bool>();
        public static readonly ChannelOption<bool> AutoRead = new ChannelOption<bool>();

        public static readonly ChannelOption<bool> SoBroadcast = new ChannelOption<bool>();
        public static readonly ChannelOption<bool> SoKeepalive = new ChannelOption<bool>();
        public static readonly ChannelOption<int> SoSndbuf = new ChannelOption<int>();
        public static readonly ChannelOption<int> SoRcvbuf = new ChannelOption<int>();
        public static readonly ChannelOption<bool> SoReuseaddr = new ChannelOption<bool>();
        public static readonly ChannelOption<int> SoLinger = new ChannelOption<int>();
        public static readonly ChannelOption<int> SoBacklog = new ChannelOption<int>();
        public static readonly ChannelOption<int> SoTimeout = new ChannelOption<int>();

        //public static readonly ChannelOption<int> IP_TOS = new ChannelOption<int>();
        //public static readonly ChannelOption<InetAddress> IP_MULTICAST_ADDR = new ChannelOption<int>("IP_MULTICAST_ADDR");
        //public static readonly ChannelOption<NetworkInterface> IP_MULTICAST_IF = new ChannelOption<int>("IP_MULTICAST_IF");
        public static readonly ChannelOption<int> IpMulticastTtl = new ChannelOption<int>();
        public static readonly ChannelOption<bool> IpMulticastLoopDisabled = new ChannelOption<bool>();

        public static readonly ChannelOption<bool> TcpNodelay = new ChannelOption<bool>();

        internal ChannelOption()
        {
        }

        public abstract bool Set(IChannelConfiguration configuration, object value);
    }

    public sealed class ChannelOption<T> : ChannelOption
    {
        public void Validate(T value)
        {
            Contract.Requires(value != null);
        }

        public override bool Set(IChannelConfiguration configuration, object value)
        {
            return configuration.SetOption(this, (T) value);
        }
    }
}