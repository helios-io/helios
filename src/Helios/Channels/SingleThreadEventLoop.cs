// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Threading.Tasks;
using Helios.Concurrency;

namespace Helios.Channels
{
    /// <summary>
    ///     Single threaded <see cref="IEventLoop" /> implementation built on top of <see cref="SingleThreadEventExecutor" />
    /// </summary>
    public class SingleThreadEventLoop : SingleThreadEventExecutor, IEventLoop
    {
        private static readonly TimeSpan DefaultBreakoutInterval = TimeSpan.FromMilliseconds(100);

        public SingleThreadEventLoop() : this(null, DefaultBreakoutInterval)
        {
        }

        public SingleThreadEventLoop(string threadName) : this(threadName, DefaultBreakoutInterval)
        {
        }

        public SingleThreadEventLoop(string threadName, TimeSpan breakoutInterval) : base(threadName, breakoutInterval)
        {
            Invoker = new DefaultChannelHandlerInvoker(this);
        }

        public IChannelHandlerInvoker Invoker { get; }

        public Task RegisterAsync(IChannel channel)
        {
            return channel.Unsafe.RegisterAsync(this);
        }

        public new IEventLoop Unwrap()
        {
            return this;
        }
    }
}