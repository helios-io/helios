namespace Helios.Channels.Impl
{
    /// <summary>
    /// Helper class used in queueing for outbound writes
    /// </summary>
    internal static class Unpooled
    {
        private static byte[] _emptyBuffer = new byte[0];
        public static byte[] EmptyBuffer
        {
            get { return _emptyBuffer; }
        }
    }
}