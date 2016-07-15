// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Net;
using Helios.Buffers;
using Helios.Channels;
using Helios.Ops;
using Helios.Ops.Executors;
using Helios.Serialization;
using Helios.Topology;

namespace Helios.Net.Bootstrap
{
    /// <summary>
    ///     Client bootstrap for outbound connections
    /// </summary>
    public class ClientBootstrap : AbstractBootstrap
    {
        public ClientBootstrap()
        {
            Workers = 2;
            InternalExecutor = new BasicExecutor();
        }

        public ClientBootstrap(ClientBootstrap other) : base(other)
        {
            Workers = other.Workers;
            InternalExecutor = other.InternalExecutor;
        }

        protected IExecutor InternalExecutor { get; set; }

        protected NetworkEventLoop EventLoop
        {
            get { return EventLoopFactory.CreateNetworkEventLoop(Workers, InternalExecutor); }
        }

        protected int Workers { get; set; }

        public ClientBootstrap WorkerThreads(int workerThreadCount)
        {
            if (workerThreadCount < 1) throw new ArgumentException("Can't be below 1", "workerThreadCount");
            Workers = workerThreadCount;
            return this;
        }

        public ClientBootstrap Executor(IExecutor executor)
        {
            if (executor == null) throw new ArgumentNullException("executor");
            InternalExecutor = executor;
            return this;
        }

        public new ClientBootstrap SetConfig(IConnectionConfig config)
        {
            base.SetConfig(config);
            return this;
        }

        public new ClientBootstrap SetTransport(TransportType type)
        {
            base.SetTransport(type);
            return this;
        }

        public ClientBootstrap RemoteAddress(INode node)
        {
            return this;
        }

        public new ClientBootstrap SetDecoder(IMessageDecoder decoder)
        {
            base.SetDecoder(decoder);
            return this;
        }

        public new ClientBootstrap SetEncoder(IMessageEncoder encoder)
        {
            base.SetEncoder(encoder);
            return this;
        }

        public new ClientBootstrap SetAllocator(IByteBufAllocator allocator)
        {
            base.SetAllocator(allocator);
            return this;
        }

        public new ClientBootstrap OnConnect(ConnectionEstablishedCallback connectionEstablishedCallback)
        {
            base.OnConnect(connectionEstablishedCallback);
            return this;
        }

        public new ClientBootstrap OnDisconnect(ConnectionTerminatedCallback connectionTerminatedCallback)
        {
            base.OnDisconnect(connectionTerminatedCallback);
            return this;
        }

        public new ClientBootstrap OnReceive(ReceivedDataCallback receivedDataCallback)
        {
            base.OnReceive(receivedDataCallback);
            return this;
        }

        public new ClientBootstrap OnError(ExceptionCallback exceptionCallback)
        {
            base.OnError(exceptionCallback);
            return this;
        }

        public new ClientBootstrap SetOption(string optionKey, object optionValue)
        {
            base.SetOption(optionKey, optionValue);
            return this;
        }

        public override void Validate()
        {
            if (Type == TransportType.All) throw new ArgumentException("Type must be set");
            if (Workers < 1) throw new ArgumentException("Workers must be at least 1");
        }

        protected override IConnectionFactory BuildInternal()
        {
            switch (Type)
            {
                case TransportType.Tcp:
                    return new TcpConnectionFactory(this);
                case TransportType.Udp:
                    return new UdpConnectionFactory(this);
                default:
                    throw new InvalidOperationException("This shouldn't happen");
            }
        }
    }
}