using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Helios.Net;
using Helios.Net.Bootstrap;
using Helios.Net.Connections;
using Helios.Topology;

namespace TimeServiceClient
{
    class Program
    {
        public static IConnection TimeServer;

        static void Main(string[] args)
        {
            var host = IPAddress.Loopback;
            var port = 9991;
            var bootstrapper =
                new ClientBootstrap()
                    .SetTransport(TransportType.Tcp).Build();

            TimeServer = bootstrapper.NewConnection(Node.Empty(), NodeBuilder.BuildNode().Host(host).WithPort(port));
            TimeServer.OnConnection += (address, connection) =>
            {
                Console.WriteLine("Confirmed connection with host.");
                connection.BeginReceive(ReceivedCallback);
            };
            TimeServer.OnDisconnection += (address, reason) => Console.WriteLine("Disconnected.");

            Console.Title = string.Format("TimeClient {0}", Process.GetCurrentProcess().Id);
            LoopConnect();
            Console.WriteLine("Requesting time from server...");
            LoopWrite();
            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
        }

        private static void ReceivedCallback(NetworkData data, IConnection responseChannel)
        {
            var timeStr = Encoding.UTF8.GetString(data.Buffer);
            Console.WriteLine("Received: {0}", timeStr);
        }

        static void LoopWrite()
        {
            var command = Encoding.UTF8.GetBytes("gettime");

            while (TimeServer.IsOpen())
            {
                Thread.Sleep(1);
                TimeServer.Send(new NetworkData() { Buffer = command, Length = command.Length });
            }
            Console.WriteLine("Connection closed.");
        }

        static void LoopConnect()
        {
            var attempts = 0;
            while (!TimeServer.IsOpen())
            {
                try
                {
                    attempts++;
                    TimeServer.Open();
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
