using System.Collections.Generic;
using Helios.Net;

namespace Helios.Serialization
{
    /// <summary>
    /// Used to encode <see cref="NetworkData"/> inside Helios
    /// </summary>
    public interface IMessageDecoder
    {
        /// <summary>
        /// Read data from <see cref="IConnection"/>.
        /// 
        /// <see cref="data"/> will be encoded by the <see cref="Decode"/> method before being written.
        /// </summary>
        void Read(IConnection connection, NetworkData data);

        /// <summary>
        /// Encodes <see cref="data"/> into a format that's acceptable for <see cref="IConnection"/>.
        /// 
        /// Might return a list of encoded objects in <see cref="decoded"/>, and it's up to the handler to determine
        /// what to do with them.
        /// </summary>
        void Decode(IConnection connection, NetworkData data, out List<NetworkData> decoded);
    }

    public abstract class MessageDecoderBase : IMessageDecoder
    {
        public void Read(IConnection connection, NetworkData data)
        {
            List<NetworkData> messages;
            Decode(connection, data, out messages);
            foreach (var message in messages)
            {
                connection.InvokeReceiveIfNotNull(message);
            }
        }

        public abstract void Decode(IConnection connection, NetworkData data, out List<NetworkData> decoded);
    }
}