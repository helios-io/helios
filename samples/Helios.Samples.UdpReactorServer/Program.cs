using System;
using System.Net;
using System.Text;
using Helios.Net;
using Helios.Reactor;
using Helios.Reactor.Udp;
using Helios.Topology;

namespace Helios.Samples.UdpReactorServer
{
    class Program
    {
        private const int DEFAULT_PORT = 1337;

        private static int Port;

        static void ServerPrint(INode node, string message)
        {
            Console.WriteLine("[{0}] {1}:{2}: {3}", DateTime.UtcNow, node.Host, node.Port, message);
        }

        static void Main(string[] args)
        {
            Port = args.Length < 1 ? DEFAULT_PORT : Int32.Parse(args[0]);
            var ip = IPAddress.Any;

            Console.WriteLine("Starting echo server...");
            Console.WriteLine("Will begin listening for requests on {0}:{1}", ip, Port);
            IReactor reactor = new SimpleUdpReactor(ip, Port);
            reactor.OnConnection += node => ServerPrint(node,
                string.Format("Accepting connection from... {0}:{1}", node.Host, node.Port));
            reactor.OnDisconnection += (address, reason) => ServerPrint(address,
                string.Format("Closed connection to... {0}:{1} [Reason:{2}]", address.Host, address.Port, reason.Type));
            reactor.OnReceive += (data, eventArgs) =>
            {
                var connection = eventArgs;
                var node = connection.Node;

                ServerPrint(connection.Node, string.Format("recieved {0} bytes", data.Length));
                var str = Encoding.UTF8.GetString(data.Buffer).Trim();
                if (str.Trim().Equals("close"))
                {
                    connection.Close();
                    return;
                }
                ServerPrint(connection.Node, string.Format("recieved \"{0}\"", str));
                ServerPrint(connection.Node,
                    string.Format("sending \"{0}\" back to {1}:{2}", str, node.Host, node.Port));
                var sendBytes = Encoding.UTF8.GetBytes(str + Environment.NewLine);
                connection.Send(new NetworkData() { Buffer = sendBytes, Length = sendBytes.Length, RemoteHost = node });
            };

            reactor.Start();
            Console.ReadKey();
        }
    }
}
