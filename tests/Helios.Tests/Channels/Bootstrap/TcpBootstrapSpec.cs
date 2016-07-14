// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Helios.Channels;
using Helios.Channels.Bootstrap;
using Helios.Channels.Sockets;
using Xunit;

namespace Helios.Tests.Channels.Bootstrap
{
    public class TcpBootstrapSpec : IDisposable
    {
        private MultithreadEventLoopGroup _serverGroup = new MultithreadEventLoopGroup(1);

        [Fact]
        public void ServerBootrap_must_support_BindAsync_on_DnsEndpoints_for_SocketChannels()
        {
            var sb = new ServerBootstrap()
                .Channel<TcpServerSocketChannel>()
                .ChildHandler(new ActionChannelInitializer<TcpSocketChannel>(channel => { }))
                .Group(_serverGroup);

            var c = sb.BindAsync(new DnsEndPoint("localhost", 0)).Result;
        }

        public void Dispose()
        {
            _serverGroup.ShutdownGracefullyAsync();
        }
    }
}