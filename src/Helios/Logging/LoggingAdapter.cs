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
        protected LoggingAdapter(string logSource) : this(logSource, LogLevel.Debug, LogLevel.Error, LogLevel.Info, LogLevel.Warning) { }

        protected LoggingAdapter(string logSource, params LogLevel[] supportedLogLevels)
        {
            Contract.Requires(supportedLogLevels != null);
            LogSource = logSource;
            _supportedLogLevels = supportedLogLevels;

            // set all internal log levels
            IsDebugEnabled = supportedLogLevels.Contains(LogLevel.Debug);
            IsErrorEnabled = supportedLogLevels.Contains(LogLevel.Error);
            IsWarningEnabled = supportedLogLevels.Contains(LogLevel.Warning);
            IsInfoEnabled = supportedLogLevels.Contains(LogLevel.Info);
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

        public void Debug(string format, params object[] args)
        {
            if (IsDebugEnabled)
                DebugInternal(new Debug(string.Format(format, args), LogSource));
        }

        protected abstract void DebugInternal(Debug message);

        public void Info(string format, params object[] args)
        {
            if (IsInfoEnabled)
                InfoInternal(new Info(string.Format(format, args), LogSource));
        }

        protected abstract void InfoInternal(Info message);

        public void Warning(string format, params object[] args)
        {
            if (IsWarningEnabled)
                WarningInternal(new Warning(string.Format(format, args), LogSource));
        }

        public void Warning(Exception cause, string format, params object[] args)
        {
            if (IsWarningEnabled)
                WarningInternal(new Warning(cause, string.Format(format, args), LogSource));
        }

        protected abstract void WarningInternal(Warning message);

        public void Error(string format, params object[] args)
        {
            if (IsErrorEnabled)
                ErrorInternal(new Error(string.Format(format, args), LogSource));
        }

        public void Error(Exception cause, string format, params object[] args)
        {
            if (IsErrorEnabled)
                ErrorInternal(new Error(cause, string.Format(format, args), LogSource));
        }

        protected abstract void ErrorInternal(Error message);

        public void Log(LogLevel logLevel, string format, params object[] args)
        {
            switch (logLevel)
            {
                case LogLevel.Info:
                    Info(format, args);
                    break;
                case LogLevel.Warning:
                    Warning(format, args);
                    break;
                case LogLevel.Error:
                    Error(format, args);
                    break;
                case LogLevel.Debug:
                    Debug(format, args);
                    break;
            }
        }
    }
}