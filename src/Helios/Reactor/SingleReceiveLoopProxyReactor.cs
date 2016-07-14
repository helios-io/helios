// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Helios.Buffers;
using Helios.Net;
using Helios.Reactor.Response;
using Helios.Serialization;

namespace Helios.Reactor
{
    /// <summary>
    /// <see cref="IReactor"/> implementation which spawns <see cref="ReactorProxyResponseChannel"/> instances for responding directly with connected clients,
    /// but maintains a single event loop for responding to incoming requests, rather than allowing each <see cref="ReactorProxyResponseChannel"/> to maintain
    /// its own independent event loop.
    /// 
    /// Great for scenarios where you want to be able to set a single event loop for a server and forget about it.
    /// </summary>
    public abstract class SingleReceiveLoopProxyReactor : ProxyReactorBase
    {
        protected SingleReceiveLoopProxyReactor(IPAddress localAddress, int localPort, NetworkEventLoop eventLoop,
            IMessageEncoder encoder, IMessageDecoder decoder, IByteBufAllocator allocator,
            SocketType socketType = SocketType.Stream, ProtocolType protocol = ProtocolType.Tcp,
            int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE)
            : base(localAddress, localPort, eventLoop, encoder, decoder, allocator, socketType, protocol, bufferSize)
        {
        }

        protected override void ReceivedData(NetworkData availableData, ReactorResponseChannel responseChannel)
        {
            if (EventLoop.Receive != null)
            {
                EventLoop.Receive(availableData, responseChannel);
            }
        }
    }
}