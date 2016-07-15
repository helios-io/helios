// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Collections.Concurrent;
using Helios.Logging;

namespace Helios.FsCheck.Tests
{
    /// <summary>
    ///     <see cref="ILogger" /> implementation used for debugging and testing purposes.
    /// </summary>
    public class TestLogger : LoggingAdapter
    {
        public TestLogger(string logSource) : base(logSource)
        {
        }

        public TestLogger(string logSource, params LogLevel[] supportedLogLevels) : base(logSource, supportedLogLevels)
        {
        }

        public ConcurrentQueue<LogEvent> Events { get; } = new ConcurrentQueue<LogEvent>();

        protected override void DebugInternal(Debug message)
        {
            QueueMessage(message);
        }

        protected override void InfoInternal(Info message)
        {
            QueueMessage(message);
        }

        protected override void WarningInternal(Warning message)
        {
            QueueMessage(message);
        }

        protected override void ErrorInternal(Error message)
        {
            QueueMessage(message);
        }

        private void QueueMessage(LogEvent message)
        {
            Events.Enqueue(message);
        }
    }
}