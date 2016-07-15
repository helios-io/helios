// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;

namespace Helios.Channels
{
    public class ChannelException : Exception
    {
        public ChannelException(Exception ex) : this("ChannelException", ex)
        {
        }

        public ChannelException(string message) : base(message)
        {
        }

        public ChannelException(string connectionRefused, Exception exception) : base(connectionRefused, exception)
        {
        }
    }
}