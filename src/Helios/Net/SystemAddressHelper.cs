// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace Helios.Net
{
    /// <summary>
    ///     INTERNAL API
    ///     Used to resolve the local MAC and IP addresses
    /// </summary>
    public static class SystemAddressHelper
    {
        public static IPAddress ConnectedIp
        {
            get { return GetConnectedNetworkInterface().GetIPProperties().UnicastAddresses.First().Address; }
        }

        public static IEnumerable<IPAddress> ConnectedIps
        {
            get { return GetConnectedNetworkInterface().GetIPProperties().UnicastAddresses.Select(x => x.Address); }
        }

        public static PhysicalAddress ConnectedMacAddress
        {
            get { return GetConnectedNetworkInterface().GetPhysicalAddress(); }
        }

        /// <summary>
        ///     Indicates whether any network connection is available.
        ///     Filter connections below a specified speed, as well as virtual network cards.
        /// </summary>
        /// <returns>
        ///     <c>NetworkInterface</c> the connected network interface, otherwise <c>null</c>.
        /// </returns>
        public static NetworkInterface GetConnectedNetworkInterface()
        {
            IList<NetworkInterface> interfacesWithData = new List<NetworkInterface>();

            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                // discard because of standard reasons
                if ((ni.OperationalStatus != OperationalStatus.Up) ||
                    (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) ||
                    (ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel))
                    continue;

                // discard "Microsoft Loopback Adapter", it will not show as NetworkInterfaceType.Loopback but as Ethernet Card.
                if (ni.Description.Equals("Microsoft Loopback Adapter", StringComparison.OrdinalIgnoreCase))
                    continue;

                // discard virtual cards (virtual box, virtual pc, etc.)
                if ((ni.Description.IndexOf("virtual", StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (ni.Name.IndexOf("virtual", StringComparison.OrdinalIgnoreCase) >= 0))
                    continue;

                //Return the network interface which has received bytes
                if (ni.GetIPv4Statistics().BytesReceived > 0)
                    interfacesWithData.Add(ni);
            }

            //Couldn't find any network adapters with data
            if (interfacesWithData.Count == 0)
                return null; //return null

            var maxBytesReceived = interfacesWithData.Max(x => x.GetIPv4Statistics().BytesReceived);
            return interfacesWithData.FirstOrDefault(x => x.GetIPv4Statistics().BytesReceived >= maxBytesReceived);
        }
    }
}