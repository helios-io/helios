using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Helios.Buffers;
using Helios.Channels;
using Helios.Channels.Bootstrap;
using Helios.Channels.Sockets;
using Helios.Codecs;
using Helios.Net;
using Helios.Reactor;
using Helios.Topology;
using Helios.Util;
using Xunit;

namespace Helios.BackwardsCompat.Tests.Tcp
{
    public class EchoHandler : ChannelHandlerAdapter
    {
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            context.WriteAndFlushAsync(message);
        }
    }

    public class TcpServerSocketChannelAndConnectionCompatSpecs
    {
        private Helios.Net.Bootstrap.IConnectionFactory _clientBootstrap;
        private ServerBootstrap _serverBootstrap;
        private IEventLoopGroup _serverGroup;

        private readonly List<IByteBuf> _received = new List<IByteBuf>();
        private readonly ManualResetEventSlim _resetEvent = new ManualResetEventSlim(false);
        private const int ReadCount = 100;

        public TcpServerSocketChannelAndConnectionCompatSpecs()
        {
            _serverGroup = new MultithreadEventLoopGroup(1);

            _clientBootstrap = new Net.Bootstrap.ClientBootstrap()
                .SetTransport(TransportType.Tcp)
                .SetDecoder(new Helios.Serialization.LengthFieldFrameBasedDecoder(int.MaxValue, 0, 4, 0, 4))
                .SetEncoder(new Helios.Serialization.LengthFieldPrepender(4, false))
                .WorkerThreads(1).Build();

            _serverBootstrap = new ServerBootstrap()
                .Channel<TcpServerSocketChannel>()
                .Group(_serverGroup)
                .ChildHandler(new ActionChannelInitializer<TcpSocketChannel>(channel =>
                {
                    channel.Pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4, true))
                        .AddLast(new HeliosBackwardsCompatabilityLengthFramePrepender(4, false))
                        .AddLast(new EchoHandler());
                }));
        }

        [Fact]
        public void Helios20_Server_and_Helios14_Client_Should_ReadWrite()
        {
            IConnection client = null;
            IChannel server = null;
            try
            {
                server = _serverBootstrap.BindAsync(new IPEndPoint(IPAddress.Loopback, 0)).Result;
                var serverAddress = (IPEndPoint) server.LocalAddress;
                client = _clientBootstrap.NewConnection(NodeBuilder.BuildNode().Host(IPAddress.Loopback).WithPort(serverAddress.Port));

                var actualReadCount = 0;

                client.Receive += (data, channel) =>
                {
                    _received.Add(Unpooled.WrappedBuffer(data.Buffer));
                    if (++ actualReadCount ==  ReadCount)
                    {
                        _resetEvent.Set();
                    }
                };

                client.OnConnection += (address, channel) =>
                {
                    channel.BeginReceive();
                };

                client.Open();

                var writes = Enumerable.Range(0, ReadCount).Select(x => ThreadLocalRandom.Current.Next()).OrderBy(y => y).ToList();
                foreach (var write in writes)
                    client.Send(Unpooled.Buffer(4).WriteInt(write).Array, 0, 4, null);

                _resetEvent.Wait();
                var decodedReceive = _received.Select(y => y.ReadInt()).OrderBy(x => x).ToList();
                Assert.True(decodedReceive.SequenceEqual(writes));

            }
            finally
            {
                client?.Close();
                server?.CloseAsync().Wait();
                _serverGroup.ShutdownGracefullyAsync().Wait();
            }
        }
    }
}
