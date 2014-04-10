using System;
using System.Net;
using System.Text;
using Helios.Net;
using Helios.Reactor;
using Helios.Reactor.Bootstrap;
using Helios.Reactor.Tcp;
using Helios.Topology;

namespace Helios.Samples.TcpReactorServer
{
    internal class Program
    {
        private const int DEFAULT_PORT = 1337;

        private static int Port;

        private static void ServerPrint(INode node, string message)
        {
            Console.WriteLine("[{0}] {1}:{2}: {3}", DateTime.UtcNow, node.Host, node.Port, message);
        }

        private static void Main(string[] args)
        {
            Port = args.Length < 1 ? DEFAULT_PORT : Int32.Parse(args[0]);
            var ip = IPAddress.Any;

            Console.WriteLine("Starting echo server...");
            Console.WriteLine("Will begin listening for requests on {0}:{1}", ip, Port);
            var bootstrapper =
                new ServerBootstrap()
                    .WorkerThreads(2)
                    .SetTransport(TransportType.Tcp)
                    .Build();
            var reactor = bootstrapper.NewReactor(NodeBuilder.BuildNode().Host(ip).WithPort(Port));
            reactor.OnConnection += (node, channel) =>
            {
                ServerPrint(node,
                    string.Format("Accepting connection from... {0}:{1}", node.Host, node.Port));
                channel.BeginReceive(ReceiveData);
            };
            reactor.OnDisconnection += (reason, address) => ServerPrint(address.RemoteHost,
                string.Format("Closed connection to... {0}:{1} [Reason:{2}]", address.RemoteHost.Host, address.RemoteHost.Port, reason.Type));
            reactor.Start();
            Console.ReadKey();
        }

        public static void ReceiveData(NetworkData data, IConnection connection)
        {
            var node = connection.RemoteHost;

            ServerPrint(connection.RemoteHost, string.Format("recieved {0} bytes", data.Length));
            var str = Encoding.UTF8.GetString(data.Buffer).Trim();
            if (str.Trim().Equals("close"))
            {
                connection.Close();
                return;
            }
            ServerPrint(connection.RemoteHost, string.Format("recieved \"{0}\"", str));
            ServerPrint(connection.RemoteHost,
                string.Format("sending \"{0}\" back to {1}:{2}", str, node.Host, node.Port));
            var sendBytes = Encoding.UTF8.GetBytes(str + Environment.NewLine);
            connection.Send(new NetworkData() {Buffer = sendBytes, Length = sendBytes.Length, RemoteHost = node});
        }
    }
}
