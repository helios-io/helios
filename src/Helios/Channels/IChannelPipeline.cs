using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Channels
{
    public interface IChannelPipeline : IEnumerable<IChannelHandler>
    {
        IChannelPipeline AddFirst(string name, IChannelHandler handler);

        IChannelPipeline AddFirst(IChannelHandlerInvoker invoker, string name, IChannelHandler handler);

        IChannelPipeline AddLast(string name, IChannelHandler handler);

        IChannelPipeline AddLast(IChannelHandlerInvoker invoker, string name, IChannelHandler handler);

        IChannelPipeline AddFirst(params IChannelHandler[] handlers);

        IChannelPipeline AddFirst(IChannelHandlerInvoker invoker, params IChannelHandler[] handlers);

        IChannelPipeline AddLast(params IChannelHandler[] handlers);

        IChannelPipeline AddLast(IChannelHandlerInvoker invoker, params IChannelHandler[] handlers);

        IChannelPipeline Remove(IChannelHandler handler);

        IChannelHandler Remove(string name);

        T Remove<T>() where T : class, IChannelHandler;

        IChannelHandler RemoveFirst();

        IChannelHandler RemoveLast();

        IChannelHandler First();

        IChannelHandlerContext FirstContext();

        IChannelHandler Last();

        IChannelHandlerContext LastContext();

        IChannelHandler Get(string name);

        T Get<T>() where T : class, IChannelHandler;

        IChannelHandlerContext Context(IChannelHandler handler);

        IChannelHandlerContext Context(string name);

        IChannelHandlerContext Context<T>() where T : class, IChannelHandler;

        IChannel Channel();

        IChannelPipeline FireChannelRegistered();

        IChannelPipeline FireChannelUnregistered();

        IChannelPipeline FireChannelActive();

        IChannelPipeline FireChannelInactive();

        IChannelPipeline FireExceptionCaught(Exception cause);

        IChannelPipeline FireUserEventTriggered(object evt);

        IChannelPipeline FireChannelRead(object msg);

        IChannelPipeline FireChannelReadComplete();

        IChannelPipeline FireChannelWritabilityChanged();

        Task BindAsync(EndPoint localAddress);

        Task ConnectAsync(EndPoint remoteAddress);

        Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress);

        Task DisconnectAsync();

        Task CloseAsync();

        Task DeregisterAsync();

        IChannelPipeline Read();

        Task WriteAsync(object msg);

        IChannelPipeline Flush();

        Task WriteAndFlushAsync(object msg);
    }
}
