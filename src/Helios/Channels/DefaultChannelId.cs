using System;
using Helios.Util;

namespace Helios.Channels
{
    [Serializable]
    sealed class DefaultChannelId : IChannelId
    {
        private readonly int _hashCode;

        private DefaultChannelId()
        {
            unchecked
            {
                _hashCode = (int) (MonotonicClock.GetTicks() & 0xFFFFFFFF);
                _hashCode *= _hashCode ^ ThreadLocalRandom.Current.Next();
            }
        }

        public int CompareTo(IChannelId other)
        {
            return 0;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is DefaultChannelId)) return false;
            return GetHashCode() == obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override string ToString()
        {
            return $"ChannelId({_hashCode})";
        }

        public static IChannelId NewInstance()
        {
            return new DefaultChannelId();
        }
    }
}