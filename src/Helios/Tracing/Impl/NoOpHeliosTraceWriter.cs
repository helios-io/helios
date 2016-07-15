// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

namespace Helios.Tracing.Impl
{
    /// <summary>
    ///     The default <see cref="IHeliosTraceWriter" /> implementation. Doesn't do anything.
    /// </summary>
    internal class NoOpHeliosTraceWriter : IHeliosTraceWriter
    {
        public void TcpClientConnectSuccess()
        {
        }

        public void TcpClientConnectFailure(string reason)
        {
        }

        public void TcpClientSend(int payloadLength)
        {
        }

        public void TcpClientSendSuccess()
        {
        }

        public void TcpClientSendFailure()
        {
        }

        public void TcpClientReceive(int payloadLength)
        {
        }

        public void TcpClientReceiveSuccess()
        {
        }

        public void TcpClientReceiveFailure()
        {
        }

        public void TcpInboundAcceptSuccess()
        {
        }

        public void TcpClientSendQueued()
        {
        }

        public void TcpInboundSendQueued()
        {
        }

        public void TcpInboundAcceptFailure(string reason)
        {
        }

        public void TcpInboundClientSend(int payloadLength)
        {
        }

        public void TcpInboundSendSuccess()
        {
        }

        public void TcpInboundSendFailure()
        {
        }

        public void TcpInboundReceive(int payloadLength)
        {
        }

        public void TcpInboundReceiveSuccess()
        {
        }

        public void TcpInboundReceiveFailure()
        {
        }

        public void UdpClientSend(int payloadLength)
        {
        }

        public void UdpClientSendSuccess()
        {
        }

        public void UdpClientSendFailure()
        {
        }

        public void UdpClientReceive(int payloadLength)
        {
        }

        public void UdpClientReceiveSuccess()
        {
        }

        public void UdpClientReceiveFailure()
        {
        }

        public void DecodeSucccess(int messageCount)
        {
        }

        public void DecodeFailure()
        {
        }

        public void DecodeMalformedBytes(int byteCount)
        {
        }

        public void EncodeSuccess()
        {
        }

        public void EncodeFailure()
        {
        }
    }
}