using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Channels.Local
{
    public static class LocalChannelRegistry
    {
        private static readonly ConcurrentDictionary<EndPoint, IChannel> BoundChannels = new ConcurrentDictionary<EndPoint, IChannel>();

        public static LocalAddress Register(IChannel channel, LocalAddress oldLocalAddress, EndPoint localAddress)
        {
            if(oldLocalAddress != null)
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

            if(BoundChannels.ContainsKey(addr))
                throw new HeliosException($"address {addr} is already in use ");

            IChannel boundChannel = BoundChannels.GetOrAdd(addr, channel);
            return addr;
        }

        public static IChannel Get(EndPoint localAddress)
        {
            return BoundChannels[localAddress];
        }

        public static void Unregister(LocalAddress localAddress)
        {
            IChannel channel;
            BoundChannels.TryRemove(localAddress, out channel);
        }
    }
}
