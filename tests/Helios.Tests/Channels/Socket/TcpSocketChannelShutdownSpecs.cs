// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Helios.Buffers;
using Helios.Channels;
using Helios.Channels.Bootstrap;
using Helios.Channels.Sockets;
using Helios.Util;
using Xunit;

namespace Helios.Tests.Channels.Socket
{
    public class TcpSocketChannelShutdownSpecs : IDisposable
    {
        public class FailAssertionHandler : ChannelHandlerAdapter
        {
            private AtomicReference<Exception> _ex;

            public FailAssertionHandler(AtomicReference<Exception> ex)
            {
                _ex = ex;
            }

            public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
            {
                _ex.Value = exception;
            }
        }

        private MultithreadEventLoopGroup _serverGroup = new MultithreadEventLoopGroup(1);
        private MultithreadEventLoopGroup _clientGroup = new MultithreadEventLoopGroup(1);

        [Fact]
        public async Task TcpSocketChannel_should_not_throw_on_shutdown()
        {
            AtomicReference<Exception> clientError = new AtomicReference<Exception>(null);
            AtomicReference<Exception> serverError = new AtomicReference<Exception>(null);

            IChannel s = null;
            IChannel c = null;
            try
            {
                var sb = new ServerBootstrap()
                    .Channel<TcpServerSocketChannel>()
                    .ChildHandler(
                        new ActionChannelInitializer<TcpSocketChannel>(
                            channel => { channel.Pipeline.AddLast(new FailAssertionHandler(serverError)); }))
                    .Group(_serverGroup);

                s = sb.BindAsync(new IPEndPoint(IPAddress.IPv6Loopback, 0)).Result;


                var cb = new ClientBootstrap()
                    .Channel<TcpSocketChannel>()
                    .Option(ChannelOption.TcpNodelay, true)
                    .Option(ChannelOption.ConnectTimeout, TimeSpan.FromMilliseconds(100))
                    .Handler(
                        new ActionChannelInitializer<TcpSocketChannel>(
                            channel => { channel.Pipeline.AddLast(new FailAssertionHandler(clientError)); }))
                    .Group(_clientGroup);

                var clientEp = s.LocalAddress;
                c = cb.ConnectAsync(clientEp).Result;
                c.WriteAndFlushAsync(Unpooled.Buffer(4).WriteInt(2)).Wait(20);
                Assert.True(c.IsOpen);
                await c.CloseAsync();

                // assert that the counters were never decremented
                Assert.True(clientError.Value == null,
                    $"Expected null but error on client was {clientError.Value?.Message} {clientError.Value?.StackTrace}");
                Assert.True(serverError.Value == null,
                    $"Expected null but error on server was {serverError.Value?.Message} {serverError.Value?.StackTrace}");
            }
            finally
            {
                try
                {
                    c?.CloseAsync().Wait(TimeSpan.FromMilliseconds(200));
                    s?.CloseAsync().Wait(TimeSpan.FromMilliseconds(200));
                }
                catch
                {
                }
            }
        }

        public void Dispose()
        {
            _serverGroup.ShutdownGracefullyAsync();
            _clientGroup.ShutdownGracefullyAsync();
        }
    }
}