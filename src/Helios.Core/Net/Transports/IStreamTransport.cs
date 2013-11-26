using System.IO;

namespace Helios.Core.Net.Transports
{
    public interface IStreamTransport : ITransport
    {
        Stream OutputStream { get; }

        Stream InputStream { get; }

        void CloseStreams();

        void DisposeStreams(bool disposing);
    }
}