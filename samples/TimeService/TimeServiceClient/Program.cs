using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Helios.Concurrency;
using Helios.Net;
using Helios.Net.Bootstrap;
using Helios.Net.Connections;
using Helios.Topology;
using Helios.Util;

namespace TimeServiceClient
{
    class Program
    {
        public static IConnection TimeClient;

        static void Main(string[] args)
        {
            var host = IPAddress.Loopback;
            var port = 9991;
            var bootstrapper =
                new ClientBootstrap()
                    .SetTransport(TransportType.Tcp).Build();

            TimeClient = bootstrapper.NewConnection(Node.Empty(), NodeBuilder.BuildNode().Host(host).WithPort(port));
            TimeClient.OnConnection += (address, connection) =>
            {
                Console.WriteLine("Confirmed connection with host.");
                connection.BeginReceive(ReceivedCallback);
            };
            TimeClient.OnDisconnection += (address, reason) => Console.WriteLine("Disconnected.");

            Console.Title = string.Format("TimeClient {0}", Process.GetCurrentProcess().Id);
            LoopConnect();
            Console.WriteLine("Requesting time from server...");
            Console.WriteLine("Printing every 1/1000 received messages");
            LoopWrite();
            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
        }

        private static void ReceivedCallback(NetworkData data, IConnection responseChannel)
        {
            var timeStr = Encoding.UTF8.GetString(data.Buffer);
            if (ThreadLocalRandom.Current.Next(0, 1000) == 1)
            {
                Console.WriteLine("Received: {0}", timeStr);
            }
        }

        static void LoopWrite()
        {
            var command = Encoding.UTF8.GetBytes("gettime");
            var fiber = FiberFactory.CreateFiber(3);

            Action dedicatedMethod = () =>
            {
                Thread.Sleep(1);
                TimeClient.Send(new NetworkData() {Buffer = command, Length = command.Length});
            };

            while (TimeClient.IsOpen())
            {
                fiber.Add(dedicatedMethod);
            }
            Console.WriteLine("Connection closed.");
            fiber.GracefulShutdown(TimeSpan.FromSeconds(1));
        }

        static void LoopConnect()
        {
            var attempts = 0;
            while (!TimeClient.IsOpen())
            {
                try
                {
                    attempts++;
                    TimeClient.Open();
                }
                catch (SocketException ex)
                {
                    Console.WriteLine("Connection attempt {0}", attempts);
                    if (attempts > 5) throw;
                }
            }
            Console.Clear();
            Console.WriteLine("Connected");
        }
    }
}
