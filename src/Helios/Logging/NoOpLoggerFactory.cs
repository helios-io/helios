// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

namespace Helios.Logging
{
    /// <summary>
    ///     Factory for creating <see cref="NoOpLogger" />s, which don't actually log anything
    /// </summary>
    public class NoOpLoggerFactory : LoggingFactory
    {
        protected override ILogger NewInstance(string name, params LogLevel[] supportedLogLevels)
        {
            return NoOpLogger.Instance;
        }
    }
}