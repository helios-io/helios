using Helios.Tracing;
using Microsoft.Diagnostics.Tracing;

namespace Helios.ETW
{
    [EventSource(Name = "HeliosTrace")]
	public sealed class HeliosEtwTraceWriter : EventSource, IHeliosTraceWriter
    {
        private HeliosEtwTraceWriter() { }

        public static readonly HeliosEtwTraceWriter Instance = new HeliosEtwTraceWriter();

        [Event(1)]
        public void TcpClientConnectSuccess()
        {
            WriteEvent(1);
        }

        [Event(2)]
        public void TcpClientConnectFailure(string reason)
        {
            WriteEvent(2, reason);
        }

        [Event(3)]
        public void TcpClientSend(int payloadLength)
        {
            WriteEvent(3, payloadLength);
        }

        [Event(4)]
        public void TcpClientSendSuccess()
        {
            WriteEvent(4);
        }

        [Event(5)]
        public void TcpClientSendFailure()
        {
            WriteEvent(5);
        }

        [Event(6)]
        public void TcpClientReceive(int payloadLength)
        {
            WriteEvent(6, payloadLength);
        }

        [Event(7)]
        public void TcpClientReceiveSuccess()
        {
            WriteEvent(7);
        }

        [Event(8)]
        public void TcpClientReceiveFailure()
        {
            WriteEvent(8);
        }

        [Event(9)]
        public void TcpInboundAcceptSuccess()
        {
            WriteEvent(9);
        }

        [Event(10)]
        public void TcpInboundAcceptFailure(string reason)
        {
            WriteEvent(10, reason);
        }

        [Event(11)]
        public void TcpInboundClientSend(int payloadLength)
        {
            WriteEvent(11, payloadLength);
        }

        [Event(12)]
        public void TcpInboundSendSuccess()
        {
            WriteEvent(12);
        }

        [Event(13)]
        public void TcpInboundSendFailure()
        {
            WriteEvent(13);
        }

        [Event(14)]
        public void TcpInboundReceive(int payloadLength)
        {
            WriteEvent(14, payloadLength);
        }

        [Event(15)]
        public void TcpInboundReceiveSuccess()
        {
            WriteEvent(15);
        }

        [Event(16)]
        public void TcpInboundReceiveFailure()
        {
            WriteEvent(16);
        }

        [Event(17)]
        public void UdpClientSend(int payloadLength)
        {
            WriteEvent(17);
        }

        [Event(18)]
        public void UdpClientSendSuccess()
        {
            WriteEvent(18);
        }

        [Event(19)]
        public void UdpClientSendFailure()
        {
            WriteEvent(19);
        }

        [Event(20)]
        public void UdpClientReceive(int payloadLength)
        {
            WriteEvent(20);
        }

        [Event(21)]
        public void UdpClientReceiveSuccess()
        {
            WriteEvent(21);
        }

        [Event(22)]
        public void UdpClientReceiveFailure()
        {
            WriteEvent(22);
        }

        [Event(23)]
        public void DecodeSucccess(int messageCount)
        {
            WriteEvent(23, messageCount);
        }

        [Event(24)]
        public void DecodeFailure()
        {
            WriteEvent(24);
        }

        [Event(25)]
        public void DecodeMalformedBytes(int byteCount)
        {
            WriteEvent(25, byteCount);
        }

        [Event(26)]
        public void EncodeSuccess()
        {
            WriteEvent(26);
        }

        [Event(27)]
        public void EncodeFailure()
        {
            WriteEvent(27);
        }

		[Event(28)]
		public void TcpClientSendQueued ()
		{
			WriteEvent (28);
		}

		[Event(29)]
		public void TcpInboundSendQueued ()
		{
			WriteEvent (29);
		}
    }
}
