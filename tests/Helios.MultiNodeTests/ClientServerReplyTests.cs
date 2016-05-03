using System.Linq;
using System.Net;
using Helios.Channels;
using Helios.MultiNodeTests.TestKit;
using Helios.Net;
using Xunit;

namespace Helios.MultiNodeTests
{
    public abstract class ClientServerReplyTests : MultiNodeTest
    {

        [Fact]
        public virtual void Should_receive_reply_from_server_200b_messages()
        {
            //arrange
            StartServer(); //uses an "echo server" callback
            StartClient();
            var messageLength = 200;
            var sends = 3;

            //act
            for (var i = 0; i < sends; i++)
            {
                Send(new byte[messageLength]);
            }
            WaitUntilNMessagesReceived(sends);

            //assert
            Assert.Equal(0, ClientExceptions.Length); // "Did not expect to find any exceptions on client, instead found: {0}", ClientExceptions.Length
            Assert.Equal(0, ServerExceptions.Length); // "Did not expect to find any exceptions on Server, instead found: {0}", ServerExceptions.Length)
            Assert.Equal(sends, ClientSendBuffer.Count);
            Assert.Equal(sends, ClientReceiveBuffer.Count);
            var outsizedMessages = ClientReceiveBuffer.Select(x => x.Length != messageLength).ToList();
            Assert.True(ClientReceiveBuffer.DequeueAll().All(x => x.Length == messageLength));
            
        }

        [Fact]
        public virtual void Should_receive_reply_from_server_MAX_200b_messages()
        {
            //arrange
            StartServer(); //echo
            StartClient();
            var messageLength = 200;
            var sends = BufferSize;

            //act
            for (var i = 0; i < sends; i++)
            {
                Send(new byte[messageLength]);
            }
            WaitUntilNMessagesReceived(sends);

            //assert
            Assert.Equal(0, ClientExceptions.Length); // "Did not expect to find any exceptions on client, instead found: {0}", ClientExceptions.Length
            Assert.Equal(0, ServerExceptions.Length); // "Did not expect to find any exceptions on Server, instead found: {0}", ServerExceptions.Length
            Assert.Equal(sends, ClientSendBuffer.Count);
            Assert.Equal(sends, ClientReceiveBuffer.Count);
            Assert.True(ClientReceiveBuffer.DequeueAll().All(x => x.Length == messageLength));
        }
    }

    public class TcpClientServerReplyTests : ClientServerReplyTests
    {
        public override TransportType TransportType
        {
            get { return TransportType.Tcp; }
        }
    }

    public class UdpClientServerReplyTests : ClientServerReplyTests
    {
        public override TransportType TransportType
        {
            get { return TransportType.Udp; }
        }

        public override IConnectionConfig Config
        {
            get { return base.Config.SetOption("receiveBufferSize", 1024*64); }
        }

        [Fact(Skip = "UDP has unreliable delivery")]
        public override void Should_receive_reply_from_server_MAX_200b_messages()
        {
            base.Should_receive_reply_from_server_MAX_200b_messages();
        }
    }
}
