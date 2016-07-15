// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Net;
using System.Net.Sockets;
using Helios.Buffers;
using Helios.Topology;

namespace Helios.Net
{
    /// <summary>
    /// Data arrived via a remote host - used to help provide a common interface
    /// on our IConnection members
    /// </summary>
    [Obsolete()]
    public struct NetworkData
    {
        public INode RemoteHost { get; set; }

        public DateTime Recieved { get; set; }

        public byte[] Buffer { get; set; }

        public int Length { get; set; }
        public static NetworkData Empty = new NetworkData() {Length = 0, RemoteHost = Node.Empty()};


        public static NetworkData Create(INode node, byte[] data, int bytes)
        {
            return new NetworkData()
            {
                Buffer = data,
                Length = bytes,
                RemoteHost = node
            };
        }

        public static NetworkData Create(INode node, IByteBuf buf)
        {
            var readableBytes = buf.ReadableBytes;
            return new NetworkData()
            {
                Buffer = buf.ToArray(),
                Length = readableBytes,
                RemoteHost = node
            };
        }

#if !NET35 && !NET40 && !NET40
        public static NetworkData Create(UdpReceiveResult receiveResult)
        {
            return new NetworkData()
            {
                Buffer = receiveResult.Buffer,
                Length = receiveResult.Buffer.Length,
                RemoteHost = receiveResult.RemoteEndPoint.ToNode(TransportType.Udp)
            };
        }
#endif
    }
}