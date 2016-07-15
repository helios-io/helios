// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Helios.Channels.Bootstrap
{
    public class DefaultNameResolver : INameResolver
    {
        public bool IsResolved(EndPoint address)
        {
            return !(address is DnsEndPoint);
        }

        public async Task<EndPoint> ResolveAsync(EndPoint address)
        {
            var asDns = address as DnsEndPoint;
            if (asDns != null)
            {
                var resolved = await Dns.GetHostEntryAsync(asDns.Host);
                return new IPEndPoint(resolved.AddressList[0], asDns.Port);
            }
            return address;
        }

        public async Task<EndPoint> ResolveAsync(EndPoint address, AddressFamily preferredFamily)
        {
            var asDns = address as DnsEndPoint;
            if (asDns != null)
            {
                var resolved = await Dns.GetHostEntryAsync(asDns.Host);
                return new IPEndPoint(resolved.AddressList.First(x => x.AddressFamily == preferredFamily), asDns.Port);
            }
            return address;
        }
    }
}