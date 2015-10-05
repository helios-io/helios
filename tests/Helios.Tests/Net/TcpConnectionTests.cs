using System.Net;
using System.Threading;
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

        //[ExpectedException(typeof(HeliosConnectionException))]
        [Test]
        public void Should_throw_exception_when_connecting_to_unreachable_node()
        {
            //arrange
            var node = NodeBuilder.BuildNode().Host(IPAddress.Loopback).WithPort(11111);
            var connection = node.GetConnection();
            var boolDisconnected = false;
            var resetEvent = new AutoResetEvent(false);
            connection.OnDisconnection += delegate(HeliosConnectionException reason, IConnection channel)
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

        #endregion
    }
}
