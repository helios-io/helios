// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Net;
using System.Net.Sockets;
using Helios.Topology;
using Helios.Util;
using Xunit;

namespace Helios.Tests.Topology
{
    public class NodeUriTests
    {
        #region Setup / Teardown

        #endregion

        #region Tests

        [Fact]
        public void Should_convert_valid_tcp_INode_to_NodeUri()
        {
            //arrange
            var testNode =
                NodeBuilder.BuildNode().Host(IPAddress.Loopback).WithPort(1337).WithTransportType(TransportType.Tcp);

            //act
            var nodeUri = new NodeUri(testNode);

            //assert
            Assert.Equal(testNode.Port, nodeUri.Port);
            Assert.Equal(testNode.Host.ToString(), nodeUri.Host);
            Assert.Equal("tcp", nodeUri.Scheme);
            Assert.True(nodeUri.IsLoopback);
        }

        [Fact]
        public void Should_convert_valid_ipv6_tcp_INode_to_NodeUri()
        {
            //TODO: does not work correctly on Mono
            if (MonotonicClock.IsMono) return;
            //arrange
            var testNode =
                NodeBuilder.BuildNode().Host(IPAddress.IPv6Loopback).WithPort(1337).WithTransportType(TransportType.Tcp);

            //act
            var nodeUri = new NodeUri(testNode);

            //assert
            Assert.Equal(testNode.Port, nodeUri.Port);
            Assert.Equal(string.Format("[{0}]", testNode.Host), nodeUri.Host);
            Assert.Equal("tcp", nodeUri.Scheme);
            Assert.True(nodeUri.IsLoopback);
        }

        [Fact]
        public void Should_convert_valid_tcp_NodeUri_to_INode()
        {
            //arrange
            var nodeUriStr = "tcp://127.0.0.1:1337/";
            var nodeUri = new Uri(nodeUriStr);

            //act
            var node = NodeUri.GetNodeFromUri(nodeUri);

            //assert
            Assert.NotNull(node);
            Assert.Equal(nodeUri.Host, node.Host.ToString());
            Assert.Equal(nodeUri.Port, node.Port);
            Assert.Equal(nodeUri.Scheme, NodeUri.GetProtocolStringForTransportType(node.TransportType));
        }

        [Fact]
        public void Should_convert_valid_UDP_INode_to_NodeUri()
        {
            //arrange
            var testNode =
                NodeBuilder.BuildNode().Host(IPAddress.Loopback).WithPort(1337).WithTransportType(TransportType.Udp);

            //act
            var nodeUri = new NodeUri(testNode);

            //assert
            Assert.Equal(testNode.Port, nodeUri.Port);
            Assert.Equal(testNode.Host.ToString(), nodeUri.Host);
            Assert.Equal("udp", nodeUri.Scheme);
            Assert.True(nodeUri.IsLoopback);
        }


        [Fact]
        public void Should_convert_valid_UDP_NodeUri_to_INode()
        {
            //arrange
            var nodeUriStr = "udp://127.0.0.1:1337/";
            var nodeUri = new Uri(nodeUriStr);

            //act
            var node = NodeUri.GetNodeFromUri(nodeUri);

            //assert
            Assert.NotNull(node);
            Assert.Equal(nodeUri.Host, node.Host.ToString());
            Assert.Equal(nodeUri.Port, node.Port);
            Assert.Equal(nodeUri.Scheme, NodeUri.GetProtocolStringForTransportType(node.TransportType));
        }

        #endregion
    }
}