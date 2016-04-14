﻿using System;
using System.Net;
using System.Threading.Tasks;
using Helios.Buffers;
using Helios.Ops;

namespace Helios.Channels
{
    public interface IChannelHandlerContext
    {
        IChannel Channel { get; }

        IByteBufAllocator Allocator { get; }


        IExecutor Executor { get; }

        IChannelHandlerInvoker Invoker { get; }

        string Name { get; }

        IChannelHandler Handler { get; }

        bool Removed { get; }

        IChannelHandlerContext FireChannelRegistered();

        IChannelHandlerContext FireChannelUnregistered();

        IChannelHandlerContext FireChannelActive();

        IChannelHandlerContext FireChannelInactive();

        IChannelHandlerContext FireChannelRead(object message);

        IChannelHandlerContext FireChannelReadComplete();

        IChannelHandlerContext FireChannelWritabilityChanged();

        IChannelHandlerContext FireExceptionCaught(Exception ex);

        IChannelHandlerContext FireUserEventTriggered(object evt);

        IChannelHandlerContext Read();

        Task WriteAsync(object message); 

        IChannelHandlerContext Flush();

        Task WriteAndFlushAsync(object message);

        Task BindAsync(EndPoint localAddress);

        Task ConnectAsync(EndPoint remoteAddress);

        Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress);

        Task DisconnectAsync();

        Task CloseAsync();

        Task DeregisterAsync();
    }
}