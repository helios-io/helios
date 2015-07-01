using System.IO;
using Helios.Net;

namespace Helios.Serialization
{
    /// <summary>
    /// A binary serializer interface for working with messages
    /// sent over IConnection and ITransport objects
    /// </summary>
    public interface ITransportSerializer
    {
        bool TryDeserialize<T>(out T obj, Stream stream);

        bool TryDeserialize<T>(out T obj, NetworkData data);

        T Deserialize<T>(Stream stream);

        T Deserialize<T>(NetworkData data);

        void Serialize<T>(T obj, Stream stream);

        void Serialize<T>(T obj, NetworkData data);
    }
}
