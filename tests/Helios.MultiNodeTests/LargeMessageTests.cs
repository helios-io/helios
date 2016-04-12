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
using Xunit;

namespace Helios.MultiNodeTests
{
    /// <summary>
    /// Tests to see how Helios can handle large messages
    /// </summary>
    
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

        [Fact]
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
            Assert.Equal(0, ClientExceptions.Length);
            Assert.Equal(0, ServerExceptions.Length);
            Assert.Equal(sends, ClientSendBuffer.Count);
            Assert.Equal(sends, ClientReceiveBuffer.Count);

            var deliveredMessage = ClientReceiveBuffer.Dequeue();
            Assert.True(message.SequenceEqual(deliveredMessage.Buffer));
        }

        [Fact]
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
            Assert.Equal(0, ClientExceptions.Length);
            Assert.Equal(0, ServerExceptions.Length);
            Assert.Equal(sends, ClientSendBuffer.Count);
            Assert.Equal(sends, ClientReceiveBuffer.Count);

            var deliveredMessage = ClientReceiveBuffer.Dequeue();
            Assert.True(message.SequenceEqual(deliveredMessage.Buffer));
        }

        [Fact]
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
            Assert.Equal(0, ClientExceptions.Length);
            Assert.Equal(0, ServerExceptions.Length);
            Assert.Equal(sends, ClientSendBuffer.Count);
            Assert.Equal(sends, ClientReceiveBuffer.Count);

            var deliveredMessage = ClientReceiveBuffer.Dequeue();
            Assert.True(message.SequenceEqual(deliveredMessage.Buffer));
        }

        [Fact]
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
            Assert.Equal(0, ClientExceptions.Length);
            Assert.Equal(0, ServerExceptions.Length);
            Assert.Equal(sends, ClientSendBuffer.Count);
            Assert.Equal(sends, ClientReceiveBuffer.Count);

            var deliveredMessage = ClientReceiveBuffer.Dequeue();
            Assert.True(message.SequenceEqual(deliveredMessage.Buffer));
        }
    }

    
    public class TcpLargeMessageTests : LargeMessageTests
    {
        public override TransportType TransportType
        {
            get { return TransportType.Tcp; }
        }
    }

    //public class UdpLargeMessageTests : LargeMessageTests
    //{
    //    public override TransportType TransportType
    //    {
    //        get { return TransportType.Udp; }
    //    }
    //}
}
