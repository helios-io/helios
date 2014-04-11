using System.Net;
using Helios.Topology;
using NUnit.Framework;

namespace Helios.Tests.Topology
{
    [TestFixture]
    public class NodeTests
    {
        #region Setup / Teardown
        #endregion

        #region Tests

        [Test]
        public void Should_resolve_localhost_hostname_to_IP()
        {
            //arrange
            var testNode =
               NodeBuilder.BuildNode().Host("localhost").WithPort(1337).WithTransportType(TransportType.Udp);

            var expectNode =
                NodeBuilder.BuildNode().Host(IPAddress.Loopback).WithPort(1337).WithTransportType(TransportType.Udp);

            //act

            //assert
            Assert.AreEqual(expectNode, testNode);
        }

        [Test]
        public void Should_resolve_remote_hostname_to_IP()
        {
            //arrange
            var testNode =
                NodeBuilder.BuildNode().Host("yahoo.com").WithPort(80).WithTransportType(TransportType.Tcp);

            //act

            //assert
            Assert.IsNotNull(testNode.Host);
        }

        #endregion
    }
}