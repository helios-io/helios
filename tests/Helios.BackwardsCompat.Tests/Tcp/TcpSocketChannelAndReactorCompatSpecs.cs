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
using Helios.Reactor.Bootstrap;
using Helios.Reactor.Tcp;
using Helios.Codecs;
using Helios.Reactor;
using Helios.Topology;
using Helios.Util;
using Xunit;

namespace Helios.BackwardsCompat.Tests.Tcp
{
    public class ReadRecorderHandler : ChannelHandlerAdapter
    {
        private readonly int _expectedReadCount;
        private int _actualReadCount;
        private readonly ManualResetEventSlim _readFinished;
        public IList<IByteBuf> Received { get; }

        public ReadRecorderHandler(IList<IByteBuf> received, ManualResetEventSlim readFinished, int expectedReadCount)
        {
            Received = received;
            _readFinished = readFinished;
            _expectedReadCount = expectedReadCount;
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            Received.Add((IByteBuf)message);
            if (++_actualReadCount == _expectedReadCount)
            {
                _readFinished.Set();
            }
        }
    }

    /// <summary>
    /// Verifies that a <see cref="TcpSocketChannel"/> and a <see cref="TcpProxyReactor"/> can communicate
    /// using the same encoding
    /// </summary>
    public class TcpSocketChannelAndReactorCompatSpecs
    {
        private Reactor.Bootstrap.IServerFactory _serverBootstrap;
        private ClientBootstrap _clientBootstrap;
        private IEventLoopGroup _clientGroup;

        private readonly List<IByteBuf> _received = new List<IByteBuf>();
        private readonly ManualResetEventSlim _resetEvent = new ManualResetEventSlim(false);
        private const int ReadCount = 100;

        public TcpSocketChannelAndReactorCompatSpecs()
        {
            _clientGroup = new MultithreadEventLoopGroup(1);
            _serverBootstrap = new Reactor.Bootstrap.ServerBootstrap()
                .SetTransport(TransportType.Tcp)
                .SetDecoder(new Helios.Serialization.LengthFieldFrameBasedDecoder(int.MaxValue,0,4,0,4))
                .SetEncoder(new Helios.Serialization.LengthFieldPrepender(4, false)).Build();

            _clientBootstrap = new ClientBootstrap()
                .Channel<TcpSocketChannel>()
                .Group(_clientGroup)
                .Handler(new ActionChannelInitializer<TcpSocketChannel>(channel =>
                {
                    channel.Pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4, true))
                        .AddLast(new HeliosBackwardsCompatabilityLengthFramePrepender(4, false))
                        .AddLast(new ReadRecorderHandler(_received, _resetEvent, ReadCount));
                }));
        }

        [Fact]
        public void Helios20_Client_and_Helios14_Server_Should_ReadWrite()
        {
            IReactor server = null;
            IChannel client = null;
            try
            {
                server = _serverBootstrap.NewReactor(NodeBuilder.BuildNode().Host(IPAddress.Loopback).WithPort(0));
                server.OnReceive += (data, channel) =>
                {
                    // echo the response back
                    channel.Send(data);
                };

                server.OnConnection += (address, channel) =>
                {
                    channel.BeginReceive();
                };
              
                server.Start();
                client = _clientBootstrap.ConnectAsync(server.LocalEndpoint).Result;
                Task.Delay(TimeSpan.FromMilliseconds(500)).Wait(); // because Helios 1.4 is stupid
                var writes = Enumerable.Range(0, ReadCount).Select(x => ThreadLocalRandom.Current.Next()).OrderBy(y => y).ToList();
                foreach (var write in writes)
                    client.WriteAndFlushAsync(Unpooled.Buffer(4).WriteInt(write));

                _resetEvent.Wait();
                var decodedReceive = _received.Select(y => y.ReadInt()).OrderBy(x => x).ToList();
                Assert.True(decodedReceive.SequenceEqual(writes));

            }
            finally
            {
                client?.CloseAsync().Wait();
                server?.Stop();
                _clientGroup.ShutdownGracefullyAsync().Wait();
            }
        }
    }
}
