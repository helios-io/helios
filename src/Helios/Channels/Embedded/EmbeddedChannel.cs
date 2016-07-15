// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Helios.Concurrency;
using Helios.Logging;
using Helios.Util;
using Helios.Util.Concurrency;
using Debug = System.Diagnostics.Debug;

namespace Helios.Channels.Embedded
{
    public class EmbeddedChannel : AbstractChannel
    {
        private static readonly EndPoint LOCAL_ADDRESS = new EmbeddedSocketAddress();
        private static readonly EndPoint REMOTE_ADDRESS = new EmbeddedSocketAddress();

        private static readonly IChannelHandler[] EMPTY_HANDLERS = new IChannelHandler[0];

        //TODO: ChannelMetadata
        private static readonly ILogger logger = LoggingFactory.GetLogger<EmbeddedChannel>();

        private readonly EmbeddedEventLoop loop = new EmbeddedEventLoop();

        private Queue<object> inboundMessages;
        private Exception lastException;
        private Queue<object> outboundMessages;
        private State state;

        public EmbeddedChannel()
            : this(EMPTY_HANDLERS)
        {
        }

        /// <summary>
        ///     Create a new instance with the pipeline initialized with the specified handlers.
        /// </summary>
        /// <param name="id">The <see cref="IChannelId" /> of this channel.</param>
        /// <param name="handlers">
        ///     The <see cref="IChannelHandler" />s that will be added to the <see cref="IChannelPipeline" />
        /// </param>
        public EmbeddedChannel(params IChannelHandler[] handlers)
            : base(EmbeddedChannelId.Instance, null)
        {
            Configuration = new DefaultChannelConfiguration(this);
            if (handlers == null)
            {
                throw new NullReferenceException("handlers cannot be null");
            }

            var p = Pipeline;
            p.AddLast(new ActionChannelInitializer<IChannel>(channel =>
            {
                var pipeline = channel.Pipeline;
                foreach (var h in handlers)
                {
                    if (h == null)
                    {
                        break;
                    }
                    pipeline.AddLast(h);
                }
            }));

            var future = loop.RegisterAsync(this);
            Debug.Assert(future.IsCompleted);
            p.AddLast(new LastInboundHandler(InboundMessages, RecordException));
        }

        public override IChannelConfiguration Configuration { get; }

        /// <summary>
        ///     Returns the <see cref="Queue{T}" /> which holds all of the <see cref="object" />s that
        ///     were received by this <see cref="IChannel" />.
        /// </summary>
        public Queue<object> InboundMessages
        {
            get { return inboundMessages ?? (inboundMessages = new Queue<object>()); }
        }

        /// <summary>
        ///     Returns the <see cref="Queue{T}" /> which holds all of the <see cref="object" />s that
        ///     were written by this <see cref="IChannel" />.
        /// </summary>
        public Queue<object> OutboundMessages
        {
            get { return outboundMessages ?? (outboundMessages = new Queue<object>()); }
        }

        public override bool DisconnectSupported
        {
            get { return false; }
        }

        protected override EndPoint LocalAddressInternal
        {
            get { return IsActive ? LOCAL_ADDRESS : null; }
        }

        protected override EndPoint RemoteAddressInternal
        {
            get { return IsActive ? REMOTE_ADDRESS : null; }
        }

        public override bool IsOpen
        {
            get { return state != State.Closed; }
        }

        public override bool IsActive
        {
            get { return state == State.Active; }
        }

        public T ReadInbound<T>()
        {
            return (T) Poll(inboundMessages);
        }

        public T ReadOutbound<T>()
        {
            return (T) Poll(outboundMessages);
        }

        protected override IChannelUnsafe NewUnsafe()
        {
            return new DefaultUnsafe(this);
        }

        protected override bool IsCompatible(IEventLoop eventLoop)
        {
            return eventLoop is EmbeddedEventLoop;
        }

        protected override void DoBind(EndPoint localAddress)
        {
            //NOOP
        }

        protected override void DoRegister()
        {
            state = State.Active;
        }

        protected override void DoDisconnect()
        {
            DoClose();
        }

        protected override void DoClose()
        {
            state = State.Closed;
        }

        protected override void DoBeginRead()
        {
            //NOOP
        }

        protected override void DoWrite(ChannelOutboundBuffer input)
        {
            for (;;)
            {
                var msg = input.Current;
                if (msg == null)
                {
                    break;
                }

                ReferenceCountUtil.Retain(msg);
                OutboundMessages.Enqueue(msg);
                input.Remove();
            }
        }

        /// <summary>
        ///     Run all tasks (which also includes scheduled tasks) that are pending in the <see cref="IEventLoop" />
        ///     for this <see cref="IChannel" />.
        /// </summary>
        public void RunPendingTasks()
        {
            try
            {
                loop.RunTasks();
            }
            catch (Exception ex)
            {
                RecordException(ex);
            }
        }

        private void FinishPendingTasks()
        {
            RunPendingTasks();
        }

        /// <summary>
        ///     Write messages to the inbound of this <see cref="IChannel" />
        /// </summary>
        /// <param name="msgs">The messages to be written.</param>
        /// <returns><c>true</c> if the write operation did add something to the inbound buffer</returns>
        public bool WriteInbound(params object[] msgs)
        {
            EnsureOpen();
            if (msgs.Length == 0)
            {
                return IsNotEmpty(inboundMessages);
            }

            var p = Pipeline;
            foreach (var m in msgs)
            {
                p.FireChannelRead(m);
            }
            p.FireChannelReadComplete();
            RunPendingTasks();
            CheckException();
            return IsNotEmpty(inboundMessages);
        }

        /// <summary>
        ///     Write messages to the outbound of this <see cref="IChannel" />.
        /// </summary>
        /// <param name="msgs">The messages to be written.</param>
        /// <returns><c>true</c> if the write operation did add something to the inbound buffer</returns>
        public bool WriteOutbound(params object[] msgs)
        {
            EnsureOpen();
            if (msgs.Length == 0)
            {
                return IsNotEmpty(outboundMessages);
            }

            var futures = RecyclableArrayList.Take();

            try
            {
                foreach (var m in msgs)
                {
                    if (m == null)
                    {
                        break;
                    }
                    futures.Add(WriteAsync(m));
                }

                Flush();

                var size = futures.Count;
                for (var i = 0; i < size; i++)
                {
                    var future = (Task) futures[i];
                    Debug.Assert(future.IsCompleted);
                    if (future.Exception != null)
                    {
                        RecordException(future.Exception);
                    }
                }

                RunPendingTasks();
                CheckException();
                return IsNotEmpty(outboundMessages);
            }
            finally
            {
                futures.Return();
            }
        }

        private void RecordException(Exception cause)
        {
            if (lastException == null)
            {
                lastException = cause;
            }
            else
            {
                logger.Warning(
                    "More than one exception was raised. " +
                    "Will report only the first one and log others. Cause: {0}", cause);
            }
        }

        /// <summary>
        ///     Mark this <see cref="IChannel" /> as finished. Any further try to write data to it will fail.
        /// </summary>
        /// <returns>bufferReadable returns <c>true</c></returns>
        public bool Finish()
        {
            CloseAsync();
            CheckException();
            return IsNotEmpty(inboundMessages) || IsNotEmpty(outboundMessages);
        }

        public override Task CloseAsync()
        {
            var future = base.CloseAsync();
            FinishPendingTasks();
            return future;
        }

        public override Task DisconnectAsync()
        {
            var future = base.DisconnectAsync();
            FinishPendingTasks();
            return future;
        }

        /// <summary>
        ///     Check to see if there was any <see cref="Exception" /> and rethrow if so.
        /// </summary>
        public void CheckException()
        {
            var e = lastException;
            if (e == null)
            {
                return;
            }

            lastException = null;
            throw e;
        }

        /// <summary>
        ///     Ensure the <see cref="IChannel" /> is open and if not throw an exception.
        /// </summary>
        protected void EnsureOpen()
        {
            if (!IsOpen)
            {
                RecordException(ClosedChannelException.Instance);
                CheckException();
            }
        }

        private static bool IsNotEmpty(Queue<object> queue)
        {
            return queue != null && queue.Count > 0;
        }

        private static object Poll(Queue<object> queue)
        {
            return IsNotEmpty(queue) ? queue.Dequeue() : null;
        }

        private enum State
        {
            Open,
            Active,
            Closed
        }

        private class DefaultUnsafe : AbstractUnsafe
        {
            public DefaultUnsafe(AbstractChannel channel)
                : base(channel)
            {
            }

            public override Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress)
            {
                return TaskEx.Completed;
            }
        }

        internal sealed class LastInboundHandler : ChannelHandlerAdapter
        {
            private readonly Queue<object> inboundMessages;
            private readonly Action<Exception> recordException;

            public LastInboundHandler(Queue<object> inboundMessages, Action<Exception> recordException)
            {
                this.inboundMessages = inboundMessages;
                this.recordException = recordException;
            }

            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                // have to pass the EmbeddedChannel.InboundMessages by reference via the constructor
                inboundMessages.Enqueue(message);
            }

            public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
            {
                // have to pass the EmbeddedChannel.RecordException method via reference
                recordException(exception);
            }
        }
    }
}