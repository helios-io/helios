// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Helios.Channels.Bootstrap
{
    public interface INameResolver
    {
        bool IsResolved(EndPoint address);

        Task<EndPoint> ResolveAsync(EndPoint address);

        Task<EndPoint> ResolveAsync(EndPoint address, AddressFamily preferredFamily);
    }
}