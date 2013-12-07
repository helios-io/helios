using System;
using System.Data;
using System.IO;
using Helios.Net;
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

        public bool TryDeserialize<T>(out T obj, NetworkData data)
        {
            using(var memoryStream = data.ToStream())
            {
                return TryDeserialize(out obj, memoryStream);
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

        public T Deserialize<T>(NetworkData data)
        {
            using (var memoryStream = data.ToStream())
            {
                return Deserialize<T>(memoryStream);
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

        public void Serialize<T>(T obj, NetworkData data)
        {
            using (var memoryStream = new MemoryStream(1024))
            {
                Serialize(obj, memoryStream);
                data.Buffer = new byte[memoryStream.Length];
                memoryStream.Write(data.Buffer, 0, data.Buffer.Length);
                data.Length = data.Buffer.Length;
            }
        }
    }
}