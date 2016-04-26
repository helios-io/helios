using System.Net;

namespace Helios.Channels.Embedded
{
    sealed class EmbeddedSocketAddress : EndPoint
    {
        public override string ToString()
        {
            return "embedded";
        }
    }
}