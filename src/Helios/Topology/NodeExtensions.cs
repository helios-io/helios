// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Net;

namespace Helios.Topology
{
    /// <summary>
    ///     Extension methods to make it easier to work with INode implementations
    /// </summary>
    public static class NodeExtensions
    {
        public static INode ToNode(this IPEndPoint endPoint, TransportType transportType)
        {
            return new Node {Host = endPoint.Address, Port = endPoint.Port, TransportType = transportType};
        }

        public static Uri ToUri(this INode node)
        {
            return new NodeUri(node);
        }

#if !NET35 && !NET40
        public static INode ToNode(this Uri uri)
        {
            return NodeUri.GetNodeFromUri(uri);
        }
#endif

        public static bool IsEmpty(this INode node)
        {
            return node is EmptyNode;
        }
    }
}