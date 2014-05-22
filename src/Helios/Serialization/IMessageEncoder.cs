using System.Collections.Generic;
using Helios.Buffers;
using Helios.Net;

namespace Helios.Serialization
{
    /// <summary>
    /// Used to encode <see cref="NetworkData"/> inside Helios
    /// </summary>
    public interface IMessageEncoder
    {
        /// <summary>
        /// Encodes <see cref="data"/> into a format that's acceptable for <see cref="IConnection"/>.
        /// 
        /// Might return a list of encoded objects in <see cref="encoded"/>, and it's up to the handler to determine
        /// what to do with them.
        /// </summary>
        void Encode(NetworkData data, out List<NetworkData> encoded);

        void Encode(IByteBuf buffer, out List<byte[]> encoded);
    }

    public abstract class MessageEncoderBase : IMessageEncoder
    {
        public abstract void Encode(NetworkData data, out List<NetworkData> encoded);
        public abstract void Encode(IByteBuf buffer, out List<byte[]> encoded);
    }

    /// <summary>
    /// Does nothing - default encoder
    /// </summary>
    public class NoOpEncoder : MessageEncoderBase
    {
        public override void Encode(NetworkData data, out List<NetworkData> encoded)
        {
            encoded = new List<NetworkData>() {data};
        }

        public override void Encode(IByteBuf buffer, out List<byte[]> encoded)
        {
            var data = buffer.ToArray();
            encoded = new List<byte[]>() {data};
        }
    }
}
