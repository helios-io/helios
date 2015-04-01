using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Helios.MultiNodeTests.TestKit;

namespace Helios.Benchmark.TCPThroughput
{
    class Program
    {
        static void Main(string[] args)
        {
            var test = new TcpThroughputHarness();
            Console.WriteLine("Helios TCP Message Throughput Test");
            Console.WriteLine("How quickly can we send messages along the following route?");
            Console.WriteLine("Client Send --> Server Receive --> Server Send --> Client Receive");
             var generations = 3;
            var threadCount = Environment.ProcessorCount;
            for (int i = 0; i < generations; i++)
            {
                var workItems = 10000*(int) Math.Pow(10, i);
                Console.WriteLine("Testing for {0} 200b messages", workItems);
                Console.WriteLine(TimeSpan.FromMilliseconds(
                    Enumerable.Range(0, 6).Select(_ =>
                    {
                        test.SetUp();
                        var sw = Stopwatch.StartNew();
                        test.RunBenchmark(workItems);
                        var elapsed = sw.ElapsedMilliseconds;
                        test.CleanUp();
                        return elapsed;
                    }).Skip(1).Average()));
            }
        }
    }

    /// <summary>
    /// Going to re-use the multi-node testkit for running this benchmark
    /// </summary>
    public class TcpThroughputHarness : MultiNodeTest
    {
        public override TransportType TransportType
        {
            get { return TransportType.Tcp; }
        }

        public override bool HighPerformance
        {
            get { return true; }
        }

        public void RunBenchmark(int messages)
        {
            //arrange
            StartServer(); //uses an "echo server" callback
            StartClient();
            var messageLength = 200;
            var sends = messages;
            var message = new byte[messageLength];
            //act
            for (var i = 0; i < sends; i++)
            {
                Send(message);
            }
            WaitUntilNMessagesReceived(sends, TimeSpan.FromMinutes(3)); //set a really long timeout, just in case

        }
    }
}
