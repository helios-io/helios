// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Net;
using System.Net.Sockets;
using Helios.Channels;
using Helios.Net;

namespace Helios.Reactor.Response
{
    /// <summary>
    ///     Response channel receives all of its events directly from the <see cref="ReactorBase" /> and doesn't maintain any
    ///     internal buffers,
    ///     nor does it directly interact with its socket in any way
    /// </summary>
    public class ReactorProxyResponseChannel : ReactorResponseChannel
    {
        public ReactorProxyResponseChannel(ReactorBase reactor, Socket outboundSocket, NetworkEventLoop eventLoop)
            : base(reactor, outboundSocket, eventLoop)
        {
        }

        public ReactorProxyResponseChannel(ReactorBase reactor, Socket outboundSocket, IPEndPoint endPoint,
            NetworkEventLoop eventLoop)
            : base(reactor, outboundSocket, endPoint, eventLoop)
        {
        }

        public override void Configure(IConnectionConfig config)
        {
        }

        protected override void BeginReceiveInternal()
        {
        }

        protected override void StopReceiveInternal()
        {
        }
    }
}