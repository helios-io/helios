using System;
using System.Net;
using System.Text;
using Helios.Net;
using Helios.Ops;
using Helios.Ops.Executors;
using Helios.Reactor.Tcp;
using Helios.Topology;

namespace TimeServiceServer
{
    public class TimeServer : HighPerformanceTcpReactor
    {
        private readonly IEventLoop _eventLoop;

        public TimeServer(IPAddress localAddress, int localPort, IEventLoop eventLoop, int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE) : base(localAddress, localPort, bufferSize)
        {
            _eventLoop = eventLoop;
        }

        protected override void NodeConnected(INode node)
        {
            _eventLoop.Execute(() => Console.WriteLine("Connected: {0}", node));
        }

        protected override void NodeDisconnected(INode node)
        {
            _eventLoop.Execute(() => Console.WriteLine("Disconnected: {0}", node));
        }

        protected override void EventLoop(NetworkData availableData)
        {
            _eventLoop.Execute(() =>
            {
                var command = Encoding.UTF8.GetString(availableData.Buffer);
                Console.WriteLine("Received: {0}", command);
                if (command.ToLowerInvariant() == "gettime")
                {
                    var time = Encoding.UTF8.GetBytes(DateTime.Now.ToLongTimeString());
                    Send(time, availableData.RemoteHost);
                    Console.WriteLine("Sent time to {0}", availableData.RemoteHost);
                }
                else
                {
                    Console.WriteLine("Invalid command");
                }
            });
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Server";
            Console.WriteLine("Starting server on {0}:{1}", IPAddress.Any, 1337);
            var eventLoop = new ThreadedEventLoop(new TryCatchExecutor(exception => Console.WriteLine("Unhandled exception: {0}", exception)),2);
            var server = new TimeServer(IPAddress.Any, 1337, eventLoop);
            server.Start();
            Console.WriteLine("Running, press any key to exit");
            Console.ReadKey();
            Console.WriteLine("Shutting down...");
            server.Stop();
            Console.WriteLine("Terminated");
        }
    }
}
