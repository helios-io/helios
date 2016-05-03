using System;
using System.Threading.Tasks;
using Helios.Channels;

namespace Helios.Tests.Channels.Bootstrap
{
    public class BootstrapSpecs : IDisposable
    {
        private readonly IEventLoopGroup groupA = new MultithreadEventLoopGroup(1);
        private readonly IEventLoopGroup groupB = new MultithreadEventLoopGroup(1);


        
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            groupA.ShutdownGracefullyAsync();
            groupB.ShutdownGracefullyAsync();
            Task.WaitAll(groupA.TerminationCompletion, groupB.TerminationCompletion);
        }
    }
}
