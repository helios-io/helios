// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using Helios.Channels;
using Helios.Channels.Embedded;

namespace Helios.FsCheck.Tests.Channels
{
    /// <summary>
    ///     Internal <see cref="IChannel" /> implementation used for lightweight testing.
    ///     Mostly for testing things that go INSIDE the channel itself.
    /// </summary>
    internal class TestChannel
    {
        public static IChannel Instance => NewInstance();

        public static IChannel NewInstance(params IChannelHandler[] handlers)
        {
            var instance = new EmbeddedChannel(handlers);
            instance.Configuration.WriteBufferHighWaterMark = ChannelOutboundBufferSpecs.WriteHighWaterMark;
            instance.Configuration.WriteBufferLowWaterMark = ChannelOutboundBufferSpecs.WriteLowWaterMark;
            instance.Configuration.AutoRead = false;
                // interferes with testing the invocation model for ChannelReadComplete
            return instance;
        }
    }
}

