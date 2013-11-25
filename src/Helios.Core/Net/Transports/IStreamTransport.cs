using System.IO;

namespace Helios.Core.Net.Transports
{
    public interface IStreamTransport
    {
        Stream OutputStream { get; }

        Stream InputStream { get; }
    }
}