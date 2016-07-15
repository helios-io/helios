// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;

namespace Helios.Net
{
    /// <summary>
    ///     constants used by Helios during network operations
    /// </summary>
    public static class NetworkConstants
    {
        public const int DEFAULT_BUFFER_SIZE = 1024*32; //32k

        /// <summary>
        ///     The default backlog value for all socket connection types
        /// </summary>
        public const int DefaultBacklog = 5;

        /// <summary>
        ///     Port used to tell Helios (and dependent applications) to bypass the network stack altogether and just use in-memory
        ///     operations
        /// </summary>
        public const int InMemoryPort = 0;

        /// <summary>
        ///     The default interval used to check-in on blacked out nodes, when not using exponential backoff
        /// </summary>
        public static readonly TimeSpan DefaultNodeRecoveryInterval = TimeSpan.FromSeconds(30);

        /// <summary>
        ///     The default connectivity timeout
        /// </summary>
        public static readonly TimeSpan DefaultConnectivityTimeout = TimeSpan.FromSeconds(30);

        /// <summary>
        ///     All of the exponential back off intervals used for checking the health
        ///     of blacked-out nodes
        /// </summary>
        public static readonly TimeSpan[] BackoffIntervals =
        {
            TimeSpan.FromSeconds(5), //5 seconds
            TimeSpan.FromSeconds(30), //30 seconds
            TimeSpan.FromMinutes(5), //5 minutes
            TimeSpan.FromMinutes(15), //15 minutes
            TimeSpan.FromMinutes(30), //30 minutes
            TimeSpan.FromHours(1), //1 hour
            TimeSpan.FromHours(2), //2 hours
            TimeSpan.FromHours(4), //4 hours
            TimeSpan.FromHours(12), //12 hours
            TimeSpan.FromDays(1), //1 day
            TimeSpan.FromDays(2) //2 days
        };
    }
}