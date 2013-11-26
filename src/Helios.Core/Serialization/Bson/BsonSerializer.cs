using System.IO;
using JsonFx.Bson;
using JsonFx.Json;
using JsonFx.Serialization;

namespace Helios.Core.Serialization.Bson
{
    public class BsonSerializer : ITransportSerializer
    {
        protected readonly BsonReader.BsonTokenizer BsonTokenizer;
        protected readonly BsonWriter.BsonFormatter BsonFormatter;
        protected readonly JsonWriter Writer;
        protected readonly JsonReader Reader;

        public BsonSerializer()
        {
            Writer = new JsonWriter();
            Reader = new JsonReader();
            BsonTokenizer = new BsonReader.BsonTokenizer();
            BsonFormatter = new BsonWriter.BsonFormatter();
        }

        public bool TryDeserialize<T>(out T obj, byte[] buffer, int offset, int length)
        {
            obj = Reader.Read<T>();
        }

        public bool TryDeserialize<T>(out T obj, byte[] buffer)
        {
            throw new System.NotImplementedException();
        }

        public T Deserialize<T>(byte[] buffer)
        {
            throw new System.NotImplementedException();
        }

        public T Deserialize<T>(byte[] buffer, int offset, int length)
        {
            throw new System.NotImplementedException();
        }

        public void Serialize<T>(T obj, Stream stream)
        {
            throw new System.NotImplementedException();
        }

        public void Serialize<T>(T obj, byte[] buffer, int offset, int length)
        {
            throw new System.NotImplementedException();
        }
    }
}