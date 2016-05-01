using System;
using System.Collections.Generic;
namespace Helios.Channels.Embedded
{
    sealed class EmbeddedChannelId : IChannelId
    {
        public static readonly EmbeddedChannelId Instance = new EmbeddedChannelId();

        private EmbeddedChannelId() { }

        public int CompareTo(IChannelId other)
        {
            if (other is EmbeddedChannelId)
            {
                return 0;
            }
            return string.Compare(ToString(), other.ToString(), StringComparison.Ordinal);
        }

        public override string ToString()
        {
            return "embedded";
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
