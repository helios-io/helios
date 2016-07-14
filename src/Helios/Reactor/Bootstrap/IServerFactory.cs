// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.
using Helios.Net.Bootstrap;
using Helios.Topology;

namespace Helios.Reactor.Bootstrap
{
    /// <summary>
    /// Factory interface for creating new <see cref="IReactor"/> instances
    /// </summary>
    [Obsolete()]
    public interface IServerFactory : IConnectionFactory
    {
        IReactor NewReactor(INode listenAddress);
    }
}