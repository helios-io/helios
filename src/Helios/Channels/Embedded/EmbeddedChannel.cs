using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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
        static readonly EndPoint LOCAL_ADDRESS = new EmbeddedSocketAddress();
        static readonly EndPoint REMOTE_ADDRESS = new EmbeddedSocketAddress();

        enum State
        {
            Open,
            Active,
            Closed
        };

        static readonly IChannelHandler[] EMPTY_HANDLERS = new IChannelHandler[0];

        //TODO: ChannelMetadata
        static readonly ILogger logger = LoggingFactory.GetLogger<EmbeddedChannel>();

        readonly EmbeddedEventLoop loop = new EmbeddedEventLoop();
        readonly IChannelConfiguration config;

        Queue<object> inboundMessages;
        Queue<object> outboundMessages;
        Exception lastException;
        State state;

        public EmbeddedChannel()
            : this(EMPTY_HANDLERS)
        {
        }

        /// <summary>
        /// Create a new instance with the pipeline initialized with the specified handlers.
        /// </summary>
        /// <param name="id">The <see cref="IChannelId"/> of this channel.</param>
        /// <param name="handlers">
        /// The <see cref="IChannelHandler"/>s that will be added to the <see cref="IChannelPipeline"/>
        /// </param>
        public EmbeddedChannel(params IChannelHandler[] handlers)
            : base(EmbeddedChannelId.Instance, null)
        {
            this.config = new DefaultChannelConfiguration(this);
            if (handlers == null)
            {
                throw new NullReferenceException("handlers cannot be null");
            }

            IChannelPipeline p = this.Pipeline;
            p.AddLast(new ActionChannelInitializer<IChannel>(channel =>
            {
                IChannelPipeline pipeline = channel.Pipeline;
                foreach (IChannelHandler h in handlers)
                {
                    if (h == null)
                    {
                        break;
                    }
                    pipeline.AddLast(h);
                }
            }));

            Task future = this.loop.RegisterAsync(this);
            Debug.Assert(future.IsCompleted);
            p.AddLast(new LastInboundHandler(this.InboundMessages, this.RecordException));
        }

        public override IChannelConfiguration Configuration
        {
            get { return this.config; }
        }

        /// <summary>
        /// Returns the <see cref="Queue{T}"/> which holds all of the <see cref="object"/>s that 
        /// were received by this <see cref="IChannel"/>.
        /// </summary>
        public Queue<object> InboundMessages
        {
            get { return this.inboundMessages ?? (this.inboundMessages = new Queue<object>()); }
        }

        /// <summary>
        /// Returns the <see cref="Queue{T}"/> which holds all of the <see cref="object"/>s that 
        /// were written by this <see cref="IChannel"/>.
        /// </summary>
        public Queue<object> OutboundMessages
        {
            get { return this.outboundMessages ?? (this.outboundMessages = new Queue<object>()); }
        }

        public T ReadInbound<T>()
        {
            return (T)Poll(this.inboundMessages);
        }

        public T ReadOutbound<T>()
        {
            return (T)Poll(this.outboundMessages);
        }

        public override bool DisconnectSupported
        {
            get { return false; }
        }

        protected override EndPoint LocalAddressInternal
        {
            get { return this.IsActive ? LOCAL_ADDRESS : null; }
        }

        protected override EndPoint RemoteAddressInternal
        {
            get { return this.IsActive ? REMOTE_ADDRESS : null; }
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
            this.state = State.Active;
        }

        protected override void DoDisconnect()
        {
            this.DoClose();
        }

        protected override void DoClose()
        {
            this.state = State.Closed;
        }

        protected override void DoBeginRead()
        {
            //NOOP
        }

        protected override void DoWrite(ChannelOutboundBuffer input)
        {
            for (;;)
            {
                object msg = input.Current;
                if (msg == null)
                {
                    break;
                }

                // TODO: reference counting
                //ReferenceCountUtil.Retain(msg);
                this.OutboundMessages.Enqueue(msg);
                input.Remove();
            }
        }

        public override bool IsOpen
        {
            get { return this.state != State.Closed; }
        }

        public override bool IsActive
        {
            get { return this.state == State.Active; }
        }

        /// <summary>
        /// Run all tasks (which also includes scheduled tasks) that are pending in the <see cref="IEventLoop"/>
        /// for this <see cref="IChannel"/>.
        /// </summary>
        public void RunPendingTasks()
        {
            try
            {
                this.loop.RunTasks();
            }
            catch (Exception ex)
            {
                this.RecordException(ex);
            }
        }

        void FinishPendingTasks()
        {
            this.RunPendingTasks();
        }

        /// <summary>
        /// Write messages to the inbound of this <see cref="IChannel"/>
        /// </summary>
        /// <param name="msgs">The messages to be written.</param>
        /// <returns><c>true</c> if the write operation did add something to the inbound buffer</returns>
        public bool WriteInbound(params object[] msgs)
        {
            this.EnsureOpen();
            if (msgs.Length == 0)
            {
                return IsNotEmpty(this.inboundMessages);
            }

            IChannelPipeline p = this.Pipeline;
            foreach (object m in msgs)
            {
                p.FireChannelRead(m);
            }
            p.FireChannelReadComplete();
            this.RunPendingTasks();
            this.CheckException();
            return IsNotEmpty(this.inboundMessages);
        }

        /// <summary>
        /// Write messages to the outbound of this <see cref="IChannel"/>.
        /// </summary>
        /// <param name="msgs">The messages to be written.</param>
        /// <returns><c>true</c> if the write operation did add something to the inbound buffer</returns>
        public bool WriteOutbound(params object[] msgs)
        {
            this.EnsureOpen();
            if (msgs.Length == 0)
            {
                return IsNotEmpty(this.outboundMessages);
            }

            var futures = RecyclableArrayList.Take();

            try
            {
                foreach (object m in msgs)
                {
                    if (m == null)
                    {
                        break;
                    }
                    futures.Add(this.WriteAsync(m));
                }

                this.Flush();

                int size = futures.Count;
                for (int i = 0; i < size; i++)
                {
                    Task future = (Task) futures[i];
                    Debug.Assert(future.IsCompleted);
                    if (future.Exception != null)
                    {
                        this.RecordException(future.Exception);
                    }
                }

                this.RunPendingTasks();
                this.CheckException();
                return IsNotEmpty(this.outboundMessages);
            }
            finally
            {
                futures.Return();
            }
        }

        void RecordException(Exception cause)
        {
            if (this.lastException == null)
            {
                this.lastException = cause;
            }
            else
            {
                logger.Warning(
                    "More than one exception was raised. " +
                        "Will report only the first one and log others. Cause: {0}", cause);
            }
        }

        /// <summary>
        /// Mark this <see cref="IChannel"/> as finished. Any further try to write data to it will fail.
        /// </summary>
        /// <returns>bufferReadable returns <c>true</c></returns>
        public bool Finish()
        {
            this.CloseAsync();
            this.CheckException();
            return IsNotEmpty(this.inboundMessages) || IsNotEmpty(this.outboundMessages);
        }

        public override Task CloseAsync()
        {
            Task future = base.CloseAsync();
            this.FinishPendingTasks();
            return future;
        }

        public override Task DisconnectAsync()
        {
            Task future = base.DisconnectAsync();
            this.FinishPendingTasks();
            return future;
        }

        /// <summary>
        /// Check to see if there was any <see cref="Exception"/> and rethrow if so.
        /// </summary>
        public void CheckException()
        {
            Exception e = this.lastException;
            if (e == null)
            {
                return;
            }

            this.lastException = null;
            throw e;
        }

        /// <summary>
        /// Ensure the <see cref="IChannel"/> is open and if not throw an exception.
        /// </summary>
        protected void EnsureOpen()
        {
            if (!this.IsOpen)
            {
                this.RecordException(ClosedChannelException.Instance);
                this.CheckException();
            }
        }

        static bool IsNotEmpty(Queue<object> queue)
        {
            return queue != null && queue.Count > 0;
        }

        static object Poll(Queue<object> queue)
        {
            return IsNotEmpty(queue) ? queue.Dequeue() : null;
        }

        class DefaultUnsafe : AbstractUnsafe
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
            readonly Queue<object> inboundMessages;
            readonly Action<Exception> recordException;

            public LastInboundHandler(Queue<object> inboundMessages, Action<Exception> recordException)
            {
                this.inboundMessages = inboundMessages;
                this.recordException = recordException;
            }

            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                // have to pass the EmbeddedChannel.InboundMessages by reference via the constructor
                this.inboundMessages.Enqueue(message);
            }

            public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
            {
                // have to pass the EmbeddedChannel.RecordException method via reference
                this.recordException(exception);
            }
        }
    }
}
