// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Helios.Buffers;
using Helios.Net;
using Helios.Reactor.Response;
using Helios.Serialization;
using Helios.Topology;

namespace Helios.Reactor
{
    /// <summary>
    ///     <see cref="IReactor" /> implementation which spawns <see cref="ReactorProxyResponseChannel" /> instances for
    ///     interacting directly with connected clients
    /// </summary>
    public abstract class ProxyReactorBase : ReactorBase
    {
        protected Dictionary<INode, ReactorResponseChannel> SocketMap = new Dictionary<INode, ReactorResponseChannel>();

        protected ProxyReactorBase(IPAddress localAddress, int localPort, NetworkEventLoop eventLoop,
            IMessageEncoder encoder, IMessageDecoder decoder, IByteBufAllocator allocator,
            SocketType socketType = SocketType.Stream, ProtocolType protocol = ProtocolType.Tcp,
            int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE)
            : base(localAddress, localPort, eventLoop, encoder, decoder, allocator, socketType, protocol, bufferSize)
        {
            BufferSize = bufferSize;
        }

        /// <summary>
        ///     If true, proxies created for each inbound connection share the parent's thread-pool. If false, each proxy is
        ///     allocated
        ///     its own thread pool.
        ///     Defaults to true.
        /// </summary>
        public bool ProxiesShareFiber { get; protected set; }

        protected override void ReceivedData(NetworkData availableData, ReactorResponseChannel responseChannel)
        {
            responseChannel.OnReceive(availableData);
        }
    }
}