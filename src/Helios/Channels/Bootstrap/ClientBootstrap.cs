// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Helios.Concurrency;
using Helios.Logging;
using Helios.Util.Concurrency;

namespace Helios.Channels.Bootstrap
{
    /// <summary>
    /// A {@link Bootstrap} that makes it easy to bootstrap a {@link Channel} to use
    /// for clients.
    ///
    /// <p>The {@link #bind()} methods are useful in combination with connectionless transports such as datagram (UDP).
    /// For regular TCP connections, please use the provided {@link #connect()} methods.</p>
    /// </summary>
    public class ClientBootstrap : AbstractBootstrap<ClientBootstrap, IChannel>
    {
        static readonly ILogger Logger = LoggingFactory.GetLogger<ClientBootstrap>();

        static readonly INameResolver DefaultResolver = new DefaultNameResolver();

        volatile INameResolver _resolver = DefaultResolver;
        volatile EndPoint _remoteAddress;

        public ClientBootstrap()
        {
        }

        ClientBootstrap(ClientBootstrap clientBootstrap)
            : base(clientBootstrap)
        {
            this._resolver = clientBootstrap._resolver;
            this._remoteAddress = clientBootstrap._remoteAddress;
        }

        /// <summary>
        /// Sets the {@link NameResolver} which will resolve the address of the unresolved named address.
        /// </summary>
        public ClientBootstrap Resolver(INameResolver resolver)
        {
            Contract.Requires(resolver != null);
            this._resolver = resolver;
            return this;
        }

        /// <summary>
        /// The {@link SocketAddress} to connect to once the {@link #connect()} method
        /// is called.
        /// </summary>
        public ClientBootstrap RemoteAddress(EndPoint remoteAddress)
        {
            this._remoteAddress = remoteAddress;
            return this;
        }

        /// <summary>
        /// @see {@link #remoteAddress(SocketAddress)}
        /// </summary>
        public ClientBootstrap RemoteAddress(string inetHost, int inetPort)
        {
            this._remoteAddress = new DnsEndPoint(inetHost, inetPort);
            return this;
        }

        /// <summary>
        /// @see {@link #remoteAddress(SocketAddress)}
        /// </summary>
        public ClientBootstrap RemoteAddress(IPAddress inetHost, int inetPort)
        {
            this._remoteAddress = new IPEndPoint(inetHost, inetPort);
            return this;
        }

        /// <summary>
        /// Connect a {@link Channel} to the remote peer.
        /// </summary>
        public Task<IChannel> ConnectAsync()
        {
            this.Validate();
            EndPoint remoteAddress = this._remoteAddress;
            if (remoteAddress == null)
            {
                throw new InvalidOperationException("remoteAddress not set");
            }

            return this.DoResolveAndConnect(remoteAddress, this.LocalAddress());
        }

        /// <summary>
        /// Connect a {@link Channel} to the remote peer.
        /// </summary>
        public Task<IChannel> ConnectAsync(string inetHost, int inetPort)
        {
            return this.ConnectAsync(new DnsEndPoint(inetHost, inetPort));
        }

        /// <summary>
        /// Connect a {@link Channel} to the remote peer.
        /// </summary>
        public Task<IChannel> ConnectAsync(IPAddress inetHost, int inetPort)
        {
            return this.ConnectAsync(new IPEndPoint(inetHost, inetPort));
        }

        /// <summary>
        /// Connect a {@link Channel} to the remote peer.
        /// </summary>
        public Task<IChannel> ConnectAsync(EndPoint remoteAddress)
        {
            Contract.Requires(remoteAddress != null);

            this.Validate();
            return this.DoResolveAndConnect(remoteAddress, this.LocalAddress());
        }

        /// <summary>
        /// Connect a {@link Channel} to the remote peer.
        /// </summary>
        public Task<IChannel> ConnectAsync(EndPoint remoteAddress, EndPoint localAddress)
        {
            Contract.Requires(remoteAddress != null);

            this.Validate();
            return this.DoResolveAndConnect(remoteAddress, localAddress);
        }

        /// <summary>
        /// @see {@link #connect()}
        /// </summary>
        async Task<IChannel> DoResolveAndConnect(EndPoint remoteAddress, EndPoint localAddress)
        {
            IChannel channel = await this.InitAndRegisterAsync();

            if (this._resolver.IsResolved(remoteAddress))
            {
                // Resolver has no idea about what to do with the specified remote address or it's resolved already.
                await DoConnect(channel, remoteAddress, localAddress);
                return channel;
            }

            EndPoint resolvedAddress;
            try
            {
                resolvedAddress = await this._resolver.ResolveAsync(remoteAddress, PreferredDnsResolutionFamily());
            }
            catch (Exception ex)
            {
                await channel.CloseAsync();
                throw;
            }

            await DoConnect(channel, resolvedAddress, localAddress);
            return channel;
        }

        static Task DoConnect(IChannel channel,
            EndPoint remoteAddress, EndPoint localAddress)
        {
            // This method is invoked before channelRegistered() is triggered.  Give user handlers a chance to set up
            // the pipeline in its channelRegistered() implementation.
            var promise = new TaskCompletionSource();
            channel.EventLoop.Execute(() =>
            {
                try
                {
                    if (localAddress == null)
                    {
                        channel.ConnectAsync(remoteAddress).LinkOutcome(promise);
                    }
                    else
                    {
                        channel.ConnectAsync(remoteAddress, localAddress).LinkOutcome(promise);
                    }
                }
                catch (Exception ex)
                {
                    channel.CloseAsync();
                    promise.TrySetException(ex);
                }
            });
            return promise.Task;
        }

        protected override void Init(IChannel channel)
        {
            IChannelPipeline p = channel.Pipeline;
            p.AddLast(null, (string) null, this.Handler());

            IDictionary<ChannelOption, object> options = this.Options();
            foreach (KeyValuePair<ChannelOption, object> e in options)
            {
                try
                {
                    if (!channel.Configuration.SetOption(e.Key, e.Value))
                    {
                        Logger.Warning("Unknown channel option: " + e);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning("Failed to set a channel option: " + channel + " Cause: {0}", ex);
                }
            }
        }

        public override ClientBootstrap Validate()
        {
            base.Validate();
            if (this.Handler() == null)
            {
                throw new InvalidOperationException("handler not set");
            }
            return this;
        }

        public override object Clone()
        {
            return new ClientBootstrap(this);
        }

        /// <summary>
        /// Returns a deep clone of this bootstrap which has the identical configuration except that it uses
        /// the given {@link EventLoopGroup}. This method is useful when making multiple {@link Channel}s with similar
        /// settings.
        /// </summary>
        public ClientBootstrap Clone(IEventLoopGroup group)
        {
            var bs = new ClientBootstrap(this);
            bs.Group(group);
            return bs;
        }

        public override string ToString()
        {
            if (this._remoteAddress == null)
            {
                return base.ToString();
            }

            var buf = new StringBuilder(base.ToString());
            buf.Length = buf.Length - 1;

            return buf.Append(", remoteAddress: ")
                .Append(this._remoteAddress)
                .Append(')')
                .ToString();
        }
    }
}