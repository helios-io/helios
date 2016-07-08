using System;
using System.Net;
using FsCheck.Xunit;
using FsCheck;
using Helios.Buffers;
using Helios.Channels;
using Helios.Channels.Bootstrap;
using Helios.Channels.Sockets;

namespace Helios.FsCheck.Tests.Channels.Sockets
{
    /// <summary>
    /// Generates a range of random options for DNS
    /// </summary>
    public static class EndpointGenerators
    {
        public static Arbitrary<EndPoint> Endpoints()
        {
            return Arb.From(Gen.Elements<EndPoint>(new IPEndPoint(IPAddress.Loopback, 0),
                new IPEndPoint(IPAddress.Any, 0),
                new IPEndPoint(IPAddress.IPv6Loopback, 0),
                new IPEndPoint(IPAddress.IPv6Any, 0),
                new DnsEndPoint("localhost", 0)));
        }
    }

    public class DnsResolutionAndBindingSpec : IDisposable
    {
        public DnsResolutionAndBindingSpec()
        {
            Arb.Register(typeof(EndpointGenerators));
        }

        private MultithreadEventLoopGroup _serverGroup = new MultithreadEventLoopGroup(1);
        private MultithreadEventLoopGroup _clientGroup = new MultithreadEventLoopGroup(1);

        [Property]
        public Property TcpSocketServerChannel_can_bind_on_any_valid_EndPoint(EndPoint ep)
        {
            IChannel c = null;
            try
            {
                var sb = new ServerBootstrap()
                    .Channel<TcpServerSocketChannel>()
                    .ChildHandler(new ActionChannelInitializer<TcpSocketChannel>(channel => { }))
                    .Group(_serverGroup);

                c = sb.BindAsync(ep).Result;

                return c.IsOpen.Label("Channel should be open").And(c.IsActive).Label("Channel should be active");
            }
            finally
            {
                c?.CloseAsync().Wait(TimeSpan.FromMilliseconds(200));
            }
        }

        [Property]
        public Property TcpServerSocketChannel_can_accept_connection_on_any_valid_Endpoint(EndPoint ep)
        {
            var ip = ep as IPEndPoint;
            

            IChannel s = null;
            IChannel c = null;
            try
            {
                var sb = new ServerBootstrap()
                    .Channel<TcpServerSocketChannel>()
                    .ChildHandler(new ActionChannelInitializer<TcpSocketChannel>(channel => { }))
                    .Group(_serverGroup);

                s = sb.BindAsync(ep).Result;

                

                var cb = new ClientBootstrap()
                    .Channel<TcpSocketChannel>()
                    .Option(ChannelOption.TcpNodelay, true)
                    .Option(ChannelOption.ConnectTimeout, TimeSpan.FromMilliseconds(100))
                    .Handler(new ActionChannelInitializer<TcpSocketChannel>(channel => { }))
                    .Group(_clientGroup);

                var clientEp = s.LocalAddress;
                if (ip != null) // handle special case of 0.0.0.0, which clients can't connect to directly.
                {
                    if (ip.Address.Equals(IPAddress.Any))
                        clientEp = new IPEndPoint(IPAddress.Loopback, ((IPEndPoint)s.LocalAddress).Port);
                    if (ip.Address.Equals(IPAddress.IPv6Any))
                        clientEp = new IPEndPoint(IPAddress.IPv6Loopback, ((IPEndPoint)s.LocalAddress).Port);
                }

                c = cb.ConnectAsync(clientEp).Result;
                c.WriteAndFlushAsync(Unpooled.Buffer(4).WriteInt(2)).Wait(20);

                return c.IsOpen.Label("Channel should be open")
                    .And(c.IsActive).Label("Channel should be active")
                    .And(c.IsWritable).Label("Channel should be writable");
            }
            finally
            {
                try
                {
                    c?.CloseAsync().Wait(TimeSpan.FromMilliseconds(200));
                    s?.CloseAsync().Wait(TimeSpan.FromMilliseconds(200));
                }
                catch { }
            }
        }

        public void Dispose()
        {
            _serverGroup.ShutdownGracefullyAsync();
            _clientGroup.ShutdownGracefullyAsync();
        }
    }
}
