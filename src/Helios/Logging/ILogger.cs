﻿using System;
using System.Security.Cryptography.X509Certificates;

namespace Helios.Logging
{
    /// <summary>
    /// Provides a logging interface used to log events within the system.
    /// </summary>
    public interface ILogger
    {
        /// <summary>Returns <c>true</c> if Debug level is enabled.</summary>
        bool IsDebugEnabled { get; }

        /// <summary>Returns <c>true</c> if Info level is enabled.</summary>
        bool IsInfoEnabled { get; }

        /// <summary>Returns <c>true</c> if Warning level is enabled.</summary>
        bool IsWarningEnabled { get; }

        /// <summary>Returns <c>true</c> if Error level is enabled.</summary>
        bool IsErrorEnabled { get; }

        /// <summary>Returns <c>true</c> if the specified level is enabled.</summary>
        bool IsEnabled(LogLevel logLevel);

        /// <summary>
        /// The name of producer of these log messages
        /// </summary>
        string LogSource { get; }

        /// <summary>Logs a message with the Debug level.</summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        void Debug(string format, params object[] args);

        /// <summary>Logs a message with the Info level.</summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        void Info(string format, params object[] args);

        /// <summary>Logs a message with the Warning level.</summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        void Warning(string format, params object[] args);

        /// <summary>Logs a message with the Warning level.</summary>
        /// <param name="cause">The cause.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        void Warning(Exception cause, string format, params object[] args);

        /// <summary>Logs a message with the Error level.</summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        void Error(string format, params object[] args);

        /// <summary>Logs a message with the Error level.</summary>
        /// <param name="cause">The cause.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        void Error(Exception cause, string format, params object[] args);

        /// <summary>Logs a message with the specified level.</summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        void Log(LogLevel logLevel, string format, params object[] args);
    }
}
