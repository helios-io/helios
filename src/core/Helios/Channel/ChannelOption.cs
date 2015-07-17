using Helios.Buffers;
using Helios.Util;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.NetworkInformation;

namespace Helios.Channel
{
    /// <summary>
    /// Singleton class for managing all default <see cref="ChannelOption{T}"/> instances.
    /// </summary>
    public static class ChannelOption
    {
        #region ChannelOptionPool

        private class ChannelOptionPool : ConstantPool<ChannelOption<object>>
        {
            protected override ChannelOption<object> NewConstant(int id, string name)
            {
                return new ChannelOption<object>(id, name);
            }
        }

        private static readonly ChannelOptionPool Pool = new ChannelOptionPool();

        #endregion

        /// <summary>
        /// Gets or creates a <see cref="ChannelOption{Object}"/> of the
        /// specified type and <see cref="name"/>.
        /// </summary>
        /// <param name="name">The name of the channel option.</param>
        /// <returns>The <see cref="ChannelOption{Object}"/> associated with this name.</returns>
        public static ChannelOption<object> ValueOf(string name)
        {
            return Pool.ValueOf(name);
        }

        /// <summary>
        /// Gets or creates a <see cref="ChannelOption{T}"/> of the
        /// specified type and <see cref="name"/>.
        /// </summary>
        /// <typeparam name="T">The type of the option.</typeparam>
        /// <param name="name">The name of the channel option.</param>
        /// <returns>The <see cref="ChannelOption{T}"/> associated with this name.</returns>
        public static ChannelOption<T> ValueOf<T>(string name)
        {
            return Pool.ValueOf(typeof(T), name);
        }

        /// <summary>
        /// Determine if a <see cref="ChannelOption{T}"/> exists for a given <see cref="name"/>.
        /// </summary>
        /// <param name="name">The name of the channel option.</param>
        /// <returns><c>true</c> if the name exists, <c>false</c> otherwise.</returns>
        public static bool Exists(string name)
        {
            return Pool.Exists(name);
        }

        public static ChannelOption<T> NewInstance<T>(string name)
        {
            return Pool.NewInstance(name);
        }

        #region Constants

        // ReSharper disable InconsistentNaming
        public static readonly ChannelOption<IByteBufAllocator> ALLOCATOR = ValueOf("ALLOCATOR");
        public static readonly ChannelOption<IRecvByteBufAllocator> RCVBUF_ALLOCATOR = ValueOf("RCVBUF_ALLOCATOR");
        public static readonly ChannelOption<IMessageSizeEstimator> MESSAGE_SIZE_ESTIMATOR = ValueOf("MESSAGE_SIZE_ESTIMATOR");

        public static readonly ChannelOption<int> CONNECT_TIMEOUT_MILLIS = ValueOf("CONNECT_TIMEOUT_MILLIS");
        public static readonly ChannelOption<int> MAX_MESSAGES_PER_READ = ValueOf("MAX_MESSAGES_PER_READ");
        public static readonly ChannelOption<int> WRITE_SPIN_COUNT = ValueOf("WRITE_SPIN_COUNT");
        public static readonly ChannelOption<int> WRITE_BUFFER_HIGH_WATER_MARK = ValueOf("WRITE_BUFFER_HIGH_WATER_MARK");
        public static readonly ChannelOption<int> WRITE_BUFFER_LOW_WATER_MARK = ValueOf("WRITE_BUFFER_LOW_WATER_MARK");

        public static readonly ChannelOption<bool> ALLOW_HALF_CLOSURE = ValueOf("ALLOW_HALF_CLOSURE");
        public static readonly ChannelOption<bool> AUTO_READ = ValueOf("AUTO_READ");

        public static readonly ChannelOption<bool> SO_BROADCAST = ValueOf("SO_BROADCAST");
        public static readonly ChannelOption<bool> SO_KEEPALIVE = ValueOf("SO_KEEPALIVE");
        public static readonly ChannelOption<int> SO_SNDBUF = ValueOf("SO_SNDBUF");
        public static readonly ChannelOption<int> SO_RCVBUF = ValueOf("SO_RCVBUF");
        public static readonly ChannelOption<bool> SO_REUSEADDR = ValueOf("SO_REUSEADDR");
        public static readonly ChannelOption<int> SO_LINGER = ValueOf("SO_LINGER");
        public static readonly ChannelOption<int> SO_BACKLOG = ValueOf("SO_BACKLOG");
        public static readonly ChannelOption<int> SO_TIMEOUT = ValueOf("SO_TIMEOUT");

        public static readonly ChannelOption<int> IP_TOS = ValueOf("IP_TOS");
        public static readonly ChannelOption<IPEndPoint> IP_MULTICAST_ADDR = ValueOf("IP_MULTICAST_ADDR");
        public static readonly ChannelOption<NetworkInterface> IP_MULTICAST_IF = ValueOf("IP_MULTICAST_IF");
        public static readonly ChannelOption<int> IP_MULTICAST_TTL = ValueOf("IP_MULTICAST_TTL");
        public static readonly ChannelOption<bool> IP_MULTICAST_LOOP_DISABLED = ValueOf("IP_MULTICAST_LOOP_DISABLED");

        public static readonly ChannelOption<bool> TCP_NODELAY = ValueOf("TCP_NODELAY");

        // ReSharper restore InconsistentNaming


        #endregion
    }

    /// <summary>
    /// A <see cref="ChannelOption{T}"/> enable us to configure a <see cref="IChannelConfig"/> in a
    /// type-safeway. Which <see cref="ChannelOption{T}"/> is supported depends on the actual implementation
    /// of <see cref="IChannelConfig"/> and may depend on the nature of the transport it belongs to.
    /// </summary>
    public sealed class ChannelOption<T> : AbstractConstant
    {
        public ChannelOption(int id, string name)
            : base(id, name, typeof(T))
        {
        }

        /// <summary>
        /// Validate the value which is set for the <see cref="ChannelOption{T}"/>. 
        /// </summary>
        /// <param name="value">The value that will be set for this option.</param>
        public void Validate(T value)
        {
            if (value == null)
                throw new ArgumentNullException("value");
        }

        #region Conversion

        // Cache of previously casted values, since template downcasting works a little differently in C#
        private static readonly ConcurrentDictionary<ChannelOption<object>, ChannelOption<T>> CastedValues = new ConcurrentDictionary<ChannelOption<object>, ChannelOption<T>>();


        public static implicit operator ChannelOption<T>(ChannelOption<object> obj)
        {
            return CastedValues.GetOrAdd(obj, new ChannelOption<T>(obj.Id, obj.Name));
        }

        #endregion
    }
}
