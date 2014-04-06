using System;

namespace Helios.Channels
{
    /// <summary>
    /// Configuration interface associated with a given <see cref="IChannel"/>
    /// </summary>
    public interface IChannelConfig
    {
        int MaxMessagesPerRead { get; }

        IChannelConfig SetMaxMessagesPerRead(int maxMessagesPerRead);

        bool IsAutoRead { get; }

        IChannelConfig SetAutoRead(bool autoRead);

        TimeSpan ConnectTimeout { get; }

        IChannelConfig SetConnectTimeout(TimeSpan timeout);
    }
}
