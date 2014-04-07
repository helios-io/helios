using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Helios.Exceptions;
using Helios.Net;
using Helios.Net.Connections;
using Helios.Topology;

namespace TimeServiceClient
{
    class Program
    {
        public static Socket TimeServer;

        static void Main(string[] args)
        {
            TimeServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //TimeServer.Bind(new IPEndPoint(IPAddress.Any, 1337));
            Console.Title = string.Format("TimeClient {0}", Process.GetCurrentProcess().Id);
            LoopConnect();
            LoopWrite();
            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
        }

        static void LoopWrite()
        {
            var command = Encoding.UTF8.GetBytes("gettime");
            var buffer = new byte[1024];

            while(TimeServer.Connected)
            {
                Thread.Sleep(50);
                TimeServer.Send(command);
                var responseSize = TimeServer.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                var response = new byte[responseSize];
                Array.Copy(buffer, response, responseSize);
                var timeStr = Encoding.UTF8.GetString(response);
                Console.WriteLine("Received: {0}", timeStr);
            }
            Console.WriteLine("Connection closed.");
        }

        static void LoopConnect()
        {
            var attempts = 0;
            while (!TimeServer.Connected)
            {
                try
                {
                    attempts++;
                    TimeServer.Connect(IPAddress.Loopback, 1337);
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
