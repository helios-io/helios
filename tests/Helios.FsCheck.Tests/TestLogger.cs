using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helios.Logging;

namespace Helios.FsCheck.Tests
{
    /// <summary>
    /// <see cref="ILogger"/> implementation used for debugging and testing purposes.
    /// </summary>
    public class TestLogger : LoggingAdapter
    {
        public ConcurrentQueue<LogEvent> Events { get; } = new ConcurrentQueue<LogEvent>();

        public TestLogger(string logSource) : base(logSource)
        {
        }

        public TestLogger(string logSource, params LogLevel[] supportedLogLevels) : base(logSource, supportedLogLevels)
        {
        }

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
