// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Helios.FsCheck.Tests.Channels.Sockets.Models
{
    /// <summary>
    ///     <see cref="IEqualityComparer{T}" /> that verifies that any two <see cref="ITcpServerSocketModel" /> are equivalent,
    ///     regardless of the
    ///     order of messages received / written and the order of clients joined / not joined.
    /// </summary>
    public class TcpServerSocketModelComparer : IEqualityComparer<ITcpServerSocketModel>
    {
        public static readonly TcpServerSocketModelComparer Instance = new TcpServerSocketModelComparer();

        private TcpServerSocketModelComparer()
        {
        }

        public bool Equals(ITcpServerSocketModel x, ITcpServerSocketModel y)
        {
            // quickly eliminate models that aren't equal by checking counts across all collections
            var sameCounts = x.LastReceivedMessages.Count == y.LastReceivedMessages.Count &&
                             x.WrittenMessages.Count == y.WrittenMessages.Count &&
                             x.RemoteClients.Count == y.RemoteClients.Count;
            var sameBoundAddresses = x.BoundAddress.Equals(y.BoundAddress);

            if (sameCounts && sameBoundAddresses) // the easy stuff is all equal. Do the hard part.
            {
                return ClientsAreEqual(x.RemoteClients, y.RemoteClients)
                       && MessagesAreEqual(x.LastReceivedMessages, y.LastReceivedMessages)
                       && MessagesAreEqual(x.WrittenMessages, y.WrittenMessages);
            }

            return false;
        }

        public int GetHashCode(ITcpServerSocketModel obj)
        {
            return obj.GetHashCode();
        }

        public static bool ClientsAreEqual(IReadOnlyList<IPEndPoint> list1, IReadOnlyList<IPEndPoint> list2)
        {
            var set1 = new HashSet<IPEndPoint>(list1);
            var set2 = new HashSet<IPEndPoint>(list2);
            return set1.SetEquals(set2);
        }

        public static bool MessagesAreEqual(IReadOnlyList<int> list1, IReadOnlyList<int> list2)
        {
            return list1.OrderBy(x => x).SequenceEqual(list2.OrderBy(y => y));
        }
    }
}

