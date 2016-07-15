// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using Helios.Logging;

namespace Helios.Channels
{
    public abstract class ChannelInitializer<T> : ChannelHandlerAdapter
        where T : IChannel
    {
        private static readonly ILogger Logger = LoggingFactory.GetLogger<ChannelInitializer<T>>();

        /// <summary>
        ///     This method will be called once the {@link Channel} was registered. After the method returns this instance
        ///     will be removed from the {@link ChannelPipeline} of the {@link Channel}.
        ///     @param channel            the {@link Channel} which was registered.
        ///     @throws Exception    is thrown if an error occurs. In that case the {@link Channel} will be closed.
        /// </summary>
        protected abstract void InitChannel(T channel);

        public sealed override void ChannelRegistered(IChannelHandlerContext context)
        {
            var pipeline = context.Channel.Pipeline;
            var success = false;
            try
            {
                InitChannel((T) context.Channel);
                pipeline.Remove(this);
                context.FireChannelRegistered();
                success = true;
            }
            catch (Exception ex)
            {
                Logger.Warning(
                    "Failed to initialize a channel. Closing: " + context.Channel + Environment.NewLine + "Cause: {0}",
                    ex);
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