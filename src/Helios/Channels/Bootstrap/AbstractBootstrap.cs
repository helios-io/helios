// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Helios.Concurrency;
using Helios.Util.Concurrency;

namespace Helios.Channels.Bootstrap
{
    public abstract class AbstractBootstrap<TBootstrap, TChannel> : ICloneable
        where TBootstrap : AbstractBootstrap<TBootstrap, TChannel>
        where TChannel : IChannel
    {
        private readonly ConcurrentDictionary<ChannelOption, object> _options;
        private volatile Func<TChannel> _channelFactory;
        private volatile IEventLoopGroup _group;
        private volatile IChannelHandler _handler;
        private volatile EndPoint _localAddress;
        private volatile AddressFamily _preferredAddressFamily = AddressFamily.InterNetworkV6;

        protected internal AbstractBootstrap()
        {
            _options = new ConcurrentDictionary<ChannelOption, object>();
            // Disallow extending from a different package.
        }

        protected internal AbstractBootstrap(AbstractBootstrap<TBootstrap, TChannel> clientBootstrap)
        {
            _group = clientBootstrap._group;
            _channelFactory = clientBootstrap._channelFactory;
            _handler = clientBootstrap._handler;
            _localAddress = clientBootstrap._localAddress;
            _preferredAddressFamily = clientBootstrap._preferredAddressFamily;
            _options = new ConcurrentDictionary<ChannelOption, object>(clientBootstrap._options);
        }

        /// <summary>
        ///     Returns a deep clone of this bootstrap which has the identical configuration.  This method is useful when making
        ///     multiple {@link Channel}s with similar settings.  Please note that this method does not clone the
        ///     {@link EventLoopGroup} deeply but shallowly, making the group a shared resource.
        /// </summary>
        public abstract object Clone();

        /// <summary>
        ///     The {@link EventLoopGroup} which is used to handle all the events for the to-be-created
        ///     {@link Channel}
        /// </summary>
        public virtual TBootstrap Group(IEventLoopGroup group)
        {
            Contract.Requires(group != null);
            if (_group != null)
            {
                throw new InvalidOperationException("group has already been set.");
            }
            _group = group;
            return (TBootstrap) this;
        }

        /// <summary>
        ///     The {@link Class} which is used to create {@link Channel} instances from.
        ///     You either use this or {@link #channelFactory(io.netty.channel.ChannelFactory)} if your
        ///     {@link Channel} implementation has no no-args constructor.
        /// </summary>
        public TBootstrap Channel<T>()
            where T : TChannel, new()
        {
            return ChannelFactory(() => new T());
        }

        public TBootstrap ChannelFactory(Func<TChannel> channelFactory)
        {
            Contract.Requires(channelFactory != null);
            _channelFactory = channelFactory;
            return (TBootstrap) this;
        }

        /// <summary>
        ///     The {@link SocketAddress} which is used to bind the local "end" to.
        /// </summary>
        public TBootstrap LocalAddress(EndPoint localAddress)
        {
            _localAddress = localAddress;
            return (TBootstrap) this;
        }

        /// <summary>
        ///     @see {@link #localAddress(SocketAddress)}
        /// </summary>
        public TBootstrap LocalAddress(int inetPort)
        {
            return LocalAddress(new IPEndPoint(IPAddress.Any, inetPort));
        }

        /// <summary>
        ///     @see {@link #localAddress(SocketAddress)}
        /// </summary>
        public TBootstrap LocalAddress(string inetHost, int inetPort)
        {
            return LocalAddress(new DnsEndPoint(inetHost, inetPort));
        }

        /// <summary>
        ///     @see {@link #localAddress(SocketAddress)}
        /// </summary>
        public TBootstrap LocalAddress(IPAddress inetHost, int inetPort)
        {
            return LocalAddress(new IPEndPoint(inetHost, inetPort));
        }

        /// <summary>
        /// Specifies the default DNS resolution family for this boostrapper.
        /// </summary>
        /// <param name="addressFamily">The address family to use.</param>
        /// <returns>The current bootstrap instance.</returns>
        public TBootstrap PreferredDnsResolutionFamily(AddressFamily addressFamily)
        {
            _preferredAddressFamily = addressFamily;
            return (TBootstrap) this;
        }

        /// <summary>
        ///     Allow to specify a {@link ChannelOption} which is used for the {@link Channel} instances once they got
        ///     created. Use a value of {@code null} to remove a previous set {@link ChannelOption}.
        /// </summary>
        public TBootstrap Option<T>(ChannelOption<T> option, T value)
        {
            Contract.Requires(option != null);
            if (value == null)
            {
                object removed;
                _options.TryRemove(option, out removed);
            }
            else
            {
                _options[option] = value;
            }
            return (TBootstrap) this;
        }

        public virtual TBootstrap Validate()
        {
            if (_group == null)
            {
                throw new InvalidOperationException("group not set");
            }
            if (_channelFactory == null)
            {
                throw new InvalidOperationException("channel or channelFactory not set");
            }
            return (TBootstrap) this;
        }

        /// <summary>
        ///     Create a new {@link Channel} and register it with an {@link EventLoop}.
        /// </summary>
        public Task<IChannel> Register()
        {
            Validate();
            return InitAndRegisterAsync();
        }

        /// <summary>
        ///     Create a new {@link Channel} and bind it.
        /// </summary>
        public Task<IChannel> BindAsync()
        {
            Validate();
            var address = _localAddress;
            if (address == null)
            {
                throw new InvalidOperationException("localAddress must be set beforehand.");
            }
            return DoBind(address);
        }

        /// <summary>
        ///     Create a new {@link Channel} and bind it.
        /// </summary>
        public Task<IChannel> BindAsync(int inetPort)
        {
            return BindAsync(new IPEndPoint(IPAddress.Any, inetPort));
        }

        /// <summary>
        ///     Create a new {@link Channel} and bind it.
        /// </summary>
        public Task<IChannel> BindAsync(string inetHost, int inetPort)
        {
            return BindAsync(new DnsEndPoint(inetHost, inetPort));
        }

        /// <summary>
        ///     Create a new {@link Channel} and bind it.
        /// </summary>
        public Task<IChannel> BindAsync(IPAddress inetHost, int inetPort)
        {
            return BindAsync(new IPEndPoint(inetHost, inetPort));
        }

        /// <summary>
        ///     Create a new {@link Channel} and bind it.
        /// </summary>
        public virtual Task<IChannel> BindAsync(EndPoint localAddress)
        {
            Validate();
            Contract.Requires(localAddress != null);

            return DoBind(localAddress);
        }

        private async Task<IChannel> DoBind(EndPoint localAddress)
        {
            var channel = await InitAndRegisterAsync();
            await DoBind0(channel, localAddress);

            return channel;
        }

        protected async Task<IChannel> InitAndRegisterAsync()
        {
            IChannel channel = _channelFactory();
            try
            {
                Init(channel);
            }
            catch (Exception ex)
            {
                channel.Unsafe.CloseForcibly();
                // as the Channel is not registered yet we need to force the usage of the GlobalEventExecutor
                throw;
            }

            try
            {
                await Group().GetNext().RegisterAsync(channel);
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

        private static Task DoBind0(IChannel channel, EndPoint localAddress)
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
        ///     the {@link ChannelHandler} to use for serving the requests.
        /// </summary>
        public TBootstrap Handler(IChannelHandler handler)
        {
            Contract.Requires(handler != null);
            _handler = handler;
            return (TBootstrap) this;
        }

        protected AddressFamily PreferredDnsResolutionFamily()
        {
            return _preferredAddressFamily;
        }

        protected EndPoint LocalAddress()
        {
            return _localAddress;
        }

        protected IChannelHandler Handler()
        {
            return _handler;
        }

        /// <summary>
        ///     Return the configured {@link EventLoopGroup} or {@code null} if non is configured yet.
        /// </summary>
        public IEventLoopGroup Group()
        {
            return _group;
        }

        protected IDictionary<ChannelOption, object> Options()
        {
            return _options;
        }
    }
}