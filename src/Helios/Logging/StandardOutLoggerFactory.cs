using System;

namespace Helios.Logging
{
    /// <summary>
    /// Creates <see cref="StdOutLogger"/> instances.
    /// </summary>
    public class StandardOutLoggerFactory : LoggingFactory
    {
        protected override ILogger NewInstance(string name, Type source)
        {
            return new StdOutLogger(name, source);
        }
    }
}