using System;
// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.
using Helios.Topology;

namespace Helios.Net.Bootstrap
{
    /// <summary>
    /// Interface used for spawning new <see cref="IConnection"/> objects
    /// </summary>
    [Obsolete()]
    public interface IConnectionFactory
    {
        IConnection NewConnection();

        IConnection NewConnection(INode remoteEndpoint);

        IConnection NewConnection(INode localEndpoint, INode remoteEndpoint);
    }
}