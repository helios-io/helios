// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Helios.Channels;
using Helios.Channels.Sockets;

namespace Helios.FsCheck.Tests.Channels.Sockets.Models
{
    /// <summary>
    ///     Shared implementation of <see cref="ITcpServerSocketModel" /> that is used across multiple child
    ///     <see cref="TcpSocketChannel" /> instances spawned by a single <see cref="TcpServerSocketChannel" />
    /// </summary>
    public sealed class ConcurrentTcpServerSocketModel : ITcpServerSocketModel
    {
        private readonly List<IChannel> _localChannels = new List<IChannel>();

        private readonly List<IPEndPoint> _remoteClients = new List<IPEndPoint>();

        private ConcurrentBag<int> _receivedMessages = new ConcurrentBag<int>();

        private ConcurrentBag<int> _writtenMessages = new ConcurrentBag<int>();

        public ConcurrentTcpServerSocketModel() : this(null)
        {
        }

        public ConcurrentTcpServerSocketModel(IChannel self)
        {
            Self = self;
        }

        public IPEndPoint BoundAddress => (IPEndPoint) Self?.LocalAddress;
        public IChannel Self { get; private set; }
        public IReadOnlyList<IChannel> LocalChannels => _localChannels;
        public IReadOnlyList<IPEndPoint> RemoteClients => _remoteClients;
        public IReadOnlyList<int> LastReceivedMessages => _receivedMessages.ToList();
        public IReadOnlyList<int> WrittenMessages => _writtenMessages.ToList();

        public ITcpServerSocketModel SetSelf(IChannel self)
        {
            Self = self;
            return this;
        }

        public ITcpServerSocketModel SetOwnAddress(IPEndPoint endpoint)
        {
            throw new NotSupportedException();
        }

        public ITcpServerSocketModel AddLocalChannel(IChannel channel)
        {
            lock (_localChannels)
            {
                _localChannels.Add(channel);
            }
            return this;
        }

        public ITcpServerSocketModel RemoveLocalChannel(IChannel channel)
        {
            lock (_localChannels)
            {
                _localChannels.Remove(channel);
            }
            return this;
        }

        public ITcpServerSocketModel AddClient(IPEndPoint endpoint)
        {
            lock (_remoteClients)
            {
                _remoteClients.Add(endpoint);
            }
            return this;
        }

        public ITcpServerSocketModel RemoveClient(IPEndPoint endpoint)
        {
            lock (_remoteClients)
            {
                _remoteClients.Remove(endpoint);
            }
            return this;
        }

        public ITcpServerSocketModel ClearMessages()
        {
            _receivedMessages = new ConcurrentBag<int>();
            _writtenMessages = new ConcurrentBag<int>();
            return this;
        }

        public ITcpServerSocketModel WriteMessages(params int[] messages)
        {
            foreach (var message in messages)
                _writtenMessages.Add(message);
            return this;
        }

        public ITcpServerSocketModel ReceiveMessages(params int[] messages)
        {
            foreach (var message in messages)
                _receivedMessages.Add(message);
            return this;
        }

        public override string ToString()
        {
            return
                $"TcpServerState(BoundAddress={BoundAddress}, Active={Self?.IsActive ?? false} RemoteConnections=[{string.Join("|", RemoteClients)}], Written=[{string.Join(",", WrittenMessages)}], Received=[{string.Join(",", LastReceivedMessages)}])";
        }
    }
}