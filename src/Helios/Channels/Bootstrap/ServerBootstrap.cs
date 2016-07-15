// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Helios.Logging;

namespace Helios.Channels.Bootstrap
{
    /// <summary>
    ///     {@link Bootstrap} sub-class which allows easy bootstrap of {@link ServerChannel}
    /// </summary>
    public class ServerBootstrap : AbstractBootstrap<ServerBootstrap, IServerChannel>
    {
        private static readonly ILogger Logger = LoggingFactory.GetLogger<ServerBootstrap>();

        private readonly ConcurrentDictionary<ChannelOption, object> _childOptions;

        static readonly INameResolver DefaultResolver = new DefaultNameResolver();

        volatile INameResolver _resolver = DefaultResolver;

        private volatile IEventLoopGroup _childGroup;
        private volatile IChannelHandler _childHandler;

        public ServerBootstrap()
        {
            _childOptions = new ConcurrentDictionary<ChannelOption, object>();
        }

        private ServerBootstrap(ServerBootstrap bootstrap)
            : base(bootstrap)
        {
            _childGroup = bootstrap._childGroup;
            _childHandler = bootstrap._childHandler;
            _childOptions = new ConcurrentDictionary<ChannelOption, object>(bootstrap._childOptions);
        }

        /// <summary>
        /// Sets the <see cref="INameResolver"/> which will resolve the address of the unresolved named address.
        /// </summary>
        public ServerBootstrap Resolver(INameResolver resolver)
        {
            Contract.Requires(resolver != null);
            this._resolver = resolver;
            return this;
        }

        /// <summary>
        ///     Specify the {@link EventLoopGroup} which is used for the parent (acceptor) and the child (client).
        /// </summary>
        public override ServerBootstrap Group(IEventLoopGroup group)
        {
            return Group(group, group);
        }

        /// <summary>
        ///     Set the {@link EventLoopGroup} for the parent (acceptor) and the child (client). These
        ///     {@link EventLoopGroup}'s are used to handle all the events and IO for {@link ServerChannel} and
        ///     {@link Channel}'s.
        /// </summary>
        public ServerBootstrap Group(IEventLoopGroup parentGroup, IEventLoopGroup childGroup)
        {
            Contract.Requires(childGroup != null);

            base.Group(parentGroup);
            if (_childGroup != null)
            {
                throw new InvalidOperationException("childGroup set already");
            }
            _childGroup = childGroup;
            return this;
        }

        /// <summary>
        ///     Allow to specify a {@link ChannelOption} which is used for the {@link Channel} instances once they get created
        ///     (after the acceptor accepted the {@link Channel}). Use a value of {@code null} to remove a previous set
        ///     {@link ChannelOption}.
        /// </summary>
        public ServerBootstrap ChildOption<T>(ChannelOption<T> childOption, T value)
        {
            Contract.Requires(childOption != null);

            if (value == null)
            {
                object removed;
                _childOptions.TryRemove(childOption, out removed);
            }
            else
            {
                _childOptions[childOption] = value;
            }
            return this;
        }

        public ServerBootstrap ChildHandler(IChannelHandler childHandler)
        {
            Contract.Requires(childHandler != null);

            _childHandler = childHandler;
            return this;
        }

        /// <summary>
        ///     Return the configured {@link EventLoopGroup} which will be used for the child channels or {@code null}
        ///     if non is configured yet.
        /// </summary>
        public IEventLoopGroup ChildGroup()
        {
            return _childGroup;
        }

        protected override void Init(IChannel channel)
        {
            var options = Options();
            foreach (var e in options)
            {
                try
                {
                    if (!channel.Configuration.SetOption(e.Key, e.Value))
                    {
                        Logger.Warning("Unknown channel option: " + e.Key);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning("Failed to set a channel option: " + channel + " Cause: {0}", ex);
                }
            }

            var p = channel.Pipeline;
            if (Handler() != null)
            {
                p.AddLast(Handler());
            }

            var currentChildGroup = _childGroup;
            var currentChildHandler = _childHandler;
            var currentChildOptions = _childOptions.ToArray();

            var childConfigSetupFunc = CompileOptionsSetupFunc(_childOptions);

            p.AddLast(new ActionChannelInitializer<IChannel>(ch =>
            {
                ch.Pipeline.AddLast(new ServerBootstrapAcceptor(currentChildGroup, currentChildHandler,
                    childConfigSetupFunc /*, currentChildAttrs*/));
            }));
        }

        public async Task<IChannel> BindAsync(DnsEndPoint endpoint)
        {
            EndPoint ep = await this._resolver.ResolveAsync(endpoint);

            return await BindAsync(ep);
        }

        public override async Task<IChannel> BindAsync(EndPoint localAddress)
        {
            if (!this._resolver.IsResolved(localAddress))
            {
                localAddress = await this._resolver.ResolveAsync(localAddress, PreferredDnsResolutionFamily());
            }
            return await base.BindAsync(localAddress);
        }

        public override ServerBootstrap Validate()
        {
            base.Validate();
            if (_childHandler == null)
            {
                throw new InvalidOperationException("childHandler not set");
            }
            if (_childGroup == null)
            {
                Logger.Warning("childGroup is not set. Using parentGroup instead.");
                _childGroup = Group();
            }
            return this;
        }

        private static Func<IChannelConfiguration, bool> CompileOptionsSetupFunc(
            IDictionary<ChannelOption, object> templateOptions)
        {
            if (templateOptions.Count == 0)
            {
                return null;
            }

            var configParam = Expression.Parameter(typeof(IChannelConfiguration));
            var resultVariable = Expression.Variable(typeof(bool));
            var assignments = new List<Expression>
            {
                Expression.Assign(resultVariable, Expression.Constant(true))
            };

            var setOptionMethodDefinition = typeof(IChannelConfiguration)
                .FindMembers(MemberTypes.Method, BindingFlags.Instance | BindingFlags.Public, Type.FilterName,
                    "SetOption")
                .Cast<MethodInfo>()
                .First(x => x.IsGenericMethodDefinition);

            foreach (var p in templateOptions)
            {
                // todo: emit log if verbose is enabled && option is missing
                var optionType = p.Key.GetType();
                if (!optionType.IsGenericType)
                {
                    throw new InvalidOperationException("Only options of type ChannelOption<T> are supported.");
                }
                if (optionType.GetGenericTypeDefinition() != typeof(ChannelOption<>))
                {
                    throw new NotSupportedException(
                        string.Format(
                            "Channel option is of an unsupported type `{0}`. Only ChannelOption and ChannelOption<T> are supported.",
                            optionType));
                }
                var valueType = optionType.GetGenericArguments()[0];
                var setOptionMethod = setOptionMethodDefinition.MakeGenericMethod(valueType);
                assignments.Add(Expression.Assign(
                    resultVariable,
                    Expression.AndAlso(
                        resultVariable,
                        Expression.Call(configParam, setOptionMethod, Expression.Constant(p.Key),
                            Expression.Constant(p.Value, valueType)))));
            }

            return
                Expression.Lambda<Func<IChannelConfiguration, bool>>(
                    Expression.Block(typeof(bool), new[] {resultVariable}, assignments), configParam).Compile();
        }

        public override object Clone()
        {
            return new ServerBootstrap(this);
        }

        public override string ToString()
        {
            var buf = new StringBuilder(base.ToString());
            buf.Length = buf.Length - 1;
            buf.Append(", ");
            if (_childGroup != null)
            {
                buf.Append("childGroup: ")
                    .Append(_childGroup.GetType().Name)
                    .Append(", ");
            }
            buf.Append("childOptions: ")
                .Append(_childOptions)
                .Append(", ");

            if (_childHandler != null)
            {
                buf.Append("childHandler: ");
                buf.Append(_childHandler);
                buf.Append(", ");
            }
            if (buf[buf.Length - 1] == '(')
            {
                buf.Append(')');
            }
            else
            {
                buf[buf.Length - 2] = ')';
                buf.Length = buf.Length - 1;
            }

            return buf.ToString();
        }

        private class ServerBootstrapAcceptor : ChannelHandlerAdapter
        {
            private readonly IEventLoopGroup childGroup;
            private readonly IChannelHandler childHandler;
            private readonly Func<IChannelConfiguration, bool> childOptionsSetupFunc;

            public ServerBootstrapAcceptor(
                IEventLoopGroup childGroup, IChannelHandler childHandler,
                Func<IChannelConfiguration, bool> childOptionsSetupFunc)
            {
                this.childGroup = childGroup;
                this.childHandler = childHandler;
                this.childOptionsSetupFunc = childOptionsSetupFunc;
            }

            public override void ChannelRead(IChannelHandlerContext ctx, object msg)
            {
                var child = (IChannel) msg;

                child.Pipeline.AddLast(childHandler);

                if (childOptionsSetupFunc != null)
                {
                    if (!childOptionsSetupFunc(child.Configuration))
                    {
                        Logger.Warning("Not all configuration options could be set.");
                    }
                }

                // todo: async/await instead?
                try
                {
                    childGroup.GetNext()
                        .RegisterAsync(child)
                        .ContinueWith(future => ForceClose(child, future.Exception),
                            TaskContinuationOptions.NotOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);
                }
                catch (Exception ex)
                {
                    ForceClose(child, ex);
                }
            }

            private static void ForceClose(IChannel child, Exception ex)
            {
                child.Unsafe.CloseForcibly();
                Logger.Warning("Failed to register an accepted channel: " + child, ex);
            }

            public override void ExceptionCaught(IChannelHandlerContext ctx, Exception cause)
            {
                var config = ctx.Channel.Configuration;
                if (config.AutoRead)
                {
                    // stop accept new connections for 1 second to allow the channel to recover
                    // See https://github.com/netty/netty/issues/1328
                    config.AutoRead = false;
                    ctx.Channel.EventLoop.ScheduleAsync(() => { config.AutoRead = true; }, TimeSpan.FromSeconds(1));
                }
                // still let the ExceptionCaught event flow through the pipeline to give the user
                // a chance to do something with it
                ctx.FireExceptionCaught(cause);
            }
        }
    }
}