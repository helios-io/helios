// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using Helios.Tracing.Impl;
using Helios.Util;

namespace Helios.Tracing
{
    /// <summary>
    ///     Static class that acts as a container for all built-in tracing
    ///     that can occur inside Helios core.
    ///     Does not expose definitions for user-defined tracing (yet.)
    /// </summary>
    public static class HeliosTrace
    {
        /// <summary>
        ///     The default <see cref="IHeliosTraceWriter" />.
        /// </summary>
        public static readonly IHeliosTraceWriter Default = new NoOpHeliosTraceWriter();

        /// <summary>
        ///     The global instance of the <see cref="IHeliosTraceWriter" /> helios will use for internal tracing.
        /// </summary>
        public static IHeliosTraceWriter Instance { get; private set; } = Default;

        /// <summary>
        ///     Set the <see cref="IHeliosTraceWriter" /> that will be used across all built-in Helios calls.
        ///     Not synchronized, and not intended to be called in concurrent code.
        /// </summary>
        /// <param name="writer">The <see cref="IHeliosTraceWriter" /> implementation that will be used internally</param>
        public static void SetWriter(IHeliosTraceWriter writer)
        {
            Guard.True(writer != null);
            Instance = writer;
        }
    }
}