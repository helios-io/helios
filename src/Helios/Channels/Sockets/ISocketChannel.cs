// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

namespace Helios.Channels.Sockets
{
    /// <summary>
    ///     Marker interface for channels which use TCP / UDP sockets
    /// </summary>
    public interface ISocketChannel : IChannel
    {
        new IServerSocketChannel Parent { get; }
        new ISocketChannelConfiguration Configuration { get; }
    }
}