// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Threading;

namespace Helios.Logging
{
    /// <summary>
    ///     An event instance produced by a <see cref="ILogger" />
    /// </summary>
    public abstract class LogEvent
    {
        protected LogEvent(string message, string logSource, LogLevel level)
        {
            Message = message;
            LogSource = logSource;
            Level = level;
            Timestamp = DateTime.UtcNow;
            ThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        public DateTime Timestamp { get; }

        public string Message { get; }

        public string LogSource { get; }

        public int ThreadId { get; }

        public LogLevel Level { get; }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this LogEvent.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this LogEvent.</returns>
        public override string ToString()
        {
            return
                $"[{Level.ToString().ToUpperInvariant()}][{Timestamp}][Thread {ThreadId.ToString().PadLeft(4, '0')}][{LogSource}] {Message}";
        }
    }

    /// <summary>
    ///     <see cref="LogLevel.Error" /> events
    /// </summary>
    public class Error : LogEvent
    {
        public Error(string message, string logSource) : base(message, logSource, LogLevel.Error)
        {
        }

        public Error(Exception cause, string message, string logSource) : base(message, logSource, LogLevel.Error)
        {
            Cause = cause;
        }

        public Exception Cause { get; }

        public override string ToString()
        {
            return base.ToString() + Environment.NewLine + $"Cause: {Cause?.ToString() ?? "Unknown"}";
        }
    }

    /// <summary>
    ///     <see cref="LogLevel.Warning" /> events
    /// </summary>
    public class Warning : LogEvent
    {
        public Warning(string message, string logSource) : base(message, logSource, LogLevel.Warning)
        {
        }

        public Warning(Exception cause, string message, string logSource) : base(message, logSource, LogLevel.Error)
        {
            Cause = cause;
        }

        public Exception Cause { get; }

        public override string ToString()
        {
            return base.ToString() + Environment.NewLine + $"Cause: {Cause?.ToString() ?? "Unknown"}";
        }
    }

    /// <summary>
    ///     <see cref="LogLevel.Info" /> events
    /// </summary>
    public class Info : LogEvent
    {
        public Info(string message, string logSource) : base(message, logSource, LogLevel.Info)
        {
        }
    }

    /// <summary>
    ///     <see cref="LogLevel.Debug" /> events
    /// </summary>
    public class Debug : LogEvent
    {
        public Debug(string message, string logSource) : base(message, logSource, LogLevel.Debug)
        {
        }
    }
}