// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Net;
using Helios.Net.Bootstrap;
using Helios.Reactor.Bootstrap;
using Helios.Topology;
using Xunit;

namespace Helios.Tests.Reactor
{
    /// <summary>
    ///     Tests to ensure that we can bind to port zero on servers and successfully retrieve the system-assigned IP address
    /// </summary>
    public class PortZeroTests
    {
        [Fact]
        public void TcpProxyServer_should_bind_to_ephemeral_port()
        {
            var server =
                new ServerBootstrap().SetTransport(TransportType.Tcp)
                    .Build()
                    .NewReactor(NodeBuilder.BuildNode().Host(IPAddress.Any).WithPort(0));
            server.Start();
            Assert.NotEqual(0, server.LocalEndpoint.Port);
            server.Stop();
        }


        [Fact]
        public void TcpProxyServer_connection_adapter_should_bind_to_ephemeral_port()
        {
            var server =
                new ServerBootstrap().SetTransport(TransportType.Tcp)
                    .Build()
                    .NewReactor(NodeBuilder.BuildNode().Host(IPAddress.Any).WithPort(0))
                    .ConnectionAdapter;
            server.Open();
            Assert.NotEqual(0, server.Local.Port);
            server.Close();
        }

        [Fact]
        public void UdpProxyServer_should_bind_to_ephemeral_port()
        {
            var server =
                new ServerBootstrap().SetTransport(TransportType.Udp)
                    .Build()
                    .NewReactor(NodeBuilder.BuildNode().Host(IPAddress.Any).WithPort(0));
            server.Start();
            Assert.NotEqual(0, server.LocalEndpoint.Port);
            server.Stop();
        }

        [Fact]
        public void TcpConnection_should_bind_to_outbound_ephemeral_port()
        {
            var serverAddress = NodeBuilder.BuildNode().Host(IPAddress.Loopback).WithPort(13171);
            var server =
                new ServerBootstrap().SetTransport(TransportType.Tcp).Build().NewReactor(serverAddress);
            var connection = new ClientBootstrap().SetTransport(TransportType.Tcp)
                .Build()
                .NewConnection(Node.Empty(), serverAddress);
            server.Start();
            connection.Open();
            Assert.NotEqual(0, connection.Local.Port);
            connection.Close();
            server.Stop();
        }

        [Fact]
        public void UdpConnection_should_bind_to_outbound_ephemeral_port()
        {
            var serverAddress = NodeBuilder.BuildNode().Host(IPAddress.Loopback).WithPort(13171);
            var server =
                new ServerBootstrap().SetTransport(TransportType.Udp).Build().NewReactor(serverAddress);
            var connection = new ClientBootstrap().SetTransport(TransportType.Udp)
                .Build()
                .NewConnection(Node.Loopback(), serverAddress);
            server.Start();
            connection.Open();
            Assert.NotEqual(0, connection.Local.Port);
            connection.Close();
            server.Stop();
        }
    }
}

