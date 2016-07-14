// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Helios.Concurrency;

namespace Helios.Channels
{
    /// <summary>
    ///     <see cref="IEventLoopGroup" /> implementation designs for multiplexing multiple active <see cref="IChannel" />
    ///     instances across one or more <see cref="SingleThreadEventLoop" /> instances.
    /// </summary>
    public sealed class MultithreadEventLoopGroup : IEventLoopGroup
    {
        private static readonly int DefaultEventLoopThreadCount = Environment.ProcessorCount*2;
        private static readonly Func<IEventLoop> DefaultEventLoopFactory = () => new SingleThreadEventLoop();

        private readonly IEventLoop[] _eventLoops;
        private int _requestId;

        public MultithreadEventLoopGroup()
            : this(DefaultEventLoopFactory, DefaultEventLoopThreadCount)
        {
        }

        public MultithreadEventLoopGroup(int eventLoopCount)
            : this(DefaultEventLoopFactory, eventLoopCount)
        {
        }

        public MultithreadEventLoopGroup(Func<IEventLoop> eventLoopFactory)
            : this(eventLoopFactory, DefaultEventLoopThreadCount)
        {
        }

        public MultithreadEventLoopGroup(Func<IEventLoop> eventLoopFactory, int eventLoopCount)
        {
            _eventLoops = new IEventLoop[eventLoopCount];
            var terminationTasks = new Task[eventLoopCount];
            for (var i = 0; i < eventLoopCount; i++)
            {
                IEventLoop eventLoop;
                var success = false;
                try
                {
                    eventLoop = eventLoopFactory();
                    success = true;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("failed to create a child event loop.", ex);
                }
                finally
                {
                    if (!success)
                    {
                        Task.WhenAll(_eventLoops
                            .Take(i)
                            .Select(loop => loop.GracefulShutdownAsync()))
                            .Wait();
                    }
                }

                _eventLoops[i] = eventLoop;
                terminationTasks[i] = eventLoop.TerminationTask;
            }
            TerminationCompletion = Task.WhenAll(terminationTasks);
        }

        public Task TerminationCompletion { get; }

        public IEventLoop GetNext()
        {
            var id = Interlocked.Increment(ref _requestId);
            return _eventLoops[Math.Abs(id%_eventLoops.Length)];
        }

        public Task ShutdownGracefullyAsync()
        {
            foreach (var eventLoop in _eventLoops)
            {
                eventLoop.GracefulShutdownAsync();
            }
            return TerminationCompletion;
        }
    }
}