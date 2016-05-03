using System;

namespace Helios.Channels
{
    public class ConnectException : Exception
    {
        public ConnectException(string s, Exception exception) : base(s, exception)
        {
        }
    }
}