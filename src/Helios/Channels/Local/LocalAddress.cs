// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Diagnostics.Contracts;
using System.Net;
using System.Text;

namespace Helios.Channels.Local
{
    public class LocalAddress : EndPoint
    {
        public static readonly LocalAddress Any = new LocalAddress("any");

        private readonly string _strVal;

        /// <summary>
        ///     Creates a new ephemeral port based on the ID of the specified <see cref="IChannel" />
        /// </summary>
        /// <param name="channel"></param>
        public LocalAddress(IChannel channel)
        {
            var sb = new StringBuilder(16);
            sb.Append("local:E");
            sb.Append(channel.GetHashCode() & 0xFFFFFFFFL | 0x100000000L);
            sb[7] = ':';
            _strVal = sb.ToString();
            Id = _strVal.Substring(6);
        }

        public LocalAddress(string id)
        {
            Contract.Requires(!string.IsNullOrEmpty(id));
            id = id.Trim().ToLowerInvariant();
            Contract.Assert(!string.IsNullOrEmpty(id));
            Id = id;
            _strVal = "local:" + id;
        }

        public string Id { get; }

        protected bool Equals(LocalAddress other)
        {
            return string.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((LocalAddress) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return _strVal;
        }
    }
}