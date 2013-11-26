using System.IO;

namespace Helios.Core.Serialization
{
    /// <summary>
    /// A binary serializer interface for working with messages
    /// sent over IConnection and ITransport objects
    /// </summary>
    public interface ITransportSerializer
    {
        bool TryDeserialize<T>(out T obj, byte[] buffer, int offset, int length);

        bool TryDeserialize<T>(out T obj, byte[] buffer);

        T Deserialize<T>(byte[] buffer);

        T Deserialize<T>(byte[] buffer, int offset, int length);

        void Serialize<T>(T obj, Stream stream);

        void Serialize<T>(T obj, byte[] buffer, int offset, int length);
    }
}
