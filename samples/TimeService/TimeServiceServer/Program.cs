using System;
using System.Net;
using System.Text;
using Helios.Net;
using Helios.Ops.Executors;
using Helios.Reactor.Tcp;

namespace TimeServiceServer
{   
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Server";
            Console.WriteLine("Starting server on {0}:{1}", IPAddress.Any, 1337);
            var eventLoop = new ThreadedEventLoop(new TryCatchExecutor(exception => Console.WriteLine("Unhandled exception: {0}", exception)),2);
            var server = new HighPerformanceTcpReactor(IPAddress.Any, 1337);
            server.OnConnection += address => eventLoop.Execute(() => Console.WriteLine("Connected: {0}", address));
            server.OnDisconnection += (address, reason) => eventLoop.Execute(() => Console.WriteLine("Disconnected: {0}; Reason: {1}", address, reason.Type));
            server.OnReceive += (data, channel) => eventLoop.Execute(() =>
            {
                var command = Encoding.UTF8.GetString(data.Buffer);
                //Console.WriteLine("Received: {0}", command);
                if (command.ToLowerInvariant() == "gettime")
                {
                    var time = Encoding.UTF8.GetBytes(DateTime.Now.ToLongTimeString());
                    channel.Send(new NetworkData(){Buffer= time, Length = time.Length, RemoteHost = channel.RemoteHost});
                    //Console.WriteLine("Sent time to {0}", channel.Node);
                }
                else
                {
                    Console.WriteLine("Invalid command: {0}", command);
                    var invalid = Encoding.UTF8.GetBytes("Unrecognized command");
                    channel.Send(new NetworkData() { Buffer = invalid, Length = invalid.Length, RemoteHost = channel.RemoteHost });
                }
            });
            server.Start();
            Console.WriteLine("Running, press any key to exit");
            Console.ReadKey();
            Console.WriteLine("Shutting down...");
            server.Stop();
            Console.WriteLine("Terminated");
        }
    }
}
