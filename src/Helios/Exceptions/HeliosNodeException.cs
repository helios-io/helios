// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using Helios.Topology;

namespace Helios.Exceptions
{
    public class HeliosNodeException : Exception
    {
        public HeliosNodeException(Exception innerException, INode affectedNode)
            : this(innerException.Message, innerException, affectedNode)
        {
        }

        public HeliosNodeException(string message) : base(message)
        {
        }

        public HeliosNodeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public HeliosNodeException(string message, INode affectedNode) : base(message)
        {
            AffectedNode = affectedNode;
        }

        public HeliosNodeException(string message, Exception innerException, INode affectedNode)
            : base(message, innerException)
        {
            AffectedNode = affectedNode;
        }

        public HeliosNodeException()
        {
        }

        public INode AffectedNode { get; protected set; }
    }
}