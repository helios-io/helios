using System.Collections.Generic;
using System.Linq;
using System.Net;
using Helios.Channels;

namespace Helios.FsCheck.Tests.Channels.Sockets.Models
{
    public class TcpClientSocketModel
    {
        public TcpClientSocketModel() : this(null) { }

        public TcpClientSocketModel(IChannel self) : this(self, Empty, Empty, ConnectionState.Connecting) { }

        public TcpClientSocketModel(IChannel self, IReadOnlyList<int> lastWrittenMessages, IReadOnlyList<int> lastReceivedMessages, ConnectionState state)
        {
            Self = self;
            LastWrittenMessages = lastWrittenMessages;
            LastReceivedMessages = lastReceivedMessages;
            State = state;
        }

        public IChannel Self { get; private set; }
        public IPEndPoint BoundAddress => (IPEndPoint)Self.LocalAddress;
        public IReadOnlyList<int> LastWrittenMessages { get; private set; }
        public IReadOnlyList<int> LastReceivedMessages { get; private set; }
        public ConnectionState State { get; private set; }

        private static readonly IReadOnlyList<int> Empty = new List<int>();

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
            return new TcpClientSocketModel(Self, LastWrittenMessages.Concat(messages).ToList(), LastReceivedMessages, State);
        }

        public TcpClientSocketModel ReceiveMessages(params int[] messages)
        {
            return new TcpClientSocketModel(Self, LastWrittenMessages, LastReceivedMessages.Concat(messages).ToList(), State);
        }

    }
}