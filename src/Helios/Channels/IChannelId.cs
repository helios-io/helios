using System;

namespace Helios.Channels
{
    /// <summary>
    /// Interface used for describing a channel's ID
    /// </summary>
    public interface IChannelId : IComparable<IChannelId>, IEquatable<IChannelId>
    {
    }
}
