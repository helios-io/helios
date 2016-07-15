// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

namespace Helios.Logging
{
    /// <summary>
    ///     Logger that does nothing.
    /// </summary>
    public class NoOpLogger : LoggingAdapter
    {
        public static NoOpLogger Instance = new NoOpLogger();

        private NoOpLogger() : base(typeof(NoOpLogger).FullName, new LogLevel[0])
        {
        }


        protected override void DebugInternal(Debug message)
        {
        }

        protected override void InfoInternal(Info message)
        {
        }

        protected override void WarningInternal(Warning message)
        {
        }

        protected override void ErrorInternal(Error message)
        {
        }
    }
}