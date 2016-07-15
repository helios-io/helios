// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Collections.Concurrent;
using System.Net;

namespace Helios.Channels.Local
{
    public static class LocalChannelRegistry
    {
        private static readonly ConcurrentDictionary<EndPoint, IChannel> BoundChannels =
            new ConcurrentDictionary<EndPoint, IChannel>();

        public static LocalAddress Register(IChannel channel, LocalAddress oldLocalAddress, EndPoint localAddress)
        {
            if (oldLocalAddress != null)
                throw new HeliosException("already bound");

            if (!(localAddress is LocalAddress))
            {
                throw new HeliosException($"Unsupported address type {localAddress.GetType()}");
            }

            var addr = (LocalAddress) localAddress;
            if (LocalAddress.Any.Equals(addr))
            {
                addr = new LocalAddress(channel);
            }

            if (BoundChannels.ContainsKey(addr))
                throw new HeliosException($"address {addr} is already in use ");

            var boundChannel = BoundChannels.GetOrAdd(addr, channel);
            return addr;
        }

        public static IChannel Get(EndPoint localAddress)
        {
            if (BoundChannels.ContainsKey(localAddress))
                return BoundChannels[localAddress];
            return null;
        }

        public static void Unregister(LocalAddress localAddress)
        {
            IChannel channel;
            BoundChannels.TryRemove(localAddress, out channel);
        }
    }
}