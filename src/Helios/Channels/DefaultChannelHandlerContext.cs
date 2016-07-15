// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Diagnostics.Contracts;

namespace Helios.Channels
{
    internal sealed class DefaultChannelHandlerContext : AbstractChannelHandlerContext
    {
        public DefaultChannelHandlerContext(DefaultChannelPipeline pipeline, IChannelHandlerInvoker invoker, string name,
            IChannelHandler handler)
            : base(pipeline, invoker, name, GetSkipPropagationFlags(handler))
        {
            Contract.Requires(handler != null);
            Handler = handler;
        }

        public override IChannelHandler Handler { get; }
    }
}