// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;

namespace Helios.Exceptions
{
    public class HeliosConnectionException : HeliosException
    {
        public HeliosConnectionException()
            : this(ExceptionType.Unknown)
        {
        }

        public HeliosConnectionException(ExceptionType type)
        {
            Type = type;
        }

        public HeliosConnectionException(ExceptionType type, Exception innerException)
            : this(type, innerException == null ? string.Empty : innerException.Message, innerException)
        {
        }

        public HeliosConnectionException(ExceptionType type, string message) : base(message)
        {
            Type = type;
        }

        public HeliosConnectionException(ExceptionType type, string message, Exception innerException)
            : base(message, innerException)
        {
            Type = type;
        }

        public ExceptionType Type { get; }
    }
}