// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Net;
using Helios.Buffers;
using Helios.Channels;
using Helios.Net;
using Helios.Net.Bootstrap;
using Helios.Ops;
using Helios.Ops.Executors;
using Helios.Serialization;

namespace Helios.Reactor.Bootstrap
{
    [Obsolete()]
    public class ServerBootstrap : AbstractBootstrap
    {
        public ServerBootstrap()
            : base()
        {
            UseProxies = true;
            WorkersShareFiber(true);
            BufferBytes = NetworkConstants.DEFAULT_BUFFER_SIZE;
            Workers = 2;
            InternalExecutor = new BasicExecutor();
        }

        public ServerBootstrap(ServerBootstrap other)
            : base(other)
        {
            UseProxies = other.UseProxies;
            BufferBytes = other.BufferBytes;
            WorkersShareFiber(other.UseSharedFiber);
            Workers = other.Workers;
            InternalExecutor = other.InternalExecutor;
        }

        protected IExecutor InternalExecutor { get; set; }

        protected NetworkEventLoop EventLoop
        {
            get { return EventLoopFactory.CreateNetworkEventLoop(Workers, InternalExecutor); }
        }

        protected int Workers { get; set; }

        protected int BufferBytes { get; set; }

        protected bool UseProxies { get; set; }

        protected bool UseSharedFiber { get; set; }

        public ServerBootstrap WorkersShareFiber(bool shareFiber)
        {
            UseSharedFiber = shareFiber;
            SetOption("proxiesShareFiber", UseSharedFiber);
            return this;
        }

        public new ServerBootstrap SetTransport(TransportType type)
        {
            base.SetTransport(type);
            return this;
        }

        public ServerBootstrap WorkerThreads(int workerThreadCount)
        {
            if (workerThreadCount < 1) throw new ArgumentException("Can't be below 1", "workerThreadCount");
            Workers = workerThreadCount;
            return this;
        }

        public ServerBootstrap BufferSize(int bufferSize)
        {
            if (bufferSize < 1024) throw new ArgumentException("Can't be below 1024", "bufferSize");
            BufferBytes = bufferSize;
            return this;
        }

        public ServerBootstrap WorkersAreProxies(bool useProxies)
        {
            UseProxies = useProxies;
            return this;
        }

        public ServerBootstrap Executor(IExecutor executor)
        {
            if (executor == null) throw new ArgumentNullException("executor");
            InternalExecutor = executor;
            return this;
        }

        public new ServerBootstrap SetConfig(IConnectionConfig config)
        {
            base.SetConfig(config);
            return this;
        }

        public new ServerBootstrap SetDecoder(IMessageDecoder decoder)
        {
            base.SetDecoder(decoder);
            return this;
        }

        public new ServerBootstrap SetEncoder(IMessageEncoder encoder)
        {
            base.SetEncoder(encoder);
            return this;
        }

        public new ServerBootstrap SetAllocator(IByteBufAllocator allocator)
        {
            base.SetAllocator(allocator);
            return this;
        }

        public new ServerBootstrap OnConnect(ConnectionEstablishedCallback connectionEstablishedCallback)
        {
            base.OnConnect(connectionEstablishedCallback);
            return this;
        }

        public new ServerBootstrap OnDisconnect(ConnectionTerminatedCallback connectionTerminatedCallback)
        {
            base.OnDisconnect(connectionTerminatedCallback);
            return this;
        }

        public new ServerBootstrap OnReceive(ReceivedDataCallback receivedDataCallback)
        {
            base.OnReceive(receivedDataCallback);
            return this;
        }

        public new ServerBootstrap OnError(ExceptionCallback exceptionCallback)
        {
            base.OnError(exceptionCallback);
            return this;
        }

        public new ServerBootstrap SetOption(string optionKey, object optionValue)
        {
            base.SetOption(optionKey, optionValue);
            return this;
        }

        public override void Validate()
        {
            if (Type == TransportType.All) throw new ArgumentException("Type must be set");
            if (Workers < 1) throw new ArgumentException("Workers must be at least 1");
            if (BufferBytes < 1024) throw new ArgumentException("BufferSize must be at least 1024");
        }

        protected override IConnectionFactory BuildInternal()
        {
            switch (Type)
            {
                case TransportType.Tcp:
                    return new TcpServerFactory(this);
                case TransportType.Udp:
                    return new UdpServerFactory(this);
                default:
                    throw new InvalidOperationException("This shouldn't happen");
            }
        }

        public new IServerFactory Build()
        {
            return (IServerFactory) BuildInternal();
        }
    }
}