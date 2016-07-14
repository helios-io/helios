// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Net;
using Helios.Buffers;
using Helios.Exceptions;
using Helios.Net.Connections;
using Helios.Ops;
using Helios.Serialization;
using Helios.Topology;

namespace Helios.Net.Builders
{
    public class NormalConnectionBuilder : IConnectionBuilder
    {
        public NormalConnectionBuilder() : this(NetworkConstants.DefaultConnectivityTimeout)
        {
        }

        public NormalConnectionBuilder(TimeSpan timeout)
        {
            Timeout = timeout;
        }

        public TimeSpan Timeout { get; }

        public IConnection BuildConnection(INode node)
        {
            switch (node.TransportType)
            {
                case TransportType.Tcp:
                    return new TcpConnection(EventLoopFactory.CreateNetworkEventLoop(), node, Timeout,
                        Encoders.DefaultEncoder, Encoders.DefaultDecoder, UnpooledByteBufAllocator.Default);
                case TransportType.Udp:
                    return new UdpConnection(EventLoopFactory.CreateNetworkEventLoop(), node, Timeout,
                        Encoders.DefaultEncoder, Encoders.DefaultDecoder, UnpooledByteBufAllocator.Default);
                default:
                    throw new HeliosConnectionException(ExceptionType.NotSupported,
                        "No support for non-UDP / TCP connections at this time.");
            }
        }

        public IConnection BuildMulticastConnection(INode bindNode, INode multicastNode)
        {
            if (MulticastHelper.IsValidMulticastAddress(multicastNode.Host))
                return new MulticastUdpConnection(EventLoopFactory.CreateNetworkEventLoop(), bindNode, multicastNode,
                    Encoders.DefaultEncoder, Encoders.DefaultDecoder, UnpooledByteBufAllocator.Default);

            throw new HeliosConnectionException(ExceptionType.NotSupported,
                string.Format("{0} is an invalid multicast IP address", multicastNode.Host));
        }
    }
}