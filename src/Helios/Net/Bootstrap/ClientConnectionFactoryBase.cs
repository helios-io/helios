// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using Helios.Topology;

namespace Helios.Net.Bootstrap
{
    public abstract class ClientConnectionFactoryBase : ClientBootstrap, IConnectionFactory
    {
        protected ClientConnectionFactoryBase(ClientBootstrap clientBootstrap) : base(clientBootstrap)
        {
        }

        public IConnection NewConnection()
        {
            throw new NotSupportedException("You must specify a remote endpoint for this connection");
        }

        public IConnection NewConnection(INode remoteEndpoint)
        {
            return NewConnection(Node.Any(), remoteEndpoint);
        }

        public IConnection NewConnection(INode localEndpoint, INode remoteEndpoint)
        {
            var connection = CreateConnection(localEndpoint, remoteEndpoint);
            connection.Configure(Config);

            if (ReceivedData != null)
                connection.Receive += (ReceivedDataCallback) ReceivedData.Clone();
            if (ConnectionEstablishedCallback != null)
                connection.OnConnection += (ConnectionEstablishedCallback) ConnectionEstablishedCallback.Clone();
            if (ConnectionTerminatedCallback != null)
                connection.OnDisconnection += (ConnectionTerminatedCallback) ConnectionTerminatedCallback.Clone();
            if (ExceptionCallback != null)
                connection.OnError += (ExceptionCallback) ExceptionCallback.Clone();

            return connection;
        }

        /// <summary>
        ///     Spawns an <see cref="IConnection" /> object internally
        /// </summary>
        protected abstract IConnection CreateConnection(INode localEndpoint, INode remoteEndpoint);
    }
}