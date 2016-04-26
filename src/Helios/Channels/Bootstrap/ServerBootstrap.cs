using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Helios.Logging;

namespace Helios.Channels.Bootstrap
{
    /// <summary>
    /// {@link Bootstrap} sub-class which allows easy bootstrap of {@link ServerChannel}
    ///
    /// </summary>
    public class ServerBootstrap : AbstractBootstrap<ServerBootstrap, IServerChannel>
    {
        static readonly ILogger Logger = LoggingFactory.GetLogger<ServerBootstrap>();

        readonly ConcurrentDictionary<ChannelOption, object> _childOptions;

        volatile IEventLoopGroup _childGroup;
        volatile IChannelHandler _childHandler;

        public ServerBootstrap()
        {
            this._childOptions = new ConcurrentDictionary<ChannelOption, object>();
        }

        ServerBootstrap(ServerBootstrap bootstrap)
            : base(bootstrap)
        {
            this._childGroup = bootstrap._childGroup;
            this._childHandler = bootstrap._childHandler;
            this._childOptions = new ConcurrentDictionary<ChannelOption, object>(bootstrap._childOptions);
        }

        /// <summary>
        /// Specify the {@link EventLoopGroup} which is used for the parent (acceptor) and the child (client).
        /// </summary>
        public override ServerBootstrap Group(IEventLoopGroup group)
        {
            return this.Group(group, group);
        }

        /// <summary>
        /// Set the {@link EventLoopGroup} for the parent (acceptor) and the child (client). These
        /// {@link EventLoopGroup}'s are used to handle all the events and IO for {@link ServerChannel} and
        /// {@link Channel}'s.
        /// </summary>
        public ServerBootstrap Group(IEventLoopGroup parentGroup, IEventLoopGroup childGroup)
        {
            Contract.Requires(childGroup != null);

            base.Group(parentGroup);
            if (this._childGroup != null)
            {
                throw new InvalidOperationException("childGroup set already");
            }
            this._childGroup = childGroup;
            return this;
        }

        /// <summary>
        /// Allow to specify a {@link ChannelOption} which is used for the {@link Channel} instances once they get created
        /// (after the acceptor accepted the {@link Channel}). Use a value of {@code null} to remove a previous set
        /// {@link ChannelOption}.
        /// </summary>
        public ServerBootstrap ChildOption<T>(ChannelOption<T> childOption, T value)
        {
            Contract.Requires(childOption != null);

            if (value == null)
            {
                object removed;
                this._childOptions.TryRemove(childOption, out removed);
            }
            else
            {
                this._childOptions[childOption] = value;
            }
            return this;
        }

        public ServerBootstrap ChildHandler(IChannelHandler childHandler)
        {
            Contract.Requires(childHandler != null);

            this._childHandler = childHandler;
            return this;
        }

        /// <summary>
        /// Return the configured {@link EventLoopGroup} which will be used for the child channels or {@code null}
        /// if non is configured yet.
        /// </summary>
        public IEventLoopGroup ChildGroup()
        {
            return this._childGroup;
        }

        protected override void Init(IChannel channel)
        {
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

            IChannelPipeline p = channel.Pipeline;
            if (this.Handler() != null)
            {
                p.AddLast(this.Handler());
            }

            IEventLoopGroup currentChildGroup = this._childGroup;
            IChannelHandler currentChildHandler = this._childHandler;
            KeyValuePair<ChannelOption, object>[] currentChildOptions = this._childOptions.ToArray();

            Func<IChannelConfiguration, bool> childConfigSetupFunc = CompileOptionsSetupFunc(this._childOptions);

            p.AddLast(new ActionChannelInitializer<IChannel>(ch =>
            {
                ch.Pipeline.AddLast(new ServerBootstrapAcceptor(currentChildGroup, currentChildHandler,
                    childConfigSetupFunc /*, currentChildAttrs*/));
            }));
        }

        public override ServerBootstrap Validate()
        {
            base.Validate();
            if (this._childHandler == null)
            {
                throw new InvalidOperationException("childHandler not set");
            }
            if (this._childGroup == null)
            {
                Logger.Warning("childGroup is not set. Using parentGroup instead.");
                this._childGroup = this.Group();
            }
            return this;
        }

        static Func<IChannelConfiguration, bool> CompileOptionsSetupFunc(IDictionary<ChannelOption, object> templateOptions)
        {
            if (templateOptions.Count == 0)
            {
                return null;
            }

            ParameterExpression configParam = Expression.Parameter(typeof(IChannelConfiguration));
            ParameterExpression resultVariable = Expression.Variable(typeof(bool));
            var assignments = new List<Expression>
            {
                Expression.Assign(resultVariable, Expression.Constant(true))
            };

            MethodInfo setOptionMethodDefinition = typeof(IChannelConfiguration)
                .FindMembers(MemberTypes.Method, BindingFlags.Instance | BindingFlags.Public, Type.FilterName, "SetOption")
                .Cast<MethodInfo>()
                .First(x => x.IsGenericMethodDefinition);

            foreach (KeyValuePair<ChannelOption, object> p in templateOptions)
            {
                // todo: emit log if verbose is enabled && option is missing
                Type optionType = p.Key.GetType();
                if (!optionType.IsGenericType)
                {
                    throw new InvalidOperationException("Only options of type ChannelOption<T> are supported.");
                }
                if (optionType.GetGenericTypeDefinition() != typeof(ChannelOption<>))
                {
                    throw new NotSupportedException(string.Format("Channel option is of an unsupported type `{0}`. Only ChannelOption and ChannelOption<T> are supported.", optionType));
                }
                Type valueType = optionType.GetGenericArguments()[0];
                MethodInfo setOptionMethod = setOptionMethodDefinition.MakeGenericMethod(valueType);
                assignments.Add(Expression.Assign(
                    resultVariable,
                    Expression.AndAlso(
                        resultVariable,
                        Expression.Call(configParam, setOptionMethod, Expression.Constant(p.Key), Expression.Constant(p.Value, valueType)))));
            }

            return Expression.Lambda<Func<IChannelConfiguration, bool>>(Expression.Block(typeof(bool), new[] { resultVariable }, assignments), configParam).Compile();
        }

        class ServerBootstrapAcceptor : ChannelHandlerAdapter
        {
            readonly IEventLoopGroup childGroup;
            readonly IChannelHandler childHandler;
            readonly Func<IChannelConfiguration, bool> childOptionsSetupFunc;

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
                var child = (IChannel)msg;

                child.Pipeline.AddLast(this.childHandler);

                if (this.childOptionsSetupFunc != null)
                {
                    if (!this.childOptionsSetupFunc(child.Configuration))
                    {
                        Logger.Warning("Not all configuration options could be set.");
                    }
                }

                // todo: async/await instead?
                try
                {
                    this.childGroup.GetNext().RegisterAsync(child).ContinueWith(future => ForceClose(child, future.Exception),
                        TaskContinuationOptions.NotOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);
                }
                catch (Exception ex)
                {
                    ForceClose(child, ex);
                }
            }

            static void ForceClose(IChannel child, Exception ex)
            {
                child.Unsafe.CloseForcibly();
                Logger.Warning("Failed to register an accepted channel: " + child, ex);
            }

            public override void ExceptionCaught(IChannelHandlerContext ctx, Exception cause)
            {
                IChannelConfiguration config = ctx.Channel.Configuration;
                if (config.AutoRead)
                {
                    // stop accept new connections for 1 second to allow the channel to recover
                    // See https://github.com/netty/netty/issues/1328
                    config.AutoRead = false;

                    //todo: scheduling 
                    ctx.Channel.EventLoop.Execute(() => { config.AutoRead = true; });
                }
                // still let the ExceptionCaught event flow through the pipeline to give the user
                // a chance to do something with it
                ctx.FireExceptionCaught(cause);
            }
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
            if (this._childGroup != null)
            {
                buf.Append("childGroup: ")
                    .Append(this._childGroup.GetType().Name)
                    .Append(", ");
            }
            buf.Append("childOptions: ")
                .Append(this._childOptions)
                .Append(", ");

            if (this._childHandler != null)
            {
                buf.Append("childHandler: ");
                buf.Append(this._childHandler);
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
    }
}
