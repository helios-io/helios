using System;

namespace Helios
{
    public class HeliosException : Exception
    {
        public HeliosException(string message, Exception inner = null) : base(message, inner) { }
    }
}
