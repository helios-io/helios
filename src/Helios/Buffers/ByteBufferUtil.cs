namespace Helios.Buffers
{
    /// <summary>
    /// Utility class for working with direct <see cref="ByteBuffer"/> instances
    /// </summary>
    public static class ByteBufferUtil
    {
        /// <summary>
        /// Default initial capacity = 4mb
        /// </summary>
        public const int DEFAULT_INITIAL_CAPACITY = 1048576*4;

        /// <summary>
        /// Default max capacity = 80mb
        /// </summary>
        public const int DEFAULT_MAX_CAPACITY = DEFAULT_INITIAL_CAPACITY*20;
    }
}
