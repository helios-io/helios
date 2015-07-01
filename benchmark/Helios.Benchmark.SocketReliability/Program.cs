using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Helios.MultiNodeTests.TestKit;
using Helios.Tracing;

namespace Helios.Benchmark.TCPThroughput
{
    class Program
    {
        static void Main(string[] args)
        {
            HeliosTrace.SetWriter(HeliosCounterTraceWriter.Instance);
            var harness = new TcpHarness();
            harness.SetUp();
            Console.WriteLine("Helios TCP Client --> Server Reliability benchmark");
            Console.WriteLine("TCP is a reliable protocol, so this should never be a problem. Buuuuuuuuut concurrent programming.");
            Console.WriteLine("Testing delivery rate of {0} messages round trip", harness.BufferSize);
            Console.WriteLine("Client.Write --> Server.Receive --> Server.Write --> Client.Receive");
            Console.WriteLine("200b payload size");
            Console.WriteLine();
            Console.WriteLine("--------------- GO ---------------");
            var sw = Stopwatch.StartNew();
            harness.RunBenchmark();
            sw.Stop();
            Console.WriteLine("Trips completed in {0} ms", sw.ElapsedMilliseconds);
            harness.CleanUp();
			var counters = HeliosCounterTraceWriter.Instance.Counter;
            Console.WriteLine("Checking counters");
        }
    }

    /// <summary>
    /// Going to re-use the multi-node testkit for running this benchmark
    /// </summary>
    public class TcpHarness : MultiNodeTest
    {
        public override TransportType TransportType
        {
            get { return TransportType.Tcp; }
        }

        public override int BufferSize
        {
            get { return 100000; }
        }

        public void RunBenchmark()
        {
            //arrange
            StartServer(); //uses an "echo server" callback
            StartClient();
            var messageLength = 200;
            var sends = BufferSize;

            Console.WriteLine("Initial queue sizes...");
            Console.WriteLine("Client Sends. Expected: {0} / Actual: {1}", 0, ClientSendBuffer.Count);
            Console.WriteLine("Client Receives. Expected: {0} / Actual: {1}", 0, ClientReceiveBuffer.Count);
            Console.WriteLine("Server Receives. Expected: {0} / Acutal: {1}", 0, ServerReceiveBuffer.Count);

            //act
            for (var i = 0; i < sends; i++)
            {
                Send(new byte[messageLength]);
            }
            WaitUntilNMessagesReceived(sends);

            //assert
            Console.WriteLine("ClientExceptions: {0}", ClientExceptions.Length);
            foreach (var exception in ClientExceptions)
            {
                Console.WriteLine(exception.Message);
                Console.WriteLine(exception.StackTrace);
            }

            Console.WriteLine("ServerExceptions: {0}", ServerExceptions.Length);
            foreach (var exception in ServerExceptions)
            {
                Console.WriteLine(exception.Message);
                Console.WriteLine(exception.StackTrace);
            }

            Console.WriteLine("Client Sends. Expected: {0} / Actual: {1}", sends, ClientSendBuffer.Count);
            Console.WriteLine("Client Receives. Expected: {0} / Actual: {1}", sends, ClientReceiveBuffer.Count);
            Console.WriteLine("Server Receives. Expected: {0} / Acutal: {1}", sends, ServerReceiveBuffer.Count);
        }
    }
}
