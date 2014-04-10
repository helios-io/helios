using System;
using System.Linq;
using System.Net;
using System.Text;
using Helios.Net;
using Helios.Ops.Executors;
using Helios.Reactor.Bootstrap;
using Helios.Topology;

namespace TimeServiceServer
{   
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Server";
            Console.WriteLine("Starting server on {0}:{1}", IPAddress.Any, 1337);
            var executor = new TryCatchExecutor(exception => Console.WriteLine("Unhandled exception: {0}", exception));

            var bootstrapper =
                new ServerBootstrap()
                    .WorkerThreads(2)
                    .Executor(executor)
                    .SetTransport(TransportType.Tcp)
                    .Build();
            var server = bootstrapper.NewReactor(NodeBuilder.BuildNode().Host(IPAddress.Any).WithPort(1337));
            server.OnConnection += (address, connection) =>
            {
                Console.WriteLine("Connected: {0}", address);
                connection.BeginReceive(Receive);
            };
            server.OnDisconnection += (address, reason) => 
                Console.WriteLine("Disconnected: {0}; Reason: {1}", address, reason.Type);
            server.Start();
            Console.WriteLine("Running, press any key to exit");
            Console.ReadKey();
            Console.WriteLine("Shutting down...");
            server.Stop();
            Console.WriteLine("Terminated");
        }

        public static void Receive(NetworkData data, IConnection channel)
        {
            var rawCommand = Encoding.UTF8.GetString(data.Buffer);
            var commands = rawCommand.Split('|').Where(x => !string.IsNullOrWhiteSpace(x)); //we use the pipe to separate commands
            foreach (var command in commands)
            {
                //Console.WriteLine("Received: {0}", command);
                if (command.ToLowerInvariant() == "gettime")
                {
                    var time = Encoding.UTF8.GetBytes(DateTime.Now.ToLongTimeString());
                    channel.Send(new NetworkData() { Buffer = time, Length = time.Length, RemoteHost = channel.RemoteHost });
                    //Console.WriteLine("Sent time to {0}", channel.Node);
                }
                else
                {
                    Console.WriteLine("Invalid command: {0}", command);
                    var invalid = Encoding.UTF8.GetBytes("Unrecognized command");
                    channel.Send(new NetworkData() { Buffer = invalid, Length = invalid.Length, RemoteHost = channel.RemoteHost });
                }
            }
        }
    }
}
