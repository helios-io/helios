using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using Helios.MultiNodeTests.TestKit;
using Helios.Net;
using NUnit.Framework;

namespace Helios.MultiNodeTests
{
    [TestFixture]
    public abstract class ClientServerReplyTests : MultiNodeTest
    {

        [Test]
        public void Should_receive_reply_from_server_200b_messages()
        {
            //arrange
            StartServer((data, channel) => channel.Send(new NetworkData() { Buffer = data.Buffer, Length = data.Length, RemoteHost = channel.Local})); //echo
            StartClient();
            var messageLength = 200;
            var sends = 3;

            //act
            for (var i = 0; i < sends; i++)
            {
                Send(new byte[messageLength]);
            }
            WaitForDelivery();

            //assert
            Assert.AreEqual(sends, ClientSendBuffer.Count);
            Assert.AreEqual(sends, ClientReceiveBuffer.Count);
            var outsizedMessages = ClientReceiveBuffer.Select(x => x.Length != messageLength).ToList();
            Assert.IsTrue(ClientReceiveBuffer.DequeueAll().All(x => x.Length == messageLength));
            Assert.AreEqual(0, ClientExceptions.Length);
            Assert.AreEqual(0, ServerExceptions.Length);
        }

        [Test]
        public void Should_receive_reply_from_server_MAX_200b_messages()
        {
            //arrange
            StartServer((data, channel) => channel.Send(new NetworkData() {Buffer = data.Buffer, Length = data.Length, RemoteHost = channel.Local})); //echo
            StartClient();
            var messageLength = 200;
            var sends = BufferSize;

            //act
            for (var i = 0; i < sends; i++)
            {
                Send(new byte[messageLength]);
            }
            WaitForDelivery();

            //assert
            Assert.AreEqual(sends, ClientSendBuffer.Count);
            Assert.AreEqual(sends, ClientReceiveBuffer.Count);
            var outsizedMessages = ClientReceiveBuffer.Where(x => x.Length != messageLength).ToList();
            Assert.IsTrue(ClientReceiveBuffer.DequeueAll().All(x => x.Length == messageLength));
            Assert.AreEqual(0, ClientExceptions.Length);
            Assert.AreEqual(0, ServerExceptions.Length);
        }
    }

    [TestFixture]
    public class TcpClientServerReplyTests : ClientServerReplyTests
    {
        public override TransportType TransportType
        {
            get { return TransportType.Tcp; }
        }
    }

    [TestFixture]
    public class UdpClientServerReplyTests : ClientServerReplyTests
    {
        public override TransportType TransportType
        {
            get { return TransportType.Udp; }
        }
    }
}
