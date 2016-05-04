// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using Helios.Channels;

namespace Helios.FsCheck.Tests.Channels.Sockets.Models
{
    public class TcpClientSocketModel
    {
        private static readonly IReadOnlyList<int> Empty = new List<int>();

        public TcpClientSocketModel() : this(null)
        {
        }

        public TcpClientSocketModel(IChannel self) : this(self, Empty, Empty, ConnectionState.Connecting)
        {
        }

        public TcpClientSocketModel(IChannel self, IReadOnlyList<int> lastWrittenMessages,
            IReadOnlyList<int> lastReceivedMessages, ConnectionState state)
        {
            Self = self;
            LastWrittenMessages = lastWrittenMessages;
            LastReceivedMessages = lastReceivedMessages;
            State = state;
        }

        public IChannel Self { get; }
        public IPEndPoint BoundAddress => (IPEndPoint) Self.LocalAddress;
        public IReadOnlyList<int> LastWrittenMessages { get; }
        public IReadOnlyList<int> LastReceivedMessages { get; }
        public ConnectionState State { get; }

        public TcpClientSocketModel SetChannel(IChannel self)
        {
            return new TcpClientSocketModel(Self, LastWrittenMessages, LastReceivedMessages, State);
        }

        public TcpClientSocketModel SetState(ConnectionState newState)
        {
            return new TcpClientSocketModel(Self, LastWrittenMessages, LastReceivedMessages, newState);
        }

        public TcpClientSocketModel ResetMessages()
        {
            return new TcpClientSocketModel(Self, Empty, Empty, State);
        }

        public TcpClientSocketModel WriteMessages(params int[] messages)
        {
            return new TcpClientSocketModel(Self, LastWrittenMessages.Concat(messages).ToList(), LastReceivedMessages,
                State);
        }

        public TcpClientSocketModel ReceiveMessages(params int[] messages)
        {
            return new TcpClientSocketModel(Self, LastWrittenMessages, LastReceivedMessages.Concat(messages).ToList(),
                State);
        }
    }
}

