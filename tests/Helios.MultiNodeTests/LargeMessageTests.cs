using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Faker;
using Helios.MultiNodeTests.TestKit;
using Helios.Net;
using Helios.Serialization;
using NUnit.Framework;

namespace Helios.MultiNodeTests
{
    /// <summary>
    /// Tests to see how Helios can handle large messages
    /// </summary>
    [TestFixture]
    public abstract class LargeMessageTests : MultiNodeTest
    {
        /// <summary>
        /// Returns a Unicode-encoded string that will be equal to the desired byte size
        /// </summary>
        public static byte[] TestMessage(int byteSize)
        {
            var str = Faker.Generators.Strings.GenerateAlphaNumericString(byteSize/2, byteSize/2);
            return Encoding.Unicode.GetBytes(str);
        }

        public override IMessageDecoder Decoder
        {
            get { return new LengthFieldFrameBasedDecoder(1024*5000,0,4,0,4); }
        }

        [Test]
        public void Should_send_and_receive_10kb_Message()
        {
            //arrange
            StartServer(); //echo
            StartClient();
            var message = TestMessage(1024*10);
            var sends = 1;

            //act
            Send(message);
            WaitUntilNMessagesReceived(sends);

            //assert
            Assert.AreEqual(0, ClientExceptions.Length, "Did not expect to find any exceptions on client, instead found: {0}", ClientExceptions.Length);
            Assert.AreEqual(0, ServerExceptions.Length, "Did not expect to find any exceptions on Server, instead found: {0}", ServerExceptions.Length);
            Assert.AreEqual(sends, ClientSendBuffer.Count);
            Assert.AreEqual(sends, ClientReceiveBuffer.Count);

            var deliveredMessage = ClientReceiveBuffer.Dequeue();
            Assert.IsTrue(message.SequenceEqual(deliveredMessage.Buffer));
        }

        [Test]
        public void Should_send_and_receive_40kb_Message()
        {
            //arrange
            StartServer(); //echo
            StartClient();
            var message = TestMessage(1024 * 40);
            var sends = 1;

            //act
            Send(message);
            WaitUntilNMessagesReceived(sends);

            //assert
            Assert.AreEqual(0, ClientExceptions.Length, "Did not expect to find any exceptions on client, instead found: {0}", ClientExceptions.Length);
            Assert.AreEqual(0, ServerExceptions.Length, "Did not expect to find any exceptions on Server, instead found: {0}", ServerExceptions.Length);
            Assert.AreEqual(sends, ClientSendBuffer.Count);
            Assert.AreEqual(sends, ClientReceiveBuffer.Count);

            var deliveredMessage = ClientReceiveBuffer.Dequeue();
            Assert.IsTrue(message.SequenceEqual(deliveredMessage.Buffer));
        }

        [Test]
        public void Should_send_and_receive_64kb_Message()
        {
            //arrange
            StartServer(); //echo
            StartClient();
            var message = TestMessage(1024 * 64);
            var sends = 1;

            //act
            Send(message);
            WaitUntilNMessagesReceived(sends);

            //assert
            Assert.AreEqual(0, ClientExceptions.Length, "Did not expect to find any exceptions on client, instead found: {0}", ClientExceptions.Length);
            Assert.AreEqual(0, ServerExceptions.Length, "Did not expect to find any exceptions on Server, instead found: {0}", ServerExceptions.Length);
            Assert.AreEqual(sends, ClientSendBuffer.Count);
            Assert.AreEqual(sends, ClientReceiveBuffer.Count);

            var deliveredMessage = ClientReceiveBuffer.Dequeue();
            Assert.IsTrue(message.SequenceEqual(deliveredMessage.Buffer));
        }

        [Test]
        public void Should_send_and_receive_4000kb_Message()
        {
            //arrange
            StartServer(); //echo
            StartClient();
            var message = TestMessage(1024 * 4000);
            var sends = 1;

            //act
            Send(message);
            WaitUntilNMessagesReceived(sends, TimeSpan.FromSeconds(10));

            //assert
            Assert.AreEqual(0, ClientExceptions.Length, "Did not expect to find any exceptions on client, instead found: {0}", ClientExceptions.Length);
            Assert.AreEqual(0, ServerExceptions.Length, "Did not expect to find any exceptions on Server, instead found: {0}", ServerExceptions.Length);
            Assert.AreEqual(sends, ClientSendBuffer.Count);
            Assert.AreEqual(sends, ClientReceiveBuffer.Count);

            var deliveredMessage = ClientReceiveBuffer.Dequeue();
            Assert.IsTrue(message.SequenceEqual(deliveredMessage.Buffer));
        }
    }

    [TestFixture]
    public class TcpLargeMessageTests : LargeMessageTests
    {
        public override TransportType TransportType
        {
            get { return TransportType.Tcp; }
        }
    }

    [TestFixture(Ignore = true, IgnoreReason = "UDP can't handle such large packets at the protocol level")]
    public class UdpLargeMessageTests : LargeMessageTests
    {
        public override TransportType TransportType
        {
            get { return TransportType.Udp; }
        }
    }
}
