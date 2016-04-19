using System.Threading.Tasks;
using Helios.Channels;

namespace Helios.Concurrency
{
    public interface IEventLoop : IEventExecutor
    {
        IChannelHandlerInvoker Invoker { get; }

        Task RegisterAsync(IChannel channel);

        new IEventLoop Unwrap();
    }

    public interface IChannelHandlerInvoker
    {
    }
}
