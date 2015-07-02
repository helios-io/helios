using System;
using System.Net;
using System.Threading.Tasks;
using Helios.Buffers;

namespace Helios.Channel
{
    /// <summary>
    /// An abstraction over a network socket or a component which is capable of I/O
    /// operations such as read, write, connect, and bind.
    /// 
    /// A channel provides a user with:
    /// * The current state of the channel (e.g. is it open? connected?),
    /// * The <see cref="IChannelConfig"/> configuration parameters of the channel (e.g. receive buffer size),
    /// * The I/O operations that the channel supports (e.g. read, write, connect, and bind), and
    /// * The <see cref="IChannelPipeline"/> which handles all I/O events and requests associated with the channel.
    /// 
    /// All I/O operations are asynchronous.
    /// TODO: fill in
    /// 
    /// Channels are hierarchical.
    /// TODO: fill in
    /// 
    /// Downcast to access transport-specific operations.
    /// TODO: fill in
    /// 
    /// Release resources
    /// It is important that you call <see cref="Close"/> or <see cref="Close(TaskCompletionSource{IChannel})"/> to release all 
    /// resources once you are done with the <see cref="IChannel"/>. This ensures that all resources are 
    /// released in a proper way, such as file handles.
    /// </summary>
    public interface IChannel : IComparable<IChannel>
    {
        /// <summary>
        /// Returns the parent of this channel.
        /// </summary>
        /// <remarks>Returns <c>null</c> if this channel does not have a parent channel.</remarks>
        IChannel Parent { get; }

        /// <summary>
        /// Returns the configuration of this channel.
        /// </summary>
        IChannelConfig Config { get; }

        /// <summary>
        /// Returns <c>true</c> if the <see cref="IChannel"/> is open and may become active later.
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// Returns <c>true</c> if the <see cref="IChannel"/> is open and registered with an <see cref="IEventLoop"/>
        /// </summary>
        bool IsRegistered { get; }

        /// <summary>
        /// Returns <c>true</c> if the <see cref="IChannel"/> is active and thus, connected.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Returns the local address where this channel is bound. The returned
        /// <see cref="EndPoint"/> can be down-cast into more concrete types such
        /// as <see cref="IPEndPoint"/> or <see cref="DnsEndPoint"/> to retrieve
        /// detailed information.
        /// </summary>
        /// <returns>
        ///     The local address of this channel, <c>null</c> if this channel is not bound.
        /// </returns>
        EndPoint LocalAddress { get; }

        /// <summary>
        /// Returns the remote address that to which this channel is connected. The returned
        /// <see cref="EndPoint"/> can be down-cast into more concrete types such
        /// as <see cref="IPEndPoint"/> or <see cref="DnsEndPoint"/> to retrieve
        /// detailed information.
        /// </summary>
        /// <returns>
        ///     The remote address of this channel.
        ///     <c>null</c> if this channel is not connected.
        /// </returns>
        /// <remarks>
        ///     If this channel is not connected but it can receive messsages
        ///     from arbitrary remote addresses (e.g. <see cref="DatagramChannel"/>),
        ///     use <see cref="DatagramPacket.Recipient"/> to determine theorigination
        ///     of the received message as this method will return <c>null</c>.
        /// </remarks>
        EndPoint RemoteAddress { get; }

        /// <summary>
        /// Returns the <see cref="Task{IChannel}"/> which will be notified when this
        /// channel is closed.
        /// <remarks>
        ///     This method always returns the same instance.
        /// </remarks>
        /// </summary>
        Task<IChannel> CloseTask { get; }

        /// <summary>
        /// Returns <c>true</c> if and only if the I/O thread will perform the 
        /// requested write operation immediately. Any write requests made when
        /// this method returns <c>false</c> are queued until the I/O thread is
        /// ready to process the queued write requests.
        /// </summary>
        bool IsWritable { get; }

        /// <summary>
        /// Returns the <see cref="IChannelPipeline"/> for this channel.
        /// </summary>
        IChannelPipeline Pipeline { get; }

        /// <summary>
        /// Returns the assigned <see cref="IByteBufAllocator"/> which will be used to allocate
        /// all <see cref="IByteBuf"/>s.
        /// </summary>
        IByteBufAllocator Allocator { get; }


        /// <summary>
        /// Returns a new <see cref="TaskCompletionSource{IChannel}"/>.
        /// </summary>
        /// <returns>A new <see cref="TaskCompletionSource{IChannel}"/> for this <see cref="IChannel"/>.</returns>
        TaskCompletionSource<IChannel> NewCompletionSource();

        /// <summary>
        /// Create a new <see cref="Task{IChannel}"/> which is marked as succeeded already. 
        /// So <see cref="Task{IChannel}.IsCompleted"/> returns <c>true</c>.
        /// </summary>
        /// <returns>A new instance of a completed <see cref="Task{IChannel}"/>.</returns>
        Task<IChannel> NewSucceededTask();

        /// <summary>
        /// Create a new <see cref="Task{IChannel}"/> which is marked as failed already. 
        /// So <see cref="Task{IChannel}.IsFaulted"/> returns <c>true</c>.
        /// </summary>
        /// <param name="cause">The reason why the returned task has failed.</param>
        /// <returns>A new <see cref="Task{IChannel}"/> that is marked as failed due to <see cref="cause"/>.</returns>
        Task<IChannel> NewFailedTask(Exception cause);

        /// <summary>
        /// Return a special <see cref="TaskCompletionSource{IChannel}"/> which can be reused for different operations.
        /// </summary>
        /// <remarks>
        ///     It's only supported use is for <see cref="IChannel.Write(object, TaskCompletionSource{IChannel})"/>.
        /// 
        ///     Be aware that the returned <see cref="TaskCompletionSource{IChannel}"/> will not support msot operations
        ///     and should only be used if you want to save an object allocation for every write operation. You will not
        ///     be able to detect if the operation wascopleted, only if it failed as the implementation will call
        ///     <see cref="IChannelPipeline.FireExceptionCaught(Exception)"/> in this case.
        /// 
        ///     THIS IS AN EXPERT FEATURE and should be used with caution!
        /// </remarks>
        TaskCompletionSource<IChannel> VoidPromise { get; }

        /// <summary>
        /// Request to bind the given <see cref="EndPoint"/> and notify the <see cref="Task{IChannel}"/> once the operation
        /// completes, either because the operation was successful or because of an error. 
        /// 
        /// This will result in having the <see cref="IChannelHandler.Bind(ChannlerHandlerContext, EndPoint, TaskCompletionSource{IChannel})"/> method
        /// of the next <see cref="IChannelHandler"/> contained in the <see cref="IChannelPipeline"/> of the <see cref="IChannel"/>.
        /// </summary>
        /// <param name="localAddress">The local address we're binding to.</param>
        /// <returns>A Task that will complete upon the success or failure of the bind operation.</returns>
        Task<IChannel> Bind(EndPoint localAddress);

        /// <summary>
        /// Request to connect to he given <see cref="EndPoint"/> and notify the <see cref="Task{IChannel}"/> once the operation
        /// completes, either because the operation was successful or because of an error.
        /// 
        /// This will result in having the <see cref="IChannelHandler.Connect(ChannelHandlerContext, EndPoint, TaskCompletionSource{IChannel})"/>
        /// method of the next <see cref="IChannelHandler"/> caontained in the <see cref="IChannelPipeline"/> of the <see cref="IChannel"/>.
        /// </summary>
        /// <param name="remoteAddress">The remote address we're connecting to.</param>
        /// <returns>A task that will complete once the connect operation succeeds or fails.</returns>
        Task<IChannel> Connect(EndPoint remoteAddress);

        /// <summary>
        /// Request to connect to he given <see cref="EndPoint"/> while bind to the <see cref="localAddress"/> and notify the
        /// <see cref="Task{IChannel}"/> once the operation completes, either because the operation was successful or because
        /// of an error.
        /// 
        /// This will result in having the <see cref="IChannelHandler.Connect(ChannelHandlerContext, EndPoint, EndPoint, TaskCompletionSource{IChannel})"/>
        /// method of the next <see cref="IChannelHandler"/> caontained in the <see cref="IChannelPipeline"/> of the <see cref="IChannel"/>.
        /// </summary>
        /// <param name="remoteAddress">The remote address we're connecting to.</param>
        /// <param name="localAddress">The local address to which we'll be binding.</param>
        /// <returns>A task that will complete once the connect operation succeeds or fails.</returns>
        Task<IChannel> Connect(EndPoint remoteAddress, EndPoint localAddress);

        /// <summary>
        /// Request to disconnect from the remote peer and notify the <see cref="Task{IChannel}"/> once the operation completes,
        /// either because the operation was successful or because of an error.
        /// 
        /// This will result in having the <see cref="IChannelHandler.Disconnect(ChannelHandlerContext context, TaskCompletionSource{IChannel})"/>
        /// method called fo the next <see cref="IChannelHandler"/> contained in the <see cref="IChannelPipeline"/> 
        /// of the <see cref="IChannel"/>.
        /// </summary>
        /// <returns>A task that will complete once the disconnect operation succeeds or fails.</returns>
        Task<IChannel> Disconnect();

        /// <summary>
        /// Request to close this <see cref="IChannel"/> and notify the <see cref="Task{IChannel}"/> once the operation completes,
        /// either because the operation was successful or because of an error.
        /// 
        /// After a channel is closed, it's not possible to reuse it again.
        /// 
        /// This will result in having the <see cref="IChannelHandler.Close(ChannelHandlerContext, TaskCompletionSource{IChannel})"/>
        /// method called of the next <see cref="IChannelHandler"/> contained in the <see cref="IChannelPipeline"/> of the
        /// <see cref="IChannel"/>.
        /// </summary>
        /// <returns>A task that will complete once the close operation succeeds or fails.</returns>
        Task<IChannel> Close();

        /// <summary>
        /// TODO: fill in
        /// </summary>
        /// <returns></returns>
        Task<IChannel> Deregister();


        /// <summary>
        /// TODO: fill in
        /// </summary>
        /// <param name="localAddress"></param>
        /// <param name="promise"></param>
        /// <returns></returns>
        Task<IChannel> Bind(EndPoint localAddress, TaskCompletionSource<IChannel> promise);

        Task<IChannel> Connect(EndPoint remoteAddress, TaskCompletionSource<IChannel> promise);

        Task<IChannel> Connect(EndPoint remoteAddress, EndPoint localAddress, TaskCompletionSource<IChannel> promise);

        Task<IChannel> Disconnect(TaskCompletionSource<IChannel> promise);

        Task<IChannel> Close(TaskCompletionSource<IChannel> promise);

        Task<IChannel> Deregister(TaskCompletionSource<IChannel> promise);

        IChannel Read();

        Task<IChannel> Write(object msg);

        Task<IChannel> Write(object msg, TaskCompletionSource<IChannel> promise);

        IChannel Flush();

        Task<IChannel> WriteAndFlush(object msg, TaskCompletionSource<IChannel> promise);

        Task<IChannel> WriteAndFlush(object msg);
    }
}
