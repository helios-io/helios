using System.IO;

namespace Helios.Net.Transports
{
    public interface IStreamTransport : ITransport
    {
        Stream OutputStream { get; }

        Stream InputStream { get; }

        void CloseStreams();

        void DisposeStreams(bool disposing);
    }
}