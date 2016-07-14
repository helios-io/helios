// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using Helios.Util;

namespace Helios.Tracing
{
    /// <summary>
    ///     A <see cref="IHeliosTraceWriter" /> implementation that uses simple <see cref="AtomicCounter" /> instances
    ///     for recording call counts to specific events.
    ///     Counter values can be observed via the <see cref="Counter" /> property.
    /// </summary>
    public sealed class HeliosCounterTraceWriter : IHeliosTraceWriter
    {
        public static HeliosCounterTraceWriter Instance = new HeliosCounterTraceWriter();

        public readonly Counters Counter = new Counters();

        private HeliosCounterTraceWriter()
        {
        }

        public void TcpClientConnectSuccess()
        {
            Counter.TcpClientConnectCounter.GetAndIncrement();
        }

        public void TcpClientConnectFailure(string reason)
        {
            Counter.TcpClientConnectFailureCounter.GetAndIncrement();
        }

        public void TcpClientSend(int payloadLength)
        {
            Counter.TcpClientSendCounter.GetAndIncrement();
        }

        public void TcpClientSendSuccess()
        {
            Counter.TcpClientSendSuccessCounter.GetAndIncrement();
        }

        public void TcpClientSendFailure()
        {
            Counter.TcpClientSendFailureCounter.GetAndIncrement();
        }

        public void TcpClientReceive(int payloadLength)
        {
            Counter.TcpClientReceiveCounter.GetAndIncrement();
        }

        public void TcpClientReceiveSuccess()
        {
            Counter.TcpClientReceiveSuccessCounter.GetAndIncrement();
        }

        public void TcpClientReceiveFailure()
        {
            Counter.TcpClientReceiveFailureCounter.GetAndIncrement();
        }

        public void TcpInboundAcceptSuccess()
        {
            Counter.TcpInboundAcceptSuccessCounter.GetAndIncrement();
        }

        public void TcpInboundAcceptFailure(string reason)
        {
            Counter.TcpInboundAcceptFailureCounter.GetAndIncrement();
        }

        public void TcpInboundClientSend(int payloadLength)
        {
            Counter.TcpInboundClientSendCounter.GetAndIncrement();
        }

        public void TcpClientSendQueued()
        {
            Counter.TcpClientSendQueuedCounter.GetAndIncrement();
        }

        public void TcpInboundSendQueued()
        {
            Counter.TcpInboundSendQueuedCounter.GetAndIncrement();
        }

        public void TcpInboundSendSuccess()
        {
            Counter.TcpInboundSendSuccessCounter.GetAndIncrement();
        }

        public void TcpInboundSendFailure()
        {
            Counter.TcpInboundSendFailureCounter.GetAndIncrement();
        }

        public void TcpInboundReceive(int payloadLength)
        {
            Counter.TcpInboundReceiveCounter.GetAndIncrement();
        }

        public void TcpInboundReceiveSuccess()
        {
            Counter.TcpInboundReceiveSuccessCounter.GetAndIncrement();
        }

        public void TcpInboundReceiveFailure()
        {
            Counter.TcpInboundReceiveFailureCounter.GetAndIncrement();
        }

        public void UdpClientSend(int payloadLength)
        {
            Counter.UdpClientSendCounter.GetAndIncrement();
        }

        public void UdpClientSendSuccess()
        {
            Counter.UdpClientSendSuccessCounter.GetAndIncrement();
        }

        public void UdpClientSendFailure()
        {
            Counter.UdpClientSendFailureCounter.GetAndIncrement();
        }

        public void UdpClientReceive(int payloadLength)
        {
            Counter.UdpClientReceiveCounter.GetAndIncrement();
        }

        public void UdpClientReceiveSuccess()
        {
            Counter.UdpClientReceiveSuccessCounter.GetAndIncrement();
        }

        public void UdpClientReceiveFailure()
        {
            Counter.UdpClientReceiveFailure.GetAndIncrement();
        }

        public void DecodeSucccess(int messageCount)
        {
            Counter.DecodeSuccessCounter.GetAndIncrement();
        }

        public void DecodeFailure()
        {
            Counter.DecodeFailureCounter.GetAndIncrement();
        }

        public void DecodeMalformedBytes(int byteCount)
        {
            Counter.DecodeMalformedBytesCounter.GetAndIncrement();
        }

        public void EncodeSuccess()
        {
            Counter.EncodeSuccessCounter.GetAndIncrement();
        }

        public void EncodeFailure()
        {
            Counter.EncodeFailureCounter.GetAndIncrement();
        }

        public class Counters
        {
            public AtomicCounter DecodeFailureCounter = new AtomicCounter(0);
            public AtomicCounter DecodeMalformedBytesCounter = new AtomicCounter(0);
            public AtomicCounter DecodeSuccessCounter = new AtomicCounter(0);
            public AtomicCounter EncodeFailureCounter = new AtomicCounter(0);
            public AtomicCounter EncodeSuccessCounter = new AtomicCounter(0);
            public AtomicCounter TcpClientConnectCounter = new AtomicCounter(0);
            public AtomicCounter TcpClientConnectFailureCounter = new AtomicCounter(0);
            public AtomicCounter TcpClientReceiveCounter = new AtomicCounter(0);
            public AtomicCounter TcpClientReceiveFailureCounter = new AtomicCounter(0);
            public AtomicCounter TcpClientReceiveSuccessCounter = new AtomicCounter(0);
            public AtomicCounter TcpClientSendCounter = new AtomicCounter(0);
            public AtomicCounter TcpClientSendFailureCounter = new AtomicCounter(0);
            public AtomicCounter TcpClientSendQueuedCounter = new AtomicCounter(0);
            public AtomicCounter TcpClientSendSuccessCounter = new AtomicCounter(0);
            public AtomicCounter TcpInboundAcceptFailureCounter = new AtomicCounter(0);
            public AtomicCounter TcpInboundAcceptSuccessCounter = new AtomicCounter(0);
            public AtomicCounter TcpInboundClientSendCounter = new AtomicCounter(0);
            public AtomicCounter TcpInboundReceiveCounter = new AtomicCounter(0);
            public AtomicCounter TcpInboundReceiveFailureCounter = new AtomicCounter(0);
            public AtomicCounter TcpInboundReceiveSuccessCounter = new AtomicCounter(0);
            public AtomicCounter TcpInboundSendFailureCounter = new AtomicCounter(0);
            public AtomicCounter TcpInboundSendQueuedCounter = new AtomicCounter(0);
            public AtomicCounter TcpInboundSendSuccessCounter = new AtomicCounter(0);
            public AtomicCounter UdpClientReceiveCounter = new AtomicCounter(0);
            public AtomicCounter UdpClientReceiveFailure = new AtomicCounter(0);
            public AtomicCounter UdpClientReceiveSuccessCounter = new AtomicCounter(0);
            public AtomicCounter UdpClientSendCounter = new AtomicCounter(0);
            public AtomicCounter UdpClientSendFailureCounter = new AtomicCounter(0);
            public AtomicCounter UdpClientSendSuccessCounter = new AtomicCounter(0);
        }
    }
}