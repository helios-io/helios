﻿namespace Helios.Channels.Sockets
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