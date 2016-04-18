using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Helios.Concurrency;

namespace Helios.Channels
{
    public interface IEventLoopGroup
    {
        Task TerminationCompletion { get; }

        IEventLoop GetNext();

        Task ShutdownGracefullyAsync();
    }
}
