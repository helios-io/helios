using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using Helios.Core.Net.Exceptions;
using Helios.Core.Reactor;
using Helios.Core.Topology;

namespace Helios.Samples.TcpReactorServer
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
            IReactor reactor = new TcpReactor(ip, Port);
            reactor.AcceptConnection += (sender, eventArgs) =>
            {
                var connection = eventArgs.Connection;
                var node = connection.Node;
                var bytes = new byte[1024];
                int read;
                ServerPrint(connection.Node,
                        string.Format("Accepting connection from... {0}:{1}", node.Host, node.Port));
                do
                {
                    try
                    {
                        read = connection.Read(bytes, 0, bytes.Length);
                        ServerPrint(connection.Node, string.Format("recieved {0} bytes", read));
                        var str = Encoding.UTF8.GetString(bytes).Trim();
                        if (str.Trim().Equals("close"))
                            break;
                        ServerPrint(connection.Node, string.Format("recieved \"{0}\"", str));
                        ServerPrint(connection.Node,
                            string.Format("sending \"{0}\" back to {1}:{2}", str, node.Host, node.Port));
                        var sendBytes = Encoding.UTF8.GetBytes(str);
                        connection.Write(sendBytes);
                        connection.Flush();
                    }
                    catch (HeliosConnectionException ex)
                    {
                        ServerPrint(connection.Node, string.Format("{0} - {1}", ex.Type, ex.Message));
                        break;
                    }
                    catch (SocketException ex)
                    {
                        ServerPrint(connection.Node, string.Format("Error! {0}", ex.Message));
                        break;
                    }
                    bytes = new byte[1024];
                   
                } while (read > 0);
                ServerPrint(connection.Node,
                        string.Format("Closing connection to... {0}:{1}", node.Host, node.Port));
                connection.Close();
            };

            reactor.Start();
        }
    }
}
