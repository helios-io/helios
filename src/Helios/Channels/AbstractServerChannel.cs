// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Net;
using System.Threading.Tasks;
using Helios.Util.Concurrency;

namespace Helios.Channels
{
    /// <summary>
    ///     A skeleton of a server-side <see cref="IChannel" /> implementation, which
    ///     does not allow any of the following operations:
    ///     * <see cref="IChannel.ConnectAsync(EndPoint)" />
    ///     * <see cref="IChannel.DisconnectAsync()" />
    ///     * <see cref="IChannel.WriteAsync(object)" />
    ///     * <see cref="IChannel.Flush()" />
    /// </summary>
    public abstract class AbstractServerChannel : AbstractChannel, IServerChannel
    {
        protected AbstractServerChannel() : base(null)
        {
        }

        protected override EndPoint RemoteAddressInternal
        {
            get { return null; }
        }

        protected override void DoDisconnect()
        {
            throw new NotSupportedException();
        }

        protected override void DoWrite(ChannelOutboundBuffer input)
        {
            throw new NotSupportedException();
        }

        protected override object FilterOutboundMessage(object msg)
        {
            throw new NotSupportedException();
        }

        protected override IChannelUnsafe NewUnsafe()
        {
            return new DefaultServerUnsafe(this);
        }

        private sealed class DefaultServerUnsafe : AbstractUnsafe
        {
            public DefaultServerUnsafe(AbstractChannel channel) : base(channel)
            {
            }

            public override Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress)
            {
                return TaskEx.FromException(new NotSupportedException());
            }
        }
    }
}