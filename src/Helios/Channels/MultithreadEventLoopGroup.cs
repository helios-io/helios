using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Helios.Concurrency;

namespace Helios.Channels
{
    /// <summary>
    /// <see cref="IEventLoopGroup"/> implementation designs for multiplexing multiple active <see cref="IChannel"/>
    /// instances across one or more <see cref="SingleThreadEventLoop"/> instances.
    /// </summary>
    public sealed class MultithreadEventLoopGroup : IEventLoopGroup
    {
        private static readonly int DefaultEventLoopThreadCount = Environment.ProcessorCount * 2;
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
            this._eventLoops = new IEventLoop[eventLoopCount];
            var terminationTasks = new Task[eventLoopCount];
            for (int i = 0; i < eventLoopCount; i++)
            {
                IEventLoop eventLoop;
                bool success = false;
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
                        Task.WhenAll(this._eventLoops
                            .Take(i)
                            .Select(loop => loop.GracefulShutdownAsync()))
                            .Wait();
                    }
                }

                this._eventLoops[i] = eventLoop;
                terminationTasks[i] = eventLoop.TerminationTask;
            }
            this.TerminationCompletion = Task.WhenAll(terminationTasks);
        }

        public Task TerminationCompletion { get; private set; }

        public IEventLoop GetNext()
        {
            int id = Interlocked.Increment(ref this._requestId);
            return this._eventLoops[Math.Abs(id % this._eventLoops.Length)];
        }

        public Task ShutdownGracefullyAsync()
        {
            foreach (IEventLoop eventLoop in this._eventLoops)
            {
                eventLoop.GracefulShutdownAsync();
            }
            return this.TerminationCompletion;
        }
    }
}