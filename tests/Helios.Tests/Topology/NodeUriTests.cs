using System;
using System.Net;
using Helios.Topology;
using NUnit.Framework;

namespace Helios.Tests.Topology
{
    [TestFixture]
    public class NodeUriTests
    {
        #region Setup / Teardown



        #endregion

        #region Tests

        [Test]
        public void Should_convert_valid_tcp_INode_to_NodeUri()
        {
            //arrange
            var testNode =
                NodeBuilder.BuildNode().Host(IPAddress.Loopback).WithPort(1337).WithTransportType(TransportType.Tcp);

            //act
            var nodeUri = new NodeUri(testNode);

            //assert
            Assert.AreEqual(testNode.Port, nodeUri.Port);
            Assert.AreEqual(testNode.Host.ToString(), nodeUri.Host);
            Assert.AreEqual("tcp", nodeUri.Scheme);
            Assert.IsTrue(nodeUri.IsLoopback);
        }

        [Test]
        public void Should_convert_valid_tcp_NodeUri_to_INode()
        {
            //arrange
            var nodeUriStr = "tcp://127.0.0.1:1337/";
            var nodeUri = new Uri(nodeUriStr);

            //act
            var node = NodeUri.GetNodeFromUri(nodeUri);

            //assert
            Assert.IsNotNull(node);
            Assert.AreEqual(nodeUri.Host, node.Host.ToString());
            Assert.AreEqual(nodeUri.Port, node.Port);
            Assert.AreEqual(nodeUri.Scheme, NodeUri.GetProtocolStringForTransportType(node.TransportType));
        }

        [Test]
        public void Should_convert_valid_UDP_INode_to_NodeUri()
        {
            //arrange
            var testNode =
                NodeBuilder.BuildNode().Host(IPAddress.Loopback).WithPort(1337).WithTransportType(TransportType.Udp);

            //act
            var nodeUri = new NodeUri(testNode);

            //assert
            Assert.AreEqual(testNode.Port, nodeUri.Port);
            Assert.AreEqual(testNode.Host.ToString(), nodeUri.Host);
            Assert.AreEqual("udp", nodeUri.Scheme);
            Assert.IsTrue(nodeUri.IsLoopback);
        }

        [Test]
        public void Should_convert_valid_UDP_NodeUri_to_INode()
        {
            //arrange
            var nodeUriStr = "udp://127.0.0.1:1337/";
            var nodeUri = new Uri(nodeUriStr);

            //act
            var node = NodeUri.GetNodeFromUri(nodeUri);

            //assert
            Assert.IsNotNull(node);
            Assert.AreEqual(nodeUri.Host, node.Host.ToString());
            Assert.AreEqual(nodeUri.Port, node.Port);
            Assert.AreEqual(nodeUri.Scheme, NodeUri.GetProtocolStringForTransportType(node.TransportType));
        }

        #endregion
    }
}
