using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Helios.Concurrency;
using Helios.Util.Concurrency;

namespace Helios.Channels.Bootstrap
{
    public abstract class AbstractBootstrap<TBootstrap, TChannel> : ICloneable
        where TBootstrap : AbstractBootstrap<TBootstrap, TChannel>
        where TChannel : IChannel
    {
        volatile IEventLoopGroup _group;
        volatile Func<TChannel> _channelFactory;
        volatile EndPoint _localAddress;
        readonly ConcurrentDictionary<ChannelOption, object> _options;
        volatile IChannelHandler _handler;

        protected internal AbstractBootstrap()
        {
            this._options = new ConcurrentDictionary<ChannelOption, object>();
            // Disallow extending from a different package.
        }

        protected internal AbstractBootstrap(AbstractBootstrap<TBootstrap, TChannel> clientBootstrap)
        {
            this._group = clientBootstrap._group;
            this._channelFactory = clientBootstrap._channelFactory;
            this._handler = clientBootstrap._handler;
            this._localAddress = clientBootstrap._localAddress;
            this._options = new ConcurrentDictionary<ChannelOption, object>(clientBootstrap._options);
        }

        /// <summary>
        /// The {@link EventLoopGroup} which is used to handle all the events for the to-be-created
        /// {@link Channel}
        /// </summary>
        public virtual TBootstrap Group(IEventLoopGroup group)
        {
            Contract.Requires(group != null);
            if (this._group != null)
            {
                throw new InvalidOperationException("group has already been set.");
            }
            this._group = group;
            return (TBootstrap)this;
        }

        /// <summary>
        /// The {@link Class} which is used to create {@link Channel} instances from.
        /// You either use this or {@link #channelFactory(io.netty.channel.ChannelFactory)} if your
        /// {@link Channel} implementation has no no-args constructor.
        /// </summary>
        public TBootstrap Channel<T>()
            where T : TChannel, new()
        {
            return this.ChannelFactory(() => new T());
        }

        public TBootstrap ChannelFactory(Func<TChannel> channelFactory)
        {
            Contract.Requires(channelFactory != null);
            this._channelFactory = channelFactory;
            return (TBootstrap)this;
        }

        /// <summary>
        /// The {@link SocketAddress} which is used to bind the local "end" to.
        /// </summary>
        public TBootstrap LocalAddress(EndPoint localAddress)
        {
            this._localAddress = localAddress;
            return (TBootstrap)this;
        }

        /// <summary>
        /// @see {@link #localAddress(SocketAddress)}
        /// </summary>
        public TBootstrap LocalAddress(int inetPort)
        {
            return this.LocalAddress(new IPEndPoint(IPAddress.Any, inetPort));
        }

        /// <summary>
        /// @see {@link #localAddress(SocketAddress)}
        /// </summary>
        public TBootstrap LocalAddress(string inetHost, int inetPort)
        {
            return this.LocalAddress(new DnsEndPoint(inetHost, inetPort));
        }

        /// <summary>
        /// @see {@link #localAddress(SocketAddress)}
        /// </summary>
        public TBootstrap LocalAddress(IPAddress inetHost, int inetPort)
        {
            return this.LocalAddress(new IPEndPoint(inetHost, inetPort));
        }

        /// <summary>
        /// Allow to specify a {@link ChannelOption} which is used for the {@link Channel} instances once they got
        /// created. Use a value of {@code null} to remove a previous set {@link ChannelOption}.
        /// </summary>
        public TBootstrap Option<T>(ChannelOption<T> option, T value)
        {
            Contract.Requires(option != null);
            if (value == null)
            {
                object removed;
                this._options.TryRemove(option, out removed);
            }
            else
            {
                this._options[option] = value;
            }
            return (TBootstrap)this;
        }

        public virtual TBootstrap Validate()
        {
            if (this._group == null)
            {
                throw new InvalidOperationException("group not set");
            }
            if (this._channelFactory == null)
            {
                throw new InvalidOperationException("channel or channelFactory not set");
            }
            return (TBootstrap)this;
        }

        /// <summary>
        /// Returns a deep clone of this bootstrap which has the identical configuration.  This method is useful when making
        /// multiple {@link Channel}s with similar settings.  Please note that this method does not clone the
        /// {@link EventLoopGroup} deeply but shallowly, making the group a shared resource.
        /// </summary>
        public abstract object Clone();

        /// <summary>
        /// Create a new {@link Channel} and register it with an {@link EventLoop}.
        /// </summary>
        public Task Register()
        {
            this.Validate();
            return this.InitAndRegisterAsync();
        }

        /// <summary>
        /// Create a new {@link Channel} and bind it.
        /// </summary>
        public Task<IChannel> BindAsync()
        {
            this.Validate();
            EndPoint address = this._localAddress;
            if (address == null)
            {
                throw new InvalidOperationException("localAddress must be set beforehand.");
            }
            return this.DoBind(address);
        }

        /// <summary>
        /// Create a new {@link Channel} and bind it.
        /// </summary>
        public Task<IChannel> BindAsync(int inetPort)
        {
            return this.BindAsync(new IPEndPoint(IPAddress.Any, inetPort));
        }

        /// <summary>
        /// Create a new {@link Channel} and bind it.
        /// </summary>
        public Task<IChannel> BindAsync(string inetHost, int inetPort)
        {
            return this.BindAsync(new DnsEndPoint(inetHost, inetPort));
        }

        /// <summary>
        /// Create a new {@link Channel} and bind it.
        /// </summary>
        public Task<IChannel> BindAsync(IPAddress inetHost, int inetPort)
        {
            return this.BindAsync(new IPEndPoint(inetHost, inetPort));
        }

        /// <summary>
        /// Create a new {@link Channel} and bind it.
        /// </summary>
        public Task<IChannel> BindAsync(EndPoint localAddress)
        {
            this.Validate();
            Contract.Requires(localAddress != null);

            return this.DoBind(localAddress);
        }

        async Task<IChannel> DoBind(EndPoint localAddress)
        {
            IChannel channel = await this.InitAndRegisterAsync();
            await DoBind0(channel, localAddress);

            return channel;
        }

        protected async Task<IChannel> InitAndRegisterAsync()
        {
            IChannel channel = this._channelFactory();
            try
            {
                this.Init(channel);
            }
            catch (Exception ex)
            {
                channel.Unsafe.CloseForcibly();
                // as the Channel is not registered yet we need to force the usage of the GlobalEventExecutor
                throw;
            }

            try
            {
                await this.Group().GetNext().RegisterAsync(channel);
            }
            catch (Exception)
            {
                if (channel.Registered)
                {
                    await channel.CloseAsync();
                }
                else
                {
                    channel.Unsafe.CloseForcibly();
                }
                throw;
            }

            // If we are here and the promise is not failed, it's one of the following cases:
            // 1) If we attempted registration from the event loop, the registration has been completed at this point.
            //    i.e. It's safe to attempt bind() or connect() now because the channel has been registered.
            // 2) If we attempted registration from the other thread, the registration request has been successfully
            //    added to the event loop's task queue for later execution.
            //    i.e. It's safe to attempt bind() or connect() now:
            //         because bind() or connect() will be executed *after* the scheduled registration task is executed
            //         because register(), bind(), and connect() are all bound to the same thread.

            return channel;
        }

        static Task DoBind0(IChannel channel, EndPoint localAddress)
        {
            // This method is invoked before channelRegistered() is triggered.  Give user handlers a chance to set up
            // the pipeline in its channelRegistered() implementation.
            var promise = new TaskCompletionSource();
            channel.EventLoop.Execute(() =>
            {
                try
                {
                    channel.BindAsync(localAddress).LinkOutcome(promise);
                }
                catch (Exception ex)
                {
                    channel.CloseAsync();
                    promise.TrySetException(ex);
                }
            });
            return promise.Task;
        }

        protected abstract void Init(IChannel channel);

        /// <summary>
        /// the {@link ChannelHandler} to use for serving the requests.
        /// </summary>
        public TBootstrap Handler(IChannelHandler handler)
        {
            Contract.Requires(handler != null);
            this._handler = handler;
            return (TBootstrap)this;
        }

        protected EndPoint LocalAddress()
        {
            return this._localAddress;
        }

        protected IChannelHandler Handler()
        {
            return this._handler;
        }

        /// <summary>
        /// Return the configured {@link EventLoopGroup} or {@code null} if non is configured yet.
        /// </summary>
        public IEventLoopGroup Group()
        {
            return this._group;
        }

        protected IDictionary<ChannelOption, object> Options()
        {
            return this._options;
        }
    }
}
