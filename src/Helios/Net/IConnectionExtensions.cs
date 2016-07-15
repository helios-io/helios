// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using Helios.Net.Builders;
using Helios.Topology;

namespace Helios.Net
{
    /// <summary>
    ///     Extension methods using a default connection builder, to help make it easier to establish ad-hoc connections with
    ///     INode instances
    /// </summary>
    public static class IConnectionExtensions
    {
        public static IConnectionBuilder DefaultConnectionBuilder =
            new NormalConnectionBuilder(NetworkConstants.DefaultConnectivityTimeout);

        public static IConnection GetConnection(this INode node)
        {
            return DefaultConnectionBuilder.BuildConnection(node);
        }
    }
}