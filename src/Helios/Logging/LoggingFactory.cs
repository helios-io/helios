﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading;

namespace Helios.Logging
{
    /// <summary>
    /// Factory class for acquiring <see cref="ILogger"/> instances.
    /// </summary>
    public abstract class LoggingFactory
    {
        private static LoggingFactory _defaultFactory;

        static LoggingFactory NewDefaultFactory(string name)
        {
            var loggingFactory = new StandardOutLoggerFactory();
            loggingFactory.NewInstance(name).Debug("Using Standard Out as the default logging system.");
            return loggingFactory;
        }

        /// <summary>
        /// Gets or sets the default <see cref="LoggingFactory"/> used by Helios. Defaults to <see cref="StandardOutLoggerFactory"/>.
        /// </summary>
        public static LoggingFactory DefaultFactory
        {
            get
            {
                var factory = Volatile.Read(ref _defaultFactory);
                if (factory == null)
                {
                    factory = NewDefaultFactory(typeof (LoggingFactory).FullName);
                    var current = Interlocked.CompareExchange(ref _defaultFactory, factory, null);
                    if (current != null)
                        return current;
                }
                return factory;
            }
            set
            {
                Contract.Requires(value != null);
                Volatile.Write(ref _defaultFactory, value);
            }
        }

        protected abstract ILogger NewInstance(string name, params LogLevel[] supportedLogLevels);

        public static ILogger GetLogger<T>(params LogLevel[] supportedLogLevels)
        {
            return GetInstance(typeof (T), supportedLogLevels);
        }

        public static ILogger GetInstance(Type t, params LogLevel[] supportedLogLevels)
        {
            return GetInstance(t.FullName, supportedLogLevels);
        }

        public static ILogger GetInstance(string name, params LogLevel[] supportedLogLevels)
        {
            return DefaultFactory.NewInstance(name, supportedLogLevels);
        }
    }
}
