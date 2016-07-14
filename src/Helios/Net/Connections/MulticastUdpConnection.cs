// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Net.Sockets;
using Helios.Buffers;
using Helios.Serialization;
using Helios.Topology;

namespace Helios.Net.Connections
{
    /// <summary>
    ///     Multi-cast implementation of a UDP
    /// </summary>
    public class MulticastUdpConnection : UdpConnection
    {
        public MulticastUdpConnection(NetworkEventLoop eventLoop, INode binding, INode multicastAddress,
            TimeSpan timeout, IMessageEncoder encoder, IMessageDecoder decoder, IByteBufAllocator allocator)
            : base(eventLoop, binding, timeout, encoder, decoder, allocator)
        {
            MulticastAddress = multicastAddress;
            InitMulticastClient();
        }

        public MulticastUdpConnection(NetworkEventLoop eventLoop, INode binding, INode multicastAddress,
            IMessageEncoder encoder, IMessageDecoder decoder, IByteBufAllocator allocator)
            : this(
                eventLoop, binding, multicastAddress, NetworkConstants.DefaultConnectivityTimeout, encoder, decoder,
                allocator)
        {
        }

        public MulticastUdpConnection(Socket client, IMessageEncoder encoder, IMessageDecoder decoder,
            IByteBufAllocator allocator)
            : base(client)
        {
            InitMulticastClient();
            Encoder = encoder;
            Decoder = decoder;
            Allocator = allocator;
        }

        public MulticastUdpConnection(Socket client)
            : this(client, Encoders.DefaultEncoder, Encoders.DefaultDecoder, UnpooledByteBufAllocator.Default)
        {
        }

        public INode MulticastAddress { get; protected set; }

        protected void InitMulticastClient()
        {
            if (Client == null)
                InitClient();
// ReSharper disable once PossibleNullReferenceException
            if (Client.AddressFamily == AddressFamily.InterNetwork)
                Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership,
                    new MulticastOption(MulticastAddress.Host));
            else
                Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.AddMembership,
                    new IPv6MulticastOption(MulticastAddress.Host));
        }
    }
}