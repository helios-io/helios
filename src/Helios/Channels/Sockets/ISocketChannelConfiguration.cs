// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

namespace Helios.Channels.Sockets
{
    /// <summary>
    ///     <see cref="IChannelConfiguration" /> specific to sockets.
    /// </summary>
    public interface ISocketChannelConfiguration : IChannelConfiguration
    {
        bool AllowHalfClosure { get; set; }

        int Linger { get; set; }

        int SendBufferSize { get; set; }

        int ReceiveBufferSize { get; set; }

        bool ReuseAddress { get; set; }

        bool KeepAlive { get; set; }

        bool TcpNoDelay { get; set; }
    }
}