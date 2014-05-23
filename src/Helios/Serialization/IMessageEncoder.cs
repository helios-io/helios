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
        /// Encodes <see cref="buffer"/> into a format that's acceptable for <see cref="IConnection"/>.
        /// 
        /// Might return a list of encoded objects in <see cref="encoded"/>, and it's up to the handler to determine
        /// what to do with them.
        /// </summary>
        void Encode(IConnection connection, IByteBuf buffer, out List<IByteBuf> encoded);
    }

    public abstract class MessageEncoderBase : IMessageEncoder
    {
        public abstract void Encode(IConnection connection, IByteBuf buffer, out List<IByteBuf> encoded);
    }

    /// <summary>
    /// Does nothing - default encoder
    /// </summary>
    public class NoOpEncoder : MessageEncoderBase
    {
        public override void Encode(IConnection connection, IByteBuf buffer, out List<IByteBuf> encoded)
        {
            encoded = new List<IByteBuf>() { buffer };
        }
    }
}
