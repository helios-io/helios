using System;
using Helios.Util;

namespace Helios.Logging
{
    /// <summary>
    /// <see cref="ILogger"/> implementation which writes messages out to <see cref="Console"/>.
    /// </summary>
    public class StdOutLogger : LoggingAdapter
    {
        static StdOutLogger()
        {
            DebugColor = ConsoleColor.Gray;
            InfoColor = ConsoleColor.White;
            WarningColor = ConsoleColor.Yellow;
            ErrorColor = ConsoleColor.Red;
            UseColors = true;
        }

        /// <summary>
        /// Gets or Sets the color of Debug events.
        /// </summary>
        public static ConsoleColor DebugColor { get; set; }

        /// <summary>
        /// Gets or Sets the color of Info events.
        /// </summary>
        public static ConsoleColor InfoColor { get; set; }

        /// <summary>
        /// Gets or Sets the color of Warning events.
        /// </summary>
        public static ConsoleColor WarningColor { get; set; }

        /// <summary>
        /// Gets or Sets the color of Error events. 
        /// </summary>
        public static ConsoleColor ErrorColor { get; set; }

        /// <summary>
        /// Gets or Sets whether or not to use colors when printing events.
        /// </summary>
        public static bool UseColors { get; set; }

        public StdOutLogger(string logSource) : base(logSource)
        {
        }

        public StdOutLogger(string logSource, params LogLevel[] supportedLogLevels) : base(logSource, supportedLogLevels)
        {
        }

        protected override void DebugInternal(Debug message)
        {
            StandardOutWriter.WriteLine(message.ToString(), DebugColor);
        }

        protected override void InfoInternal(Info message)
        {
            StandardOutWriter.WriteLine(message.ToString(), InfoColor);
        }

        protected override void WarningInternal(Warning message)
        {
            StandardOutWriter.WriteLine(message.ToString(), WarningColor);
        }

        protected override void ErrorInternal(Error message)
        {
            StandardOutWriter.WriteLine(message.ToString(), ErrorColor);
        }
    }
}