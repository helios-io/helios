using System.Collections.Generic;
using Helios.Buffers;

namespace Helios.Channel
{
    /// <summary>
    /// A set of configuration properties for <see cref="IChannel"/>.
    /// 
    /// Please down-cast to a more specific configuration type such as
    /// <see cref="SocketChannelConfig"/> or use <see cref="SetOptions"/> to
    /// set the transport-specific properties.
    /// <example>
    ///     IChannel ch = ...;
    ///     SocketChannelConfig cfg = (SocketChannelConfig)ch.Config;
    ///     cfg.SetTcpNoDelay(false);
    /// </example>
    /// 
    /// //TODO: port option map
    /// </summary>
    public interface IChannelConfig
    {
        /// <summary>
        /// The entire set of <see cref="ChannelOption{T}"/>s for this configuration.
        /// </summary>
        IDictionary<ChannelOption<object>, object> Options { get; }

        /// <summary>
        /// Return the value of the given <see cref="ChannelOption{T}"/>
        /// </summary>
        /// <param name="options">The options that we'll be setting our channel to.</param>
        /// <returns><c>true</c> if we were ableto set the new options, <c>false</c> otherwise.</returns>
        bool SetOptions(IDictionary<ChannelOption<object>, object> options);

        /// <summary>
        /// Return the value of the given <see cref="ChannelOption{T}"/>
        /// </summary>
        /// <typeparam name="T">The type of the option.</typeparam>
        /// <param name="option">The option we're going to retrieve.</param>
        /// <returns>The value of the given option.</returns>
        T GetOption<T>(ChannelOption<T> option);

        /// <summary>
        /// Sets a configuration property with the specified name and value.
        /// To override this method properly, you must call the base class:
        /// <code>
        ///     public bool SetOption{T}(ChannelOption{T} option, T value){
        ///         if(base.SetOption(option, value)){
        ///             return true;
        ///         }
        /// 
        ///         if(option.equals(additionalOption)){
        ///             ...
        ///             return true;
        ///         }
        ///     }
        /// </code>
        /// </summary>
        /// <typeparam name="T">The type of the option.</typeparam>
        /// <param name="option">The name of the option.</param>
        /// <param name="value">The value of the option.</param>
        /// <returns><c>true</c> if and only if the property has been set.</returns>
        bool SetOption<T>(ChannelOption<T> option, T value);

        /// <summary>
        /// Returns the connect timeout of the channel in milliseconds. If the
        /// <see cref="IChannel"/> does not support the connect operation, this
        /// property is not used at all and will be ignored.
        /// </summary>
        /// <returns>The connect timeout in milliseconds. <c>0</c> if disabled.</returns>
        int ConnectTimeoutMillis { get; }

        /// <summary>
        /// Sets the connect timeout of the channel in milliseconds. If the
        /// <see cref="IChannel"/> does not support the connect operation, this
        /// property is not used at all and will be ignored.
        /// </summary>
        /// <param name="connectTimeoutMillis">The connect timeout in milliseconds. Set to <c>0</c> to disable.</param>
        /// <returns>An updated config object.</returns>
        IChannelConfig SetConnectTimeoutMillis(int connectTimeoutMillis);

        /// <summary>
        /// Returns the maximum number of messages to read per read loop.
        /// A <see cref="IChannelHandler.ChannelRead(ChannelHandlerContext, object)"/> event.
        /// </summary>
        /// <remarks>
        /// If this vaue is greater than 1, an event loop might attempt to read multiple times to procure multiple messages.
        /// </remarks>
        int MaxMessagesPerRead { get; }

        /// <summary>
        /// Sets the maximum number of messages to read per read loop. 
        /// </summary>
        /// <param name="maxMessagesPerRead">The number of messages to read per read loop.</param>
        /// <returns>An updated config object.</returns>
        /// <remarks>If this vaue is greater than 1, an event loop might attempt to read multiple times to procure multiple messages.</remarks>
        IChannelConfig SetMaxMessagesPerRead(int maxMessagesPerRead);

        /// <summary>
        /// Get the maximum loop count for a write operation until
        /// <see cref="WritableByteChannelBuffer.Write(IByteBuffer)"/> returns a non-zero value.
        /// It is similar to what a spin lock is used for in concurrent programming.
        /// It improves memory utilization and write throughput depending on what platform
        /// Helios runs on. The default value is <code>16</code>.
        /// </summary>
        int WriteSpinCount { get; }

        /// <summary>
        /// Set the maximum loop count for a write operation until
        /// <see cref="WritableByteChannelBuffer.Write(IByteBuffer)"/> returns a non-zero value.
        /// It is similar to what a spin lock is used for in concurrent programming.
        /// It improves memory utilization and write throughput depending on what platform
        /// Helios runs on. The default value is <code>16</code>.
        /// </summary>
        /// <param name="spinCount">The spin count value.</param>
        /// <returns>An updated config object.</returns>
        IChannelConfig SetWriteSpinCount(int spinCount);

        /// <summary>
        /// Returns the <see cref="IByteBufAllocator"/> which is used by the channel
        /// to allocate buffers.
        /// </summary>
        IByteBufAllocator Allocator { get; }

        /// <summary>
        /// Set the <see cref="IByteBufAllocator"/> which is used by the channel
        /// to allocate buffers.
        /// </summary>
        /// <param name="allocator">The allocator instance.</param>
        /// <returns>An updated config object.</returns>
        IChannelConfig SetAllocator(IByteBufAllocator allocator);

        //TODO: RecvByteBufAllocator

        /// <summary>
        /// Returns <c>true</c> if and only if <see cref="ChannelHandlerContext.Read"/> will be invoked automatically
        /// so that a user application doesn't need to call it at all. The default value is <c>true</c>.
        /// </summary>
        bool AutoRead { get; }

        /// <summary>
        /// Sets if <see cref="ChannelHandlerContext.Read"/> will be invoked automatically
        /// so that a user application doesn't need to call it at all. The default value is <c>true</c>.
        /// </summary>
        /// <param name="autoRead">The new auto-read value.</param>
        /// <returns>An updated config object.</returns>
        IChannelConfig SetAutoRead(bool autoRead);

        int WriteBufferHighWaterMark { get; }

        IChannelConfig SetWriteBufferHighWaterMark(int writeBufferHighWaterMark);

        int WriteBufferLowWaterMark { get; }

        IChannelConfig SetWriteBufferLowWaterMark(int writeBufferLowWaterMark);

        //TODO: MessageSizeEstimator
    }
}