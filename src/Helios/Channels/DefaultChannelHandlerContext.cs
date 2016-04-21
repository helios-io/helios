using System.Diagnostics.Contracts;

namespace Helios.Channels
{
    internal sealed class DefaultChannelHandlerContext : AbstractChannelHandlerContext
    {
        public DefaultChannelHandlerContext(IChannelPipeline pipeline, IChannelHandlerInvoker invoker, string name,
            IChannelHandler handler)
            : base(pipeline, invoker, name, GetSkipPropagationFlags(handler))
        {
            Contract.Requires(handler != null);
            Handler = handler;
        }

        public override IChannelHandler Handler { get; }
    }
}