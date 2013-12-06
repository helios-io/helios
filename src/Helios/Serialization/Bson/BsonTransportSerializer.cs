using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Helios.Serialization.Bson
{
    public class BsonTransportSerializer : ITransportSerializer
    {
        public bool TryDeserialize<T>(out T obj, Stream stream)
        {
            try
            {
                using (var bson = new BsonReader(stream))
                {
                    var jsonSerializer = new JsonSerializer();
                    obj = jsonSerializer.Deserialize<T>(bson);
                }
                return true;
            }
            catch
            {
                obj = default(T);
                return false;
            }
        }

        public T Deserialize<T>(Stream stream)
        {
            using (var bson = new BsonReader(stream))
            {
                var jsonSerializer = new JsonSerializer();
                return jsonSerializer.Deserialize<T>(bson);
            }
        }

        public void Serialize<T>(T obj, Stream stream)
        {
            using (var bson = new BsonWriter(stream))
            {
                var jsonSerializer = new JsonSerializer();
                jsonSerializer.Serialize(bson, obj);
            }
        }
    }
}