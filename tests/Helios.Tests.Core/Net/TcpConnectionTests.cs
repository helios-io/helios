using System.Net;
using Helios.Exceptions;
using Helios.Net;
using Helios.Topology;
using NUnit.Framework;

namespace Helios.Tests.Net
{
    [TestFixture]
    public class TcpConnectionTests
    {
        #region Setup / Teardown
        #endregion

        #region Tests

        [ExpectedException(typeof(HeliosConnectionException))]
        [Test]
        public void Should_throw_exception_when_connecting_to_unreachable_node()
        {
            //arrange
            var node = NodeBuilder.BuildNode().Host(IPAddress.Loopback).WithPort(11111);
            var connection = node.GetConnection();

            //act
            connection.Open();

            //assert
        }

        #endregion
    }
}
