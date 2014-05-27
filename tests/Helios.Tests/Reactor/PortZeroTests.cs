using System.Net;
using Helios.Net;
using Helios.Net.Bootstrap;
using Helios.Reactor.Bootstrap;
using Helios.Topology;
using NUnit.Framework;

namespace Helios.Tests.Reactor
{
    /// <summary>
    /// Tests to ensure that we can bind to port zero on servers and successfully retrieve the system-assigned IP address
    /// </summary>
    [TestFixture]
    public class PortZeroTests
    {

        [Test]
        public void TcpProxyServer_should_bind_to_ephemeral_port()
        {
            var server =
                new ServerBootstrap().SetTransport(TransportType.Tcp).Build().NewReactor(NodeBuilder.BuildNode().Host(IPAddress.Any).WithPort(0));
            server.Start();
            Assert.AreNotEqual(0, server.LocalEndpoint.Port);
            server.Stop();
        }


        [Test]
        public void TcpProxyServer_connection_adapter_should_bind_to_ephemeral_port()
        {
            var server =
                new ServerBootstrap().SetTransport(TransportType.Tcp).Build().NewReactor(NodeBuilder.BuildNode().Host(IPAddress.Any).WithPort(0)).ConnectionAdapter;
            server.Open();
            Assert.AreNotEqual(0, server.Local.Port);
            server.Close();
        }

        [Test]
        public void UdpProxyServer_should_bind_to_ephemeral_port()
        {
            var server =
                new ServerBootstrap().SetTransport(TransportType.Udp).Build().NewReactor(NodeBuilder.BuildNode().Host(IPAddress.Any).WithPort(0));
            server.Start();
            Assert.AreNotEqual(0, server.LocalEndpoint.Port);
            server.Stop();
        }

        [Test]
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
            Assert.AreNotEqual(0, connection.Local.Port);
            connection.Close();
            server.Stop();
        }

        [Test]
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
            Assert.AreNotEqual(0, connection.Local.Port);
            connection.Close();
            server.Stop();
        }
    }
}
