using System.IO;

namespace Helios.Channels
{
    public class ConnectTimeoutException : IOException
    {
        public ConnectTimeoutException(string message)
            : base(message)
        {
        }

        public ConnectTimeoutException()
        {
        }
    }
}