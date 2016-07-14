// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Net;

namespace Helios.Topology
{
    /// <summary>
    ///     Special case pattern - uses an Empty node to denote when an item is local, rather than networked.
    /// </summary>
    public class EmptyNode : INode
    {
        private IPEndPoint _endPoint;
        public IPAddress Host { get; set; }
        public int Port { get; set; }
        public TransportType TransportType { get; set; }
        public string MachineName { get; set; }
        public string OS { get; set; }
        public string ServiceVersion { get; set; }
        public string CustomData { get; set; }

        public IPEndPoint ToEndPoint()
        {
            return _endPoint ?? (_endPoint = new IPEndPoint(IPAddress.None, 0));
        }

        public object Clone()
        {
            return Node.Empty();
        }
    }
}