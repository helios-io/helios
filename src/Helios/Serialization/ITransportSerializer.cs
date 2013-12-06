using System.IO;

namespace Helios.Serialization
{
    /// <summary>
    /// A binary serializer interface for working with messages
    /// sent over IConnection and ITransport objects
    /// </summary>
    public interface ITransportSerializer
    {
        bool TryDeserialize<T>(out T obj, Stream stream);

        T Deserialize<T>(Stream stream);

        void Serialize<T>(T obj, Stream stream);
    }
}
