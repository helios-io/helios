using System.Collections.Generic;
using Helios.Buffers;
using Helios.Net;

namespace Helios.Serialization
{
    /// <summary>
    /// Used to encode <see cref="NetworkData"/> inside Helios
    /// </summary>
    public interface IMessageDecoder
    {
        /// <summary>
        /// Encodes <see cref="data"/> into a format that's acceptable for <see cref="IConnection"/>.
        /// 
        /// Might return a list of decoded objects in <see cref="decoded"/>, and it's up to the handler to determine
        /// what to do with them.
        /// </summary>
        void Decode(NetworkData data, out List<NetworkData> decoded);

        void Decode(IConnection connection, IByteBuf buffer, out List<IByteBuf> decoded);
    }

    public abstract class MessageDecoderBase : IMessageDecoder
    {
        public abstract void Decode(NetworkData data, out List<NetworkData> decoded);
        public abstract void Decode(IConnection connection, IByteBuf buffer, out List<IByteBuf> decoded);
    }

    /// <summary>
    /// Dummy decoder that doesn't actually do anything
    /// </summary>
    public class NoOpDecoder : MessageDecoderBase
    {
        public override void Decode(NetworkData data, out List<NetworkData> decoded)
        {
            decoded = new List<NetworkData>() {data};
        }

        public override void Decode(IConnection connection, IByteBuf buffer, out List<IByteBuf> decoded)
        {
            decoded = new List<IByteBuf>() {buffer};
        }
    }
}