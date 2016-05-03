using System;

namespace Helios.Logging
{
    /// <summary>
    /// Creates <see cref="StdOutLogger"/> instances.
    /// </summary>
    public class StandardOutLoggerFactory : LoggingFactory
    {
        protected override ILogger NewInstance(string name, params LogLevel[] supportedLogLevels)
        {
            return new StdOutLogger(name );
        }
    }
}