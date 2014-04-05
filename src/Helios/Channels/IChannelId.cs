using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Helios.Net;
using Helios.Util;

namespace Helios.Channels
{
    /// <summary>
    /// Interface used for describing a channel's ID
    /// </summary>
    public interface IChannelId : IComparable<IChannelId>, IEquatable<IChannelId>
    {
    }

    public sealed class DefaultChannelId : IChannelId
    {
        private byte[] _data;
        private string _stringValue;

        private int hashCode;
        internal void Init()
        {
            using (var memoryStream = new MemoryStream())
            {
                //ProcessId
                var bytes = BitConverter.GetBytes(ProcessId);
                memoryStream.Write(bytes, 0, bytes.Length);

                //MachineId
                bytes = SystemAddressHelper.ConnectedMacAddress.GetAddressBytes();
                memoryStream.Write(bytes, 0, bytes.Length);

                //Sequence
                var sequence = SequenceCounter.GetAndIncrement();
                bytes = BitConverter.GetBytes(sequence);
                memoryStream.Write(bytes, 0, bytes.Length);

                //DateTime
                var currentMs = (DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond);
                bytes = BitConverter.GetBytes(currentMs);
                memoryStream.Write(bytes, 0, bytes.Length);

                //Random
                var random = ThreadLocalRandom.Current.Next();
                hashCode = random;
                bytes = BitConverter.GetBytes(random);
                memoryStream.Write(bytes, 0, bytes.Length);

                _data = memoryStream.GetBuffer();
            }
        }

        #region Static Members

        /// <summary>
        /// Creates a new <see cref="IChannelId"/> instance
        /// </summary>
        /// <returns>A new <see cref="IChannelId"/> instance</returns>
        public static IChannelId NewChanneldId()
        {
            var channelId = new DefaultChannelId();
            channelId.Init();
            return channelId;
        }

        static int ProcessId
        {
            get { return Process.GetCurrentProcess().Id; }
        }

        static readonly AtomicCounter SequenceCounter = new AtomicCounter(0);

        #endregion

        #region IComparable<IChannelId> members

        public int CompareTo(IChannelId other)
        {
            return String.Compare(ToString(), other.ToString(), StringComparison.Ordinal);
        }

        #endregion

        #region IEquatable<IChannelId> members

        public bool Equals(IChannelId other)
        {
            return ToString().Equals(other.ToString());
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(_stringValue)) return _stringValue;

            var hex = new StringBuilder(_data.Length * 2);
            foreach (var b in _data)
                hex.AppendFormat("{0:x2}", b);
            _stringValue = hex.ToString();

            return _stringValue;
        }

        public override int GetHashCode()
        {
// ReSharper disable once NonReadonlyFieldInGetHashCode
            return hashCode;
        }

        #endregion
    }
}
