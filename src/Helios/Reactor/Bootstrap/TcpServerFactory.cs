// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using Helios.Reactor.Tcp;
using Helios.Topology;

namespace Helios.Reactor.Bootstrap
{
    /// <summary>
    ///     <see cref="IServerFactory" /> instance for spawning TCP-based <see cref="IReactor" /> instances
    /// </summary>
    public sealed class TcpServerFactory : ServerFactoryBase
    {
        public TcpServerFactory(ServerBootstrap other)
            : base(other)
        {
        }

        protected override ReactorBase NewReactorInternal(INode listenAddress)
        {
            if (UseProxies)
                return new TcpProxyReactor(listenAddress.Host, listenAddress.Port, EventLoop, Encoder, Decoder,
                    Allocator, BufferBytes);
            throw new NotImplementedException("Have not implemented non-TCP proxies");
        }
    }
}