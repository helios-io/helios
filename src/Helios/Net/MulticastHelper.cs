// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Helios.Net
{
    /// <summary>
    ///     Class used to help with some UDP-specific connection properties
    /// </summary>
    public static class MulticastHelper
    {
        public static readonly string MulticastIPv4AddressRangeRegex =
            @"\b(22[0-4]|23[0-9])\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b";

        private static readonly Regex MRegex = new Regex(MulticastIPv4AddressRangeRegex);

        /*
         * IPv4 multicast address range:  224.0.0.0 to 239.255.255.255
         * IPv6 multicast address range: 
         */

        public static bool IsValidMulticastAddress(IPAddress address)
        {
            switch (address.AddressFamily)
            {
                case AddressFamily.InterNetwork: //IPv4
                    return MRegex.IsMatch(address.ToString());
                case AddressFamily.InterNetworkV6: //IPv6
                    return address.IsIPv6Multicast;
                default:
                    return false;
            }
        }
    }
}