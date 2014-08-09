using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Helios.MultiNodeTests.TestKit;
using Helios.Net;
using NUnit.Framework;

namespace Helios.MultiNodeTests
{
    [TestFixture]
    public class ClientServerReplyTests : MultiNodeTest
    {
        public override TransportType TransportType
        {
            get { return TransportType.Tcp; }
        }

        [Test]
        public void Should_receive_reply_from_server_200b()
        {
            //arrange
            StartServer((data, channel) => channel.Send(new NetworkData() { Buffer = data.Buffer, Length = data.Length, RemoteHost = channel.Local})); //echo
            StartClient();

            //act
            var sends = 3;
            for (var i = 0; i < sends; i++)
            {
                Send(new byte[200]);
            }
            WaitForDelivery();

            //assert
            Assert.AreEqual(sends, ClientSendBuffer.Count);
            Assert.AreEqual(sends, ClientReceiveBuffer.Count);
        }
    }
}
