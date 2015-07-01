using Helios.Tracing.Impl;
using Helios.Util;

namespace Helios.Tracing
{
    /// <summary>
    /// Static class that acts as a container for all built-in tracing
    /// that can occur inside Helios core.
    /// 
    /// Does not expose definitions for user-defined tracing (yet.)
    /// </summary>
    public static class HeliosTrace
    {
        /// <summary>
        /// The global instance of the <see cref="IHeliosTraceWriter"/> helios will use for internal tracing.
        /// </summary>
        public static IHeliosTraceWriter Instance { get { return _instance; } }

        /// <summary>
        /// Set the <see cref="IHeliosTraceWriter"/> that will be used across all built-in Helios calls.
        /// 
        /// Not synchronized, and not intended to be called in concurrent code.
        /// </summary>
        /// <param name="writer">The <see cref="IHeliosTraceWriter"/> implementation that will be used internally</param>
        public static void SetWriter(IHeliosTraceWriter writer)
        {
            Guard.True(writer != null);
            _instance = writer;
        }

        /// <summary>
        /// The default <see cref="IHeliosTraceWriter"/>.
        /// </summary>
        public readonly static IHeliosTraceWriter Default = new NoOpHeliosTraceWriter();

        private static IHeliosTraceWriter _instance = Default;
    }
}
