// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Net;
using System.Threading;
using Helios.Net;
using Helios.Topology;
using Xunit;

namespace Helios.Tests.Net
{
    public class TcpConnectionTests
    {
        #region Tests

        //[ExpectedException(typeof(HeliosConnectionException))]
        [Fact]
        public void Should_throw_exception_when_connecting_to_unreachable_node()
        {
            //arrange
            var node = NodeBuilder.BuildNode().Host(IPAddress.Loopback).WithPort(11111);
            var connection = node.GetConnection();
            var boolDisconnected = false;
            var resetEvent = new AutoResetEvent(false);
            connection.OnDisconnection += delegate
            {
                boolDisconnected = true;
                resetEvent.Set();
            };

            //act
            connection.Open();
            resetEvent.WaitOne();

            //assert
            Assert.True(boolDisconnected);
        }

        [Fact]
        public void Should_not_throw_exception_when_connecting_via_ipv6()
        {
            // arrange
            var node = NodeBuilder.BuildNode().Host(IPAddress.IPv6Loopback).WithPort(11111);
            var connection = node.GetConnection();
            var boolDisconnected = false;
            var resetEvent = new AutoResetEvent(false);
            connection.OnDisconnection += delegate
            {
                boolDisconnected = true;
                resetEvent.Set();
            };

            // act
            connection.Open();
            resetEvent.WaitOne();

            // assert
            Assert.True(boolDisconnected);
        }

        #endregion

        #region Setup / Teardown

        #endregion
    }
}

