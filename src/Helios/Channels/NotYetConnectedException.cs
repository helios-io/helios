using System.IO;

namespace Helios.Channels
{
    public class NotYetConnectedException : IOException
    {
        public static readonly NotYetConnectedException Instance = new NotYetConnectedException();

        private NotYetConnectedException()
        {
        }
    }
}