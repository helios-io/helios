// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using Helios.Reactor.Udp;
using Helios.Topology;

namespace Helios.Reactor.Bootstrap
{
    /// <summary>
    ///     <see cref="IServerFactory" /> instance for spawning UDP-based <see cref="IReactor" /> instances
    /// </summary>
    public sealed class UdpServerFactory : ServerFactoryBase
    {
        public UdpServerFactory(ServerBootstrap other) : base(other)
        {
        }

        protected override ReactorBase NewReactorInternal(INode listenAddress)
        {
            return new UdpProxyReactor(listenAddress.Host, listenAddress.Port, EventLoop, Encoder, Decoder, Allocator,
                BufferBytes);
        }
    }
}