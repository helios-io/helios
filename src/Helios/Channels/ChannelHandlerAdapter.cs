// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Net;
using System.Threading.Tasks;

namespace Helios.Channels
{
    /// <summary>
    ///     Default implementation of <see cref="IChannelHandler" /> - all implementations are no-nops.
    ///     Begin from this to begin adding your own <see cref="IChannelHandler" /> implementations.
    /// </summary>
    public class ChannelHandlerAdapter : IChannelHandler
    {
        internal bool Added;

        [Skip]
        public virtual void ChannelRegistered(IChannelHandlerContext context)
        {
            context.FireChannelRegistered();
        }

        [Skip]
        public virtual void ChannelUnregistered(IChannelHandlerContext context)
        {
            context.FireChannelUnregistered();
        }

        [Skip]
        public virtual void ChannelActive(IChannelHandlerContext context)
        {
            context.FireChannelActive();
        }

        [Skip]
        public virtual void ChannelInactive(IChannelHandlerContext context)
        {
            context.FireChannelInactive();
        }

        [Skip]
        public virtual void ChannelRead(IChannelHandlerContext context, object message)
        {
            context.FireChannelRead(message);
        }

        [Skip]
        public virtual void ChannelReadComplete(IChannelHandlerContext context)
        {
            context.FireChannelReadComplete();
        }

        [Skip]
        public virtual void ChannelWritabilityChanged(IChannelHandlerContext context)
        {
            context.FireChannelWritabilityChanged();
        }

        [Skip]
        public virtual void HandlerAdded(IChannelHandlerContext context)
        {
        }

        [Skip]
        public virtual void HandlerRemoved(IChannelHandlerContext context)
        {
        }

        [Skip]
        public virtual void UserEventTriggered(IChannelHandlerContext context, object evt)
        {
            context.FireUserEventTriggered(evt);
        }

        [Skip]
        public virtual Task WriteAsync(IChannelHandlerContext context, object message)
        {
            return context.WriteAsync(message);
        }

        [Skip]
        public virtual void Flush(IChannelHandlerContext context)
        {
            context.Flush();
        }

        [Skip]
        public virtual Task BindAsync(IChannelHandlerContext context, EndPoint localAddress)
        {
            return context.BindAsync(localAddress);
        }

        [Skip]
        public virtual Task ConnectAsync(IChannelHandlerContext context, EndPoint remoteAddress, EndPoint localAddress)
        {
            return context.ConnectAsync(remoteAddress, localAddress);
        }

        [Skip]
        public virtual Task DisconnectAsync(IChannelHandlerContext context)
        {
            return context.DisconnectAsync();
        }

        [Skip]
        public virtual Task CloseAsync(IChannelHandlerContext context)
        {
            return context.CloseAsync();
        }

        [Skip]
        public virtual void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            context.FireExceptionCaught(exception);
        }

        [Skip]
        public virtual Task DeregisterAsync(IChannelHandlerContext context)
        {
            return context.DeregisterAsync();
        }

        [Skip]
        public virtual void Read(IChannelHandlerContext context)
        {
            context.Read();
        }
    }
}