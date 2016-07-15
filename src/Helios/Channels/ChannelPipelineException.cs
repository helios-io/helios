// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;

namespace Helios.Channels
{
    /// <summary>
    ///     Exception thrown whenever there's an issue modifying the <see cref="IChannelPipeline" />
    /// </summary>
    public class ChannelPipelineException : Exception
    {
        public ChannelPipelineException(string message)
            : base(message)
        {
        }

        public ChannelPipelineException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}