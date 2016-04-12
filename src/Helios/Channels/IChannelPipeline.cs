using System.Collections.Generic;

namespace Helios.Channels
{

    /// <summary>
    /// Represents a duplex pipeline of handlers. 
    /// </summary>
    public interface IChannelPipeline : IEnumerable<IChannelHandler>
    {
        /// <summary>
        /// Inserts a {@link ChannelHandler} at the first position of this pipeline.
        /// </summary>
        ///
        /// @param name     the name of the handler to insert first. {@code null} to let the name auto-generated.
        /// @param handler  the handler to insert first
        ///
        /// @throws IllegalArgumentException
        ///         if there's an entry with the same name already in the pipeline
        /// @throws NullPointerException
        ///         if the specified handler is {@code null}
        IChannelPipeline AddFirst(string name, IChannelHandler handler);

        /// <summary>
        /// Inserts a {@link ChannelHandler} at the first position of this pipeline.
        /// </summary>
        /// <param name="invoker">the {@link ChannelHandlerInvoker} which invokes the {@code handler}s event handler methods</param>
        /// <param name="name">the name of the handler to insert first. <code>null</code> to let the name auto-generated.</param>
        /// <param name="handler">the handler to insert first</param>
        /// <exception cref="ArgumentException">if there's an entry with the same name already in the pipeline</exception>
        /// <exception cref="ArgumentNullException">if the specified handler is <code>null</code></exception>
        IChannelPipeline AddFirst(IChannelHandlerInvoker invoker, string name, IChannelHandler handler);

        /// <summary>
        /// Appends a {@link ChannelHandler} at the last position of this pipeline.
        ///
        /// @param name     the name of the handler to append. {@code null} to let the name auto-generated.
        /// @param handler  the handler to append
        ///
        /// @throws IllegalArgumentException
        ///         if there's an entry with the same name already in the pipeline
        /// @throws NullPointerException
        ///         if the specified handler is {@code null}
        /// </summary>
        IChannelPipeline AddLast(string name, IChannelHandler handler);

        /// <summary>
        ///     Appends a {@link ChannelHandler} at the last position of this pipeline.
        /// </summary>
        /// <param name="invoker">the {@link ChannelHandlerInvoker} which invokes the {@code handler}s event handler methods</param>
        /// <param name="name">the name of the handler to append. {@code null} to let the name auto-generated.</param>
        /// <param name="handler">the handler to append</param>
        /// <exception cref="ArgumentException">if there's an entry with the same name already in the pipeline</exception>
        /// <exception cref="ArgumentNullException">if the specified handler is <code>null</code></exception>
        IChannelPipeline AddLast(IChannelHandlerInvoker invoker, string name, IChannelHandler handler);

        /// <summary>
        ///     Inserts a {@link ChannelHandler} before an existing handler of this pipeline.
        /// </summary>
        /// <param name="baseName">the name of the existing handler</param>
        /// <param name="name">the name of the handler to insert before. {@code null} to let the name auto-generated.</param>
        /// <param name="handler">the handler to insert before</param>
        /// @throws NoSuchElementException
        /// if there's no such entry with the specified {@code baseName}
        /// @throws IllegalArgumentException
        /// if there's an entry with the same name already in the pipeline
        /// @throws NullPointerException
        /// if the specified baseName or handler is {@code null}
        IChannelPipeline AddBefore(string baseName, string name, IChannelHandler handler);

        /// <summary>
        ///     Inserts a {@link ChannelHandler} before an existing handler of this pipeline.
        /// </summary>
        /// <param name="invoker">the {@link ChannelHandlerInvoker} which invokes the {@code handler}s event handler methods</param>
        /// <param name="baseName">the name of the existing handler</param>
        /// <param name="name">the name of the handler to insert before. {@code null} to let the name auto-generated.</param>
        /// <param name="handler">the handler to insert before</param>
        /// @throws NoSuchElementException
        /// if there's no such entry with the specified {@code baseName}
        /// @throws IllegalArgumentException
        /// if there's an entry with the same name already in the pipeline
        /// @throws NullPointerException
        /// if the specified baseName or handler is {@code null}
        IChannelPipeline AddBefore(IChannelHandlerInvoker invoker, string baseName, string name, IChannelHandler handler);

        /// <summary>
        ///     Inserts a {@link ChannelHandler} after an existing handler of this pipeline.
        /// </summary>
        /// <param name="baseName">the name of the existing handler</param>
        /// <param name="name">the name of the handler to insert after. {@code null} to let the name auto-generated.</param>
        /// <param name="handler">the handler to insert after</param>
        /// @throws NoSuchElementException
        /// if there's no such entry with the specified {@code baseName}
        /// @throws IllegalArgumentException
        /// if there's an entry with the same name already in the pipeline
        /// @throws NullPointerException
        /// if the specified baseName or handler is {@code null}
        IChannelPipeline AddAfter(string baseName, string name, IChannelHandler handler);

        /// <summary>
        ///     Inserts a {@link ChannelHandler} after an existing handler of this pipeline.
        /// </summary>
        /// <param name="invoker">the {@link ChannelHandlerInvoker} which invokes the {@code handler}s event handler methods</param>
        /// <param name="baseName">the name of the existing handler</param>
        /// <param name="name">the name of the handler to insert after. {@code null} to let the name auto-generated.</param>
        /// <param name="handler">the handler to insert after</param>
        /// @throws NoSuchElementException
        /// if there's no such entry with the specified {@code baseName}
        /// @throws IllegalArgumentException
        /// if there's an entry with the same name already in the pipeline
        /// @throws NullPointerException
        /// if the specified baseName or handler is {@code null}
        IChannelPipeline AddAfter(IChannelHandlerInvoker invoker, string baseName, string name, IChannelHandler handler);

        /// <summary>
        /// Inserts a {@link ChannelHandler}s at the first position of this pipeline.
        ///
        /// @param handlers  the handlers to insert first
        ///
        /// </summary>
        IChannelPipeline AddFirst(params IChannelHandler[] handlers);

        /// <summary>
        /// Inserts a {@link ChannelHandler}s at the first position of this pipeline.
        ///
        /// @param invoker   the {@link ChannelHandlerInvoker} which invokes the {@code handler}s event handler methods
        /// @param handlers  the handlers to insert first
        ///
        /// </summary>
        IChannelPipeline AddFirst(IChannelHandlerInvoker invoker, params IChannelHandler[] handlers);

        /// <summary>
        /// Inserts a {@link ChannelHandler}s at the last position of this pipeline.
        ///
        /// @param handlers  the handlers to insert last
        ///
        /// </summary>
        IChannelPipeline AddLast(params IChannelHandler[] handlers);

        /// <summary>
        /// Inserts a {@link ChannelHandler}s at the last position of this pipeline.
        ///
        /// @param invoker   the {@link ChannelHandlerInvoker} which invokes the {@code handler}s event handler methods
        /// @param handlers  the handlers to insert last
        ///
        /// </summary>
        IChannelPipeline AddLast(IChannelHandlerInvoker invoker, params IChannelHandler[] handlers);

        /// <summary>
        /// Removes the specified {@link ChannelHandler} from this pipeline.
        ///
        /// @param  handler          the {@link ChannelHandler} to remove
        ///
        /// @throws NoSuchElementException
        ///         if there's no such handler in this pipeline
        /// @throws NullPointerException
        ///         if the specified handler is {@code null}
        /// </summary>
        IChannelPipeline Remove(IChannelHandler handler);

        /// <summary>
        /// Removes the {@link ChannelHandler} with the specified name from this pipeline.
        /// </summary>
        /// <param name="name">the name under which the {@link ChannelHandler} was stored.</param>
        /// <returns>the removed handler</returns>
        /// 
        /// <exception cref="ArgumentException">if there's no such handler with the specified name in this pipeline</exception>
        /// <exception cref="ArgumentNullException">if the specified name is {@code null}</exception>
        IChannelHandler Remove(string name);

        /// <summary>
        /// Removes the {@link ChannelHandler} of the specified type from this pipeline.
        ///
        /// @param <T>           the type of the handler
        /// @param handlerType   the type of the handler
        ///
        /// @return the removed handler
        ///
        /// @throws NoSuchElementException
        ///         if there's no such handler of the specified type in this pipeline
        /// @throws NullPointerException
        ///         if the specified handler type is {@code null}
        /// </summary>
        T Remove<T>() where T : class, IChannelHandler;

        /// <summary>
        /// Removes the first {@link ChannelHandler} in this pipeline.
        ///
        /// @return the removed handler
        ///
        /// @throws NoSuchElementException
        ///         if this pipeline is empty
        /// </summary>
        IChannelHandler RemoveFirst();

        /// <summary>
        /// Removes the last {@link ChannelHandler} in this pipeline.
        ///
        /// @return the removed handler
        ///
        /// @throws NoSuchElementException
        ///         if this pipeline is empty
        /// </summary>
        IChannelHandler RemoveLast();

        /// <summary>
        /// Replaces the specified {@link ChannelHandler} with a new handler in this pipeline.
        ///
        /// @param  oldHandler    the {@link ChannelHandler} to be replaced
        /// @param  newName       the name under which the replacement should be added.
        ///                       {@code null} to use the same name with the handler being replaced.
        /// @param  newHandler    the {@link ChannelHandler} which is used as replacement
        ///
        /// @return itself
        /// @throws NoSuchElementException
        ///         if the specified old handler does not exist in this pipeline
        /// @throws IllegalArgumentException
        ///         if a handler with the specified new name already exists in this
        ///         pipeline, except for the handler to be replaced
        /// @throws NullPointerException
        ///         if the specified old handler or new handler is {@code null}
        /// </summary>
        IChannelPipeline Replace(IChannelHandler oldHandler, string newName, IChannelHandler newHandler);

        /// Replaces the {@link ChannelHandler} of the specified name with a new handler in this pipeline.
        /// @param  oldName       the name of the {@link ChannelHandler} to be replaced
        /// @param  newName       the name under which the replacement should be added.
        ///                       {@code null} to use the same name with the handler being replaced.
        /// @param  newHandler    the {@link ChannelHandler} which is used as replacement
        /// @return the removed handler
        /// @throws NoSuchElementException
        ///         if the handler with the specified old name does not exist in this pipeline
        /// @throws IllegalArgumentException
        ///         if a handler with the specified new name already exists in this
        ///         pipeline, except for the handler to be replaced
        /// @throws NullPointerException
        ///         if the specified old handler or new handler is {@code null}
        IChannelHandler Replace(string oldName, string newName, IChannelHandler newHandler);

        /// <summary>
        /// Replaces the {@link ChannelHandler} of the specified type with a new handler in this pipeline.
        ///
        /// @param  oldHandlerType   the type of the handler to be removed
        /// @param  newName          the name under which the replacement should be added.
        ///                          {@code null} to use the same name with the handler being replaced.
        /// @param  newHandler       the {@link ChannelHandler} which is used as replacement
        ///
        /// @return the removed handler
        ///
        /// @throws NoSuchElementException
        ///         if the handler of the specified old handler type does not exist
        ///         in this pipeline
        /// @throws IllegalArgumentException
        ///         if a handler with the specified new name already exists in this
        ///         pipeline, except for the handler to be replaced
        /// @throws NullPointerException
        ///         if the specified old handler or new handler is {@code null}
        /// </summary>
        T Replace<T>(string newName, IChannelHandler newHandler) where T : class, IChannelHandler;

        /// <summary>
        /// Returns the first {@link ChannelHandler} in this pipeline.
        ///
        /// @return the first handler.  {@code null} if this pipeline is empty.
        /// </summary>
        IChannelHandler First();

        /// <summary>
        /// Returns the context of the first {@link ChannelHandler} in this pipeline.
        ///
        /// @return the context of the first handler.  {@code null} if this pipeline is empty.
        /// </summary>
        IChannelHandlerContext FirstContext();

        /// <summary>
        /// Returns the last {@link ChannelHandler} in this pipeline.
        ///
        /// @return the last handler.  {@code null} if this pipeline is empty.
        /// </summary>
        IChannelHandler Last();

        /// <summary>
        /// Returns the context of the last {@link ChannelHandler} in this pipeline.
        ///
        /// @return the context of the last handler.  {@code null} if this pipeline is empty.
        /// </summary>
        IChannelHandlerContext LastContext();

        /// <summary>Returns the {@link ChannelHandler} with the specified name in this pipeline.</summary>
        /// <returns>the handler with the specified name. {@code null} if there's no such handler in this pipeline.</returns>
        IChannelHandler Get(string name);

        /// <summary>
        /// Returns the {@link ChannelHandler} of the specified type in this
        /// pipeline.
        ///
        /// @return the handler of the specified handler type.
        ///         {@code null} if there's no such handler in this pipeline.
        /// </summary>
        T Get<T>() where T : class, IChannelHandler;

        /// <summary>
        /// Returns the context object of the specified {@link ChannelHandler} in
        /// this pipeline.
        ///
        /// @return the context object of the specified handler.
        ///         {@code null} if there's no such handler in this pipeline.
        /// </summary>
        IChannelHandlerContext Context(IChannelHandler handler);

        /// <summary>Returns the context object of the {@link ChannelHandler} with the specified name in this pipeline.</summary>
        /// <returns>the context object of the handler with the specified name. {@code null} if there's no such handler in this pipeline.</returns>
        IChannelHandlerContext Context(string name);

        /// <summary>
        /// Returns the context object of the {@link ChannelHandler} of the
        /// specified type in this pipeline.
        ///
        /// @return the context object of the handler of the specified type.
        ///         {@code null} if there's no such handler in this pipeline.
        /// </summary>
        IChannelHandlerContext Context<T>() where T : class, IChannelHandler;

        /// <summary>
        /// Returns the {@link Channel} that this pipeline is attached to.
        ///
        /// @return the channel. {@code null} if this pipeline is not attached yet.
        /// </summary>
        IChannel Channel();

        /// <summary>
        /// A {@link Channel} is active now, which means it is connected.
        ///
        /// This will result in having the  {@link ChannelHandler#channelActive(ChannelHandlerContext)} method
        /// called of the next  {@link ChannelHandler} contained in the  {@link ChannelPipeline} of the
        /// {@link Channel}.
        /// </summary>
        /// <summary>
        /// A {@link Channel} was registered to its {@link EventLoop}.
        ///
        /// This will result in having the  {@link ChannelHandler#channelRegistered(ChannelHandlerContext)} method
        /// called of the next  {@link ChannelHandler} contained in the  {@link ChannelPipeline} of the
        /// {@link Channel}.
        /// </summary>
        IChannelPipeline FireChannelRegistered();

        /// <summary>
        /// A {@link Channel} was unregistered from its {@link EventLoop}.
        ///
        /// This will result in having the  {@link ChannelHandler#channelUnregistered(ChannelHandlerContext)} method
        /// called of the next  {@link ChannelHandler} contained in the  {@link ChannelPipeline} of the
        /// {@link Channel}.
        /// </summary>
        IChannelPipeline FireChannelUnregistered();

        /// <summary>
        /// A {@link Channel} is active now, which means it is connected.
        ///
        /// This will result in having the  {@link ChannelHandler#channelActive(ChannelHandlerContext)} method
        /// called of the next  {@link ChannelHandler} contained in the  {@link ChannelPipeline} of the
        /// {@link Channel}.
        /// </summary>
        IChannelPipeline FireChannelActive();

        /// <summary>
        /// A {@link Channel} is inactive now, which means it is closed.
        ///
        /// This will result in having the  {@link ChannelHandler#channelInactive(ChannelHandlerContext)} method
        /// called of the next  {@link ChannelHandler} contained in the  {@link ChannelPipeline} of the
        /// {@link Channel}.
        /// </summary>
        IChannelPipeline FireChannelInactive();

        /// <summary>
        /// A {@link Channel} received an {@link Throwable} in one of its inbound operations.
        ///
        /// This will result in having the  {@link ChannelHandler#exceptionCaught(ChannelHandlerContext, Throwable)}
        /// method  called of the next  {@link ChannelHandler} contained in the  {@link ChannelPipeline} of the
        /// {@link Channel}.
        /// </summary>
        IChannelPipeline FireExceptionCaught(Exception cause);

        /// <summary>
        /// A {@link Channel} received an user defined event.
        ///
        /// This will result in having the  {@link ChannelHandler#userEventTriggered(ChannelHandlerContext, Object)}
        /// method  called of the next  {@link ChannelHandler} contained in the  {@link ChannelPipeline} of the
        /// {@link Channel}.
        /// </summary>
        IChannelPipeline FireUserEventTriggered(object evt);

        /// <summary>
        /// A {@link Channel} received a message.
        ///
        /// This will result in having the {@link ChannelHandler#channelRead(ChannelHandlerContext, Object)}
        /// method  called of the next {@link ChannelHandler} contained in the  {@link ChannelPipeline} of the
        /// {@link Channel}.
        /// </summary>
        IChannelPipeline FireChannelRead(object msg);

        IChannelPipeline FireChannelReadComplete();

        /// <summary>
        /// Triggers an {@link ChannelHandler#channelWritabilityChanged(ChannelHandlerContext)}
        /// event to the next {@link ChannelHandler} in the {@link ChannelPipeline}.
        /// </summary>
        IChannelPipeline FireChannelWritabilityChanged();

        /// <summary>
        /// Request to bind to the given {@link SocketAddress} and notify the {@link ChannelFuture} once the operation
        /// completes, either because the operation was successful or because of an error.
        ///
        /// The given {@link ChannelPromise} will be notified.
        /// <p>
        /// This will result in having the
        /// {@link ChannelHandler#bind(ChannelHandlerContext, SocketAddress, ChannelPromise)} method
        /// called of the next {@link ChannelHandler} contained in the  {@link ChannelPipeline} of the
        /// {@link Channel}.
        /// </summary>
        Task BindAsync(EndPoint localAddress);

        /// <summary>
        /// Request to connect to the given {@link EndPoint} and notify the {@link Task} once the operation
        /// completes, either because the operation was successful or because of an error.
        ///
        /// The given {@link Task} will be notified.
        ///
        /// <p>
        /// If the connection fails because of a connection timeout, the {@link Task} will get failed with
        /// a {@link ConnectTimeoutException}. If it fails because of connection refused a {@link ConnectException}
        /// will be used.
        /// <p>
        /// This will result in having the
        /// {@link ChannelHandler#connect(ChannelHandlerContext, EndPoint, EndPoint, ChannelPromise)}
        /// method called of the next {@link ChannelHandler} contained in the  {@link ChannelPipeline} of the
        /// {@link Channel}.
        /// </summary>
        Task ConnectAsync(EndPoint remoteAddress);

        /// <summary>
        /// Request to connect to the given {@link EndPoint} while bind to the localAddress and notify the
        /// {@link Task} once the operation completes, either because the operation was successful or because of
        /// an error.
        ///
        /// The given {@link ChannelPromise} will be notified and also returned.
        /// <p>
        /// This will result in having the
        /// {@link ChannelHandler#connect(ChannelHandlerContext, EndPoint, EndPoint, ChannelPromise)}
        /// method called of the next {@link ChannelHandler} contained in the  {@link ChannelPipeline} of the
        /// {@link Channel}.
        /// </summary>
        Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress);

        /// <summary>
        /// Request to disconnect from the remote peer and notify the {@link Task} once the operation completes,
        /// either because the operation was successful or because of an error.
        ///
        /// The given {@link ChannelPromise} will be notified.
        /// <p>
        /// This will result in having the
        /// {@link ChannelHandler#disconnect(ChannelHandlerContext, ChannelPromise)}
        /// method called of the next {@link ChannelHandler} contained in the  {@link ChannelPipeline} of the
        /// {@link Channel}.
        /// </summary>
        Task DisconnectAsync();

        /// <summary>
        /// Request to close the {@link Channel} and notify the {@link ChannelFuture} once the operation completes,
        /// either because the operation was successful or because of
        /// an error.
        ///
        /// After it is closed it is not possible to reuse it again.
        /// <p>
        /// This will result in having the
        /// {@link ChannelHandler#close(ChannelHandlerContext, ChannelPromise)}
        /// method called of the next {@link ChannelHandler} contained in the  {@link ChannelPipeline} of the
        /// {@link Channel}.
        /// </summary>
        Task CloseAsync();

        /// <summary>
        /// Request to deregister the {@link Channel} bound this {@link ChannelPipeline} from the previous assigned
        /// {@link EventExecutor} and notify the {@link ChannelFuture} once the operation completes, either because the
        /// operation was successful or because of an error.
        ///
        /// The given {@link ChannelPromise} will be notified.
        /// <p>ChannelOutboundHandler
        /// This will result in having the
        /// {@link ChannelHandler#deregister(ChannelHandlerContext, ChannelPromise)}
        /// method called of the next {@link ChannelHandler} contained in the  {@link ChannelPipeline} of the
        /// {@link Channel}.
        /// </summary>
        Task DeregisterAsync();

        /// <summary>
        /// Request to Read data from the {@link Channel} into the first inbound buffer, triggers an
        /// {@link ChannelHandler#channelRead(ChannelHandlerContext, Object)} event if data was
        /// read, and triggers a
        /// {@link ChannelHandler#channelReadComplete(ChannelHandlerContext) channelReadComplete} event so the
        /// handler can decide to continue reading.  If there's a pending read operation already, this method does nothing.
        /// <p>
        /// This will result in having the
        /// {@link ChannelHandler#read(ChannelHandlerContext)}
        /// method called of the next {@link ChannelHandler} contained in the  {@link ChannelPipeline} of the
        /// {@link Channel}.
        /// </summary>
        IChannelPipeline Read();

        /// <summary>
        /// Request to write a message via this {@link ChannelPipeline}.
        /// This method will not request to actual flush, so be sure to call {@link #flush()}
        /// once you want to request to flush all pending data to the actual transport.
        /// </summary>
        Task WriteAsync(object msg);

        /// <summary>
        /// Request to flush all pending messages.
        /// </summary>
        IChannelPipeline Flush();

        /// <summary>
        /// Shortcut for call {@link #write(Object)} and {@link #flush()}.
        /// </summary>
        Task WriteAndFlushAsync(object msg);
    }
}