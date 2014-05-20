using System.Collections.Generic;
using Helios.Net;

namespace Helios.Serialization
{
    /// <summary>
    /// Used to encode <see cref="NetworkData"/> inside Helios
    /// </summary>
    public interface IMessageEncoder
    {
        /// <summary>
        /// Write data to the <see cref="IConnection"/>.
        /// 
        /// <see cref="data"/> will be encoded by the <see cref="Encode"/> method before being written.
        /// </summary>
        void Write(IConnection connection, NetworkData data);

        /// <summary>
        /// Encodes <see cref="data"/> into a format that's acceptable for <see cref="IConnection"/>.
        /// 
        /// Might return a list of encoded objects in <see cref="encoded"/>, and it's up to the handler to determine
        /// what to do with them.
        /// </summary>
        void Encode(IConnection connection, NetworkData data, out List<NetworkData> encoded);
    }

    public abstract class MessageEncoderBase : IMessageEncoder
    {
        public void Write(IConnection connection, NetworkData data)
        {
            List<NetworkData> messages;
            Encode(connection, data, out messages);
            foreach (var message in messages)
            {
                connection.Send(message);
            }
        }

        public abstract void Encode(IConnection connection, NetworkData data, out List<NetworkData> encoded);
    }

    /// <summary>
    /// Does nothing - default encoder
    /// </summary>
    public class NoOpEncoder : MessageEncoderBase
    {
        public override void Encode(IConnection connection, NetworkData data, out List<NetworkData> encoded)
        {
            encoded = new List<NetworkData>() {data};
        }
    }
}
