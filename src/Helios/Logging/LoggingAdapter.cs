using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;

namespace Helios.Logging
{
    /// <summary>
    /// Abstract base class that implements most of the expected logger behavior
    /// </summary>
    public abstract class LoggingAdapter : ILogger
    {
        private readonly LogLevel[] _supportedLogLevels;

        

        /// <summary>
        /// All <see cref="LogLevel"/>s are enabled by default.
        /// </summary>
        protected LoggingAdapter(string logSource, Type logType) : this(logSource, logType, LogLevel.DebugLevel, LogLevel.ErrorLevel, LogLevel.InfoLevel, LogLevel.WarningLevel) { }

        protected LoggingAdapter(string logSource, Type logType, params LogLevel[] supportedLogLevels)
        {
            Contract.Requires(supportedLogLevels != null);
            LogSource = logSource;
            LogType = logType;
            _supportedLogLevels = supportedLogLevels;

            // set all internal log levels
            IsDebugEnabled = supportedLogLevels.Contains(LogLevel.DebugLevel);
            IsErrorEnabled = supportedLogLevels.Contains(LogLevel.ErrorLevel);
            IsWarningEnabled = supportedLogLevels.Contains(LogLevel.WarningLevel);
            IsInfoEnabled = supportedLogLevels.Contains(LogLevel.InfoLevel);
        }

        public bool IsDebugEnabled { get; }
        public bool IsInfoEnabled { get; }
        public bool IsWarningEnabled { get; }
        public bool IsErrorEnabled { get; }


        public bool IsEnabled(LogLevel logLevel)
        {
            return _supportedLogLevels.Contains(logLevel);
        }

        public string LogSource { get; }
        public Type LogType { get; }

        public void Debug(string format, params object[] args)
        {
            if (IsDebugEnabled)
                DebugInternal(FormatLog(LogLevel.DebugLevel, format, args));
        }

        protected abstract void DebugInternal(string message);

        public void Info(string format, params object[] args)
        {
            if (IsInfoEnabled)
                InfoInternal(FormatLog(LogLevel.InfoLevel, format, args));
        }

        protected abstract void InfoInternal(string message);

        public void Warning(string format, params object[] args)
        {
            if (IsWarningEnabled)
                WarningInternal(FormatLog(LogLevel.WarningLevel, format, args));
        }

        protected abstract void WarningInternal(string message);

        public void Error(string format, params object[] args)
        {
            if (IsErrorEnabled)
                ErrorInternal(FormatLog(LogLevel.ErrorLevel, format, args));
        }

        public void Error(Exception cause, string format, params object[] args)
        {
            if (IsErrorEnabled)
                ErrorInternal(FormatLog(cause, LogLevel.ErrorLevel, format, args));
        }

        private string FormatLog(LogLevel level, string format, params object[] args)
        {
            return
                $"[{level}][{DateTime.UtcNow}][{Thread.CurrentThread.ManagedThreadId}][{LogSource}] {string.Format(format, args)}";
        }

        private string FormatLog(Exception ex, LogLevel level, string format, params object[] args)
        {
            return
                $"[{level}][{DateTime.UtcNow}][{Thread.CurrentThread.ManagedThreadId}][{LogSource}] {string.Format(format, args)} {Environment.NewLine}Cause: {ex}";
        }

        protected abstract void ErrorInternal(string message);

        public void Log(LogLevel logLevel, string format, params object[] args)
        {
            switch (logLevel)
            {
                case LogLevel.InfoLevel:
                    Info(format, args);
                    break;
                case LogLevel.WarningLevel:
                    Warning(format, args);
                    break;
                case LogLevel.ErrorLevel:
                    Error(format, args);
                    break;
                case LogLevel.DebugLevel:
                    Debug(format, args);
                    break;
            }
        }
    }
}