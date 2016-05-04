// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Linq;
using System.Net;
using System.Text;
using Faker.Generators;
using Helios.MultiNodeTests.TestKit;
using Helios.Serialization;
using Xunit;

namespace Helios.MultiNodeTests
{
    /// <summary>
    ///     Tests to see how Helios can handle large messages
    /// </summary>
    public abstract class LargeMessageTests : MultiNodeTest
    {
        public override IMessageDecoder Decoder
        {
            get { return new LengthFieldFrameBasedDecoder(1024*5000, 0, 4, 0, 4); }
        }

        /// <summary>
        ///     Returns a Unicode-encoded string that will be equal to the desired byte size
        /// </summary>
        public static byte[] TestMessage(int byteSize)
        {
            var str = Strings.GenerateAlphaNumericString(byteSize/2, byteSize/2);
            return Encoding.Unicode.GetBytes(str);
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
            var message = TestMessage(1024*40);
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
            var message = TestMessage(1024*64);
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
            var message = TestMessage(1024*4000);
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

