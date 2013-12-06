using System.Net;
using Helios.Core.Exceptions;
using Helios.Core.Net;
using Helios.Core.Topology;
using NUnit.Framework;

namespace Helios.Tests.Core.Net
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
