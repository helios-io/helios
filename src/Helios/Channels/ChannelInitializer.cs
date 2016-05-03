using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helios.Logging;

namespace Helios.Channels
{
    public abstract class ChannelInitializer<T> : ChannelHandlerAdapter
        where T : IChannel
    {
        static readonly ILogger Logger = LoggingFactory.GetLogger<ChannelInitializer<T>>();

        /// <summary>
        /// This method will be called once the {@link Channel} was registered. After the method returns this instance
        /// will be removed from the {@link ChannelPipeline} of the {@link Channel}.
        ///
        /// @param channel            the {@link Channel} which was registered.
        /// @throws Exception    is thrown if an error occurs. In that case the {@link Channel} will be closed.
        /// </summary>
        protected abstract void InitChannel(T channel);

        public sealed override void ChannelRegistered(IChannelHandlerContext context)
        {
            IChannelPipeline pipeline = context.Channel.Pipeline;
            bool success = false;
            try
            {
                this.InitChannel((T)context.Channel);
                pipeline.Remove(this);
                context.FireChannelRegistered();
                success = true;
            }
            catch (Exception ex)
            {
                Logger.Warning("Failed to initialize a channel. Closing: " + context.Channel + Environment.NewLine + "Cause: {0}", ex);
            }
            finally
            {
                if (pipeline.Context(this) != null)
                {
                    pipeline.Remove(this);
                }
                if (!success)
                {
                    context.CloseAsync();
                }
            }
        }
    }
}
