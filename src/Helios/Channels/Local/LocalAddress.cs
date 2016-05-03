using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Channels.Local
{
    public class LocalAddress : EndPoint
    {
        public static readonly LocalAddress Any = new LocalAddress("any");

        private readonly string _id;
        private readonly string _strVal;

        /// <summary>
        /// Creates a new ephemeral port based on the ID of the specified <see cref="IChannel"/>
        /// </summary>
        /// <param name="channel"></param>
        public LocalAddress(IChannel channel)
        {
            var sb = new StringBuilder(16);
            sb.Append("local:E");
            sb.Append(channel.GetHashCode() & 0xFFFFFFFFL | 0x100000000L);
            sb[7] = ':';
            _strVal = sb.ToString();
            _id = _strVal.Substring(6);
        }

        public LocalAddress(string id)
        {
            Contract.Requires(!string.IsNullOrEmpty(id));
            id = id.Trim().ToLowerInvariant();
            Contract.Assert(!string.IsNullOrEmpty(id));
            _id = id;
            _strVal = "local:" + id;
        }

        public string Id => _id;

        protected bool Equals(LocalAddress other)
        {
            return string.Equals(_id, other._id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LocalAddress) obj);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public override string ToString()
        {
            return _strVal;
        }
    }
}
