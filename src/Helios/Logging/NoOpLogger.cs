using System;

namespace Helios.Logging
{
    /// <summary>
    /// Logger that does nothing.
    /// </summary>
    public class NoOpLogger : LoggingAdapter
    {
        private NoOpLogger() : base(typeof(NoOpLogger).FullName, new LogLevel[0]) { }

        public static NoOpLogger Instance = new NoOpLogger();


        protected override void DebugInternal(Debug message)
        {
            
        }

        protected override void InfoInternal(Info message)
        {
            
        }

        protected override void WarningInternal(Warning message)
        {
            
        }

        protected override void ErrorInternal(Error message)
        {
            
        }
    }
}
