// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;

namespace Helios.Channels.Embedded
{
    internal sealed class EmbeddedChannelId : IChannelId
    {
        public static readonly EmbeddedChannelId Instance = new EmbeddedChannelId();

        private EmbeddedChannelId()
        {
        }

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