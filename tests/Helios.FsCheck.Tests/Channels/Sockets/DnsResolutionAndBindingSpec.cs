// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Net;
using System.Net.Sockets;
using FsCheck.Xunit;
using FsCheck;
using Helios.Buffers;
using Helios.Channels;
using Helios.Channels.Bootstrap;
using Helios.Channels.Sockets;
using Helios.Util;
using Xunit;

namespace Helios.FsCheck.Tests.Channels.Sockets
{
    public enum IpMapping
    {
        AnyIpv6,
        AnyIpv4,
        LoopbackIpv6,
        LoopbackIpv4,
        Localhost
    }


    /// <summary>
    /// Generates a range of random options for DNS
    /// </summary>
    public static class EndpointGenerators
    {
        public static Arbitrary<EndPoint> Endpoints()
        {
            return Arb.From(Gen.Elements<EndPoint>(new IPEndPoint(IPAddress.Loopback, 0),
                new IPEndPoint(IPAddress.Any, 0),
                new IPEndPoint(IPAddress.IPv6Loopback, 0),
                new IPEndPoint(IPAddress.IPv6Any, 0),
                new DnsEndPoint("localhost", 0)));
        }
    }

    public class DnsResolutionAndBindingSpec : IDisposable
    {
        public DnsResolutionAndBindingSpec()
        {
            Arb.Register(typeof(EndpointGenerators));
        }

        public static readonly bool IsMono = MonotonicClock.IsMono;

        public static EndPoint MappingToEndpoint(IpMapping map)
        {
            switch (map)
            {
                case IpMapping.AnyIpv4:
                    return new IPEndPoint(IPAddress.Any, 0);
                case IpMapping.AnyIpv6:
                    return new IPEndPoint(IPAddress.IPv6Any, 0);
                case IpMapping.LoopbackIpv4:
                    return new IPEndPoint(IPAddress.Loopback, 0);
                case IpMapping.LoopbackIpv6:
                    return new IPEndPoint(IPAddress.IPv6Loopback, 0);
                case IpMapping.Localhost:
                default:
                    return new DnsEndPoint("localhost", 0);
            }
        }

        private MultithreadEventLoopGroup _serverGroup = new MultithreadEventLoopGroup(1);
        private MultithreadEventLoopGroup _clientGroup = new MultithreadEventLoopGroup(1);

        [Theory]
        [InlineData(IpMapping.AnyIpv4, IpMapping.Localhost, AddressFamily.InterNetwork)]
        [InlineData(IpMapping.AnyIpv6, IpMapping.Localhost, AddressFamily.InterNetwork)]
        //[InlineData(IpMapping.AnyIpv4, IpMapping.LoopbackIpv6, AddressFamily.InterNetworkV6)] //not valid; will fail
        [InlineData(IpMapping.AnyIpv4, IpMapping.LoopbackIpv4, AddressFamily.InterNetwork)]
        [InlineData(IpMapping.AnyIpv6, IpMapping.LoopbackIpv6, AddressFamily.InterNetwork)]
        [InlineData(IpMapping.AnyIpv6, IpMapping.LoopbackIpv4, AddressFamily.InterNetwork)]
        [InlineData(IpMapping.LoopbackIpv4, IpMapping.Localhost, AddressFamily.InterNetwork)]
        [InlineData(IpMapping.LoopbackIpv6, IpMapping.Localhost, AddressFamily.InterNetworkV6)]
        public void TcpSocketServerChannel_can_be_connected_to_on_any_aliased_Endpoint(IpMapping actual,
            IpMapping alias, AddressFamily family)
        {
            var inboundActual = MappingToEndpoint(actual);
            var inboundAlias = MappingToEndpoint(alias);
            var isIp = inboundAlias is IPEndPoint;

            if (IsMono && family == AddressFamily.InterNetworkV6)
            {
                Assert.True(true, "Mono currently does not support IPV6 in DNS resolution");
                return;
            }


            IChannel s = null;
            IChannel c = null;
            try
            {
                var sb = new ServerBootstrap()
                    .ChannelFactory(() => new TcpServerSocketChannel())
                    .PreferredDnsResolutionFamily(family)
                    .ChildHandler(new ActionChannelInitializer<TcpSocketChannel>(channel => { }))
                    .Group(_serverGroup);

                s = sb.BindAsync(inboundActual).Result;


                var cb = new ClientBootstrap()
                    .ChannelFactory(() => new TcpSocketChannel())
                    .Option(ChannelOption.TcpNodelay, true)
                    .Option(ChannelOption.ConnectTimeout, TimeSpan.FromMilliseconds(100))
                    .PreferredDnsResolutionFamily(family)
                    .Handler(new ActionChannelInitializer<TcpSocketChannel>(channel => { }))
                    .Group(_clientGroup);

                EndPoint clientEp = isIp
                    ? new IPEndPoint(((IPEndPoint) inboundAlias).Address, ((IPEndPoint) s.LocalAddress).Port)
                    : (EndPoint) new DnsEndPoint(((DnsEndPoint) inboundAlias).Host, ((IPEndPoint) s.LocalAddress).Port);

                c = cb.ConnectAsync(clientEp).Result;
                c.WriteAndFlushAsync(Unpooled.Buffer(4).WriteInt(2)).Wait(20);

                Assert.True(c.IsOpen);
                Assert.True(c.IsActive);
                Assert.True(c.IsWritable);
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

        [Property]
        public Property TcpSocketServerChannel_can_bind_on_any_valid_EndPoint(EndPoint ep)
        {
            IChannel c = null;
            var family = ep.BestAddressFamily();

            // TODO: remove this code once https://bugzilla.xamarin.com/show_bug.cgi?id=35536 is fixed
            if (IsMono && family == AddressFamily.InterNetworkV6 && ep is DnsEndPoint)
            {
                family = AddressFamily.InterNetwork;
            }

            try
            {
                var sb = new ServerBootstrap()
                    .ChannelFactory(() => new TcpServerSocketChannel(family))
                    .ChildHandler(new ActionChannelInitializer<TcpSocketChannel>(channel => { }))
                    .PreferredDnsResolutionFamily(family)
                    .Group(_serverGroup);

                c = sb.BindAsync(ep).Result;

                return c.IsOpen.Label("Channel should be open").And(c.IsActive).Label("Channel should be active");
            }
            finally
            {
                c?.CloseAsync().Wait(TimeSpan.FromMilliseconds(200));
            }
        }

        [Property]
        public Property TcpServerSocketChannel_can_accept_connection_on_any_valid_Endpoint(EndPoint ep)
        {
            var ip = ep as IPEndPoint;
            var family = ep.BestAddressFamily();

            // TODO: remove this code once https://bugzilla.xamarin.com/show_bug.cgi?id=35536 is fixed

            if (IsMono && family == AddressFamily.InterNetworkV6 && ep is DnsEndPoint)
            {
                family = AddressFamily.InterNetwork;
            }

            IChannel s = null;
            IChannel c = null;
            try
            {
                var sb = new ServerBootstrap()
                    .ChannelFactory(() => new TcpServerSocketChannel(family))
                    .ChildHandler(new ActionChannelInitializer<TcpSocketChannel>(channel => { }))
                    .PreferredDnsResolutionFamily(family)
                    .Group(_serverGroup);

                s = sb.BindAsync(ep).Result;

                var cb = new ClientBootstrap()
                    .ChannelFactory(() => new TcpSocketChannel(family))
                    .Option(ChannelOption.TcpNodelay, true)
                    .Option(ChannelOption.ConnectTimeout, TimeSpan.FromMilliseconds(100))
                    .PreferredDnsResolutionFamily(family)
                    .Handler(new ActionChannelInitializer<TcpSocketChannel>(channel => { }))
                    .Group(_clientGroup);

                var clientEp = s.LocalAddress;
                if (ip != null) // handle special case of 0.0.0.0, which clients can't connect to directly.
                {
                    if (ip.Address.Equals(IPAddress.Any))
                        clientEp = new IPEndPoint(IPAddress.Loopback, ((IPEndPoint) s.LocalAddress).Port);
                    if (ip.Address.Equals(IPAddress.IPv6Any))
                        clientEp = new IPEndPoint(IPAddress.IPv6Loopback, ((IPEndPoint) s.LocalAddress).Port);
                }

                c = cb.ConnectAsync(clientEp).Result;
                c.WriteAndFlushAsync(Unpooled.Buffer(4).WriteInt(2)).Wait(20);

                return c.IsOpen.Label("Channel should be open")
                    .And(c.IsActive).Label("Channel should be active")
                    .And(c.IsWritable).Label("Channel should be writable");
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