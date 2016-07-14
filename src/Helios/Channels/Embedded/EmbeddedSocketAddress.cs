// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Net;

namespace Helios.Channels.Embedded
{
    internal sealed class EmbeddedSocketAddress : EndPoint
    {
        public override string ToString()
        {
            return "embedded";
        }
    }
}