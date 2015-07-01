using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Helios.Net.Bootstrap;
using Helios.Serialization;
using Helios.Topology;

namespace Helios.Samples.UdpPerformanceTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("We're going to write a ton of data to the console. 100k iterations.");
            Console.WriteLine("Going!");
            
            var remote = Node.Loopback(11010);
            var client =
                new ClientBootstrap().SetTransport(TransportType.Udp)
                    .SetEncoder(new NoOpEncoder())
                    .SetDecoder(new NoOpDecoder()).Build().NewConnection(Node.Any(), remote);
            client.Open();
            var bytes = Encoding.UTF8.GetBytes("THIS IS OUR TEST PAYLOAD");
            
            var stopwatch = Stopwatch.StartNew();
            var i = 0;
            while (i < 100000)
            {
                client.Send(bytes, 0, bytes.Length, remote);
                i++;
            }
            Console.WriteLine("Done queuing messages... waiting for queue to drain");
            while (client.MessagesInSendQueue > 0)
            {
                Thread.Sleep(10);
            }
            Console.WriteLine("Done, press any key to exit");
            stopwatch.Stop();
            Console.WriteLine("Took {0} seconds to complete", stopwatch.Elapsed.TotalSeconds);
            Console.ReadKey();
        }
    }
}
