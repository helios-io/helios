// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using Helios.Serialization;

namespace Helios.Codecs
{
    /// <summary>
    ///     Exception thrown by a <see cref="IMessageDecoder" /> when encountering corrupt data
    /// </summary>
    public class DecoderException : HeliosException
    {
        public DecoderException(string message) : base(message)
        {
        }

        public DecoderException(Exception inner) : base("Exception occurred while decoding.", inner)
        {
        }
    }

    /// <summary>
    ///     Exception thrown by a <see cref="IMessageEncoder" />
    /// </summary>
    public class EncoderException : HeliosException
    {
        public EncoderException(Exception inner) : base("Exception occurred while encoding.", inner)
        {
        }

        public EncoderException(string message) : base(message)
        {
        }
    }

    /// <summary>
    ///     Exception class that is thrown by a <see cref="IMessageDecoder" /> when it encounters a frame that is longer than
    ///     can be processed
    /// </summary>
    public class TooLongFrameException : DecoderException
    {
        public TooLongFrameException(string message) : base(message)
        {
        }
    }

    /// <summary>
    ///     Thrown when a frame of negative length is detected by a <see cref="IMessageDecoder" />
    /// </summary>
    public class CorruptedFrameException : HeliosException
    {
        public CorruptedFrameException(string message) : base(message)
        {
        }
    }
}