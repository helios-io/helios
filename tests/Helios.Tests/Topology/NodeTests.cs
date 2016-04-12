using System.Net;
using Helios.Topology;
using Xunit;

namespace Helios.Tests.Topology
{
    
    public class NodeTests
    {
        #region Setup / Teardown
        #endregion

        #region Tests

        [Fact]
        public void Should_resolve_localhost_hostname_to_IP()
        {
            //arrange
            var testNode =
               NodeBuilder.BuildNode().Host("localhost").WithPort(1337).WithTransportType(TransportType.Udp);

            var expectNode =
                NodeBuilder.BuildNode().Host(IPAddress.Loopback).WithPort(1337).WithTransportType(TransportType.Udp);

            //act

            //assert
            Assert.Equal(expectNode, testNode);
        }

        [Fact]
        public void Should_resolve_remote_hostname_to_IP()
        {
            //arrange
            var testNode =
                NodeBuilder.BuildNode().Host("yahoo.com").WithPort(80).WithTransportType(TransportType.Tcp);

            //act

            //assert
            Assert.NotNull(testNode.Host);
        }

        #endregion
    }
}