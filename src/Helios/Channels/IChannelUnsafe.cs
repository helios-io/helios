﻿using System.Net;
using System.Threading.Tasks;
using Helios.Concurrency;

namespace Helios.Channels
{
    public interface IChannelUnsafe
    {
        IRecvByteBufferAllocatorHandle RecvBufAllocHandle { get; }

        ChannelOutboundBuffer OutboundBuffer { get; }

        Task RegisterAsync(IEventLoop eventLoop);

        Task DeregisterAsync();

        Task BindAsync(EndPoint localAddress);

        Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress);

        Task DisconnectAsync();

        Task CloseAsync();

        void CloseForcibly();

        void BeginRead();

        Task WriteAsync(object message);

        void Flush();
    }
}