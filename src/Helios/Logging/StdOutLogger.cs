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

        public StdOutLogger(string logSource, Type logType) : base(logSource, logType)
        {
        }

        public StdOutLogger(string logSource, Type logType, params LogLevel[] supportedLogLevels) : base(logSource, logType, supportedLogLevels)
        {
        }

        protected override void DebugInternal(string message)
        {
            StandardOutWriter.WriteLine(message, DebugColor);
        }

        protected override void InfoInternal(string message)
        {
            StandardOutWriter.WriteLine(message, InfoColor);
        }

        protected override void WarningInternal(string message)
        {
            StandardOutWriter.WriteLine(message, WarningColor);
        }

        protected override void ErrorInternal(string message)
        {
            StandardOutWriter.WriteLine(message, ErrorColor);
        }
    }
}