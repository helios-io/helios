// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using Helios.Channels;

namespace Helios.FsCheck.Tests.Channels.Sockets.Models
{
    public sealed class ImmutableTcpServerSocketModel : ITcpServerSocketModel
    {
        private static readonly IReadOnlyList<IPEndPoint> EmptyIps = new List<IPEndPoint>();
        private static readonly IReadOnlyList<IChannel> EmptyChannels = new List<IChannel>();
        private static readonly IReadOnlyList<int> EmptyMessages = new List<int>();

        public ImmutableTcpServerSocketModel() : this(null, null)
        {
        }

        public ImmutableTcpServerSocketModel(IChannel self, IPEndPoint endpoint)
            : this(self, endpoint, EmptyIps, EmptyMessages, EmptyMessages, EmptyChannels)
        {
        }

        public ImmutableTcpServerSocketModel(IChannel self, IPEndPoint endpoint, IReadOnlyList<IPEndPoint> remoteClients,
            IReadOnlyList<int> lastReceivedMessages, IReadOnlyList<int> writtenMessages,
            IReadOnlyList<IChannel> localChannels)
        {
            Self = self;
            RemoteClients = remoteClients;
            LastReceivedMessages = lastReceivedMessages;
            WrittenMessages = writtenMessages;
            LocalChannels = localChannels;
            BoundAddress = endpoint;
        }

        public IPEndPoint BoundAddress { get; }
        public IChannel Self { get; }
        public IReadOnlyList<IChannel> LocalChannels { get; }
        public IReadOnlyList<IPEndPoint> RemoteClients { get; }
        public IReadOnlyList<int> LastReceivedMessages { get; }
        public IReadOnlyList<int> WrittenMessages { get; }


        /// <summary>
        ///     MUTABLE, due to weird setup issue on bind.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public ITcpServerSocketModel SetSelf(IChannel self)
        {
            return new ImmutableTcpServerSocketModel(self, BoundAddress, RemoteClients, LastReceivedMessages,
                WrittenMessages, LocalChannels);
        }

        public ITcpServerSocketModel SetOwnAddress(IPEndPoint endpoint)
        {
            return new ImmutableTcpServerSocketModel(Self, endpoint, RemoteClients, LastReceivedMessages,
                WrittenMessages, LocalChannels);
        }

        public ITcpServerSocketModel AddLocalChannel(IChannel channel)
        {
            return new ImmutableTcpServerSocketModel(Self, BoundAddress, RemoteClients, LastReceivedMessages,
                WrittenMessages, LocalChannels.Concat(new[] {channel}).ToList());
        }

        public ITcpServerSocketModel RemoveLocalChannel(IChannel channel)
        {
            return new ImmutableTcpServerSocketModel(Self, BoundAddress, RemoteClients, LastReceivedMessages,
                WrittenMessages, LocalChannels.Where(x => !x.Id.Equals(channel.Id)).ToList());
        }

        public ITcpServerSocketModel AddClient(IPEndPoint endpoint)
        {
            return new ImmutableTcpServerSocketModel(Self, BoundAddress, RemoteClients.Concat(new[] {endpoint}).ToList(),
                LastReceivedMessages, WrittenMessages, LocalChannels);
        }

        public ITcpServerSocketModel RemoveClient(IPEndPoint endpoint)
        {
            return new ImmutableTcpServerSocketModel(Self, BoundAddress,
                RemoteClients.Where(x => !Equals(x, endpoint)).ToList(), LastReceivedMessages, WrittenMessages,
                LocalChannels);
        }

        public ITcpServerSocketModel ClearMessages()
        {
            return new ImmutableTcpServerSocketModel(Self, BoundAddress, RemoteClients, EmptyMessages, EmptyMessages,
                LocalChannels);
        }

        public ITcpServerSocketModel WriteMessages(params int[] messages)
        {
            return new ImmutableTcpServerSocketModel(Self, BoundAddress, RemoteClients, LastReceivedMessages,
                WrittenMessages.Concat(messages).ToList(), LocalChannels);
        }

        public ITcpServerSocketModel ReceiveMessages(params int[] messages)
        {
            return new ImmutableTcpServerSocketModel(Self, BoundAddress, RemoteClients,
                LastReceivedMessages.Concat(messages).ToList(), WrittenMessages, LocalChannels);
        }

        public override string ToString()
        {
            return
                $"TcpServerState(Hash={GetHashCode()},BoundAddress={BoundAddress}, Active={Self?.IsActive ?? false} RemoteConnections=[{string.Join("|", RemoteClients)}], Written=[{string.Join(",", WrittenMessages)}], Received=[{string.Join(",", LastReceivedMessages)}])";
        }
    }
}

