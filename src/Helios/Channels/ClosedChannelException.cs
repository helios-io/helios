using System.IO;

namespace Helios.Channels
{
    public class ClosedChannelException : IOException
    {
        public static readonly ClosedChannelException Instance = new ClosedChannelException();

        private ClosedChannelException()
        {
        }
    }
}