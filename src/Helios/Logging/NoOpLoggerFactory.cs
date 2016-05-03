namespace Helios.Logging
{
    /// <summary>
    /// Factory for creating <see cref="NoOpLogger"/>s, which don't actually log anything
    /// </summary>
    public class NoOpLoggerFactory : LoggingFactory
    {
        protected override ILogger NewInstance(string name, params LogLevel[] supportedLogLevels)
        {
            return NoOpLogger.Instance;
        }
    }
}