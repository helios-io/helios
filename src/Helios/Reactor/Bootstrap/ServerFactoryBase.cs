// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using Helios.Net;
using Helios.Topology;

namespace Helios.Reactor.Bootstrap
{
    /// <summary>
    /// A <see cref="IServerFactory"/> instance that spawns <see cref="IReactor"/> instances with UDP transport enabled
    /// </summary>
    public abstract class ServerFactoryBase : ServerBootstrap, IServerFactory
    {
        protected ServerFactoryBase(ServerBootstrap other)
            : base(other)
        {
        }

        protected abstract ReactorBase NewReactorInternal(INode listenAddress);

        public IReactor NewReactor(INode listenAddress)
        {
            var reactor = NewReactorInternal(listenAddress);
            reactor.Configure(Config);

            if (ReceivedData != null)
                reactor.OnReceive += (ReceivedDataCallback) ReceivedData.Clone();
            if (ConnectionEstablishedCallback != null)
                reactor.OnConnection += (ConnectionEstablishedCallback) ConnectionEstablishedCallback.Clone();
            if (ConnectionTerminatedCallback != null)
                reactor.OnDisconnection += (ConnectionTerminatedCallback) ConnectionTerminatedCallback.Clone();
            if (ExceptionCallback != null)
                reactor.OnError += (ExceptionCallback) ExceptionCallback.Clone();

            return reactor;
        }

        public IConnection NewConnection()
        {
            return NewConnection(Node.Any());
        }

        public IConnection NewConnection(INode localEndpoint)
        {
            var reactor = (ReactorBase) NewReactor(localEndpoint);
            return reactor.ConnectionAdapter;
        }

        public IConnection NewConnection(INode localEndpoint, INode remoteEndpoint)
        {
            return NewConnection(localEndpoint);
        }
    }
}