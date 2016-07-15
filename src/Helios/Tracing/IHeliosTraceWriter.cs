// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

namespace Helios.Tracing
{
    /// <summary>
    ///     Interface for recording traces through<see cref="HeliosTrace" />
    /// </summary>
    public interface IHeliosTraceWriter
    {
        #region TCP outbound trace methods

        /// <summary>
        ///     Outbound TCP connection succeeded
        /// </summary>
        void TcpClientConnectSuccess();

        /// <summary>
        ///     Outbound TCP connection failed to connect
        /// </summary>
        /// <param name="reason">Reason why we failed to connect</param>
        void TcpClientConnectFailure(string reason);

        /// <summary>
        ///     Outbound TCP connection sent N bytes
        /// </summary>
        /// <param name="payloadLength">The number of bytes sent</param>
        void TcpClientSend(int payloadLength);

        /// <summary>
        ///     Recorded a successful TCP send operation.
        /// </summary>
        void TcpClientSendSuccess();

        /// <summary>
        ///     Recorded an unsuccessful TCP send operation.
        /// </summary>
        void TcpClientSendFailure();

        /// <summary>
        ///     TCP client received N bytes
        /// </summary>
        /// <param name="payloadLength">The number of bytes received</param>
        void TcpClientReceive(int payloadLength);

        /// <summary>
        ///     Recorded a successful TCP receive operation.
        /// </summary>
        void TcpClientReceiveSuccess();

        /// <summary>
        ///     Recorded an unsuccessful TCP receive operation.
        /// </summary>
        void TcpClientReceiveFailure();

        /// <summary>
        ///     Queued an outbound send on an outbound TCP connection
        /// </summary>
        void TcpClientSendQueued();

        #endregion

        #region TCP inbound trace methods

        /// <summary>
        ///     Inbound TCP connection succeeded
        /// </summary>
        void TcpInboundAcceptSuccess();

        /// <summary>
        ///     Inbound TCP connection failed to connect
        /// </summary>
        /// <param name="reason">Reason why we failed to connect</param>
        void TcpInboundAcceptFailure(string reason);

        /// <summary>
        ///     Inbound TCP connection sent N bytes
        /// </summary>
        /// <param name="payloadLength">The number of bytes sent</param>
        void TcpInboundClientSend(int payloadLength);

        /// <summary>
        ///     Recorded a successful TCP send operation.
        /// </summary>
        void TcpInboundSendSuccess();

        /// <summary>
        ///     Recorded an unsuccessful TCP send operation.
        /// </summary>
        void TcpInboundSendFailure();

        /// <summary>
        ///     TCP Inbound client received N bytes
        /// </summary>
        /// <param name="payloadLength">The number of bytes received</param>
        void TcpInboundReceive(int payloadLength);

        /// <summary>
        ///     Recorded a successful TCP receive operation.
        /// </summary>
        void TcpInboundReceiveSuccess();

        /// <summary>
        ///     Recorded an unsuccessful TCP receive operation.
        /// </summary>
        void TcpInboundReceiveFailure();

        /// <summary>
        ///     Queued an outbound reply on an inbound TCP connection
        /// </summary>
        void TcpInboundSendQueued();

        #endregion

        #region UDP trace methods

        /// <summary>
        ///     Outbound UDP connection sent N bytes
        /// </summary>
        /// <param name="payloadLength">The number of bytes sent</param>
        void UdpClientSend(int payloadLength);

        /// <summary>
        ///     Recorded a successful UDP send operation.
        /// </summary>
        void UdpClientSendSuccess();

        /// <summary>
        ///     Recorded an unsuccessful UDP send operation.
        /// </summary>
        void UdpClientSendFailure();

        /// <summary>
        ///     UDP client received N bytes
        /// </summary>
        /// <param name="payloadLength">The number of bytes received</param>
        void UdpClientReceive(int payloadLength);

        /// <summary>
        ///     Recorded a successful UDP send operation.
        /// </summary>
        void UdpClientReceiveSuccess();

        /// <summary>
        ///     Recorded an unsuccessful UDP send operation.
        /// </summary>
        void UdpClientReceiveFailure();

        #endregion

        #region LengthFrameDecoder methods

        /// <summary>
        ///     Successfully decoded N number of messages
        /// </summary>
        /// <param name="messageCount">The number of messages decoded.</param>
        void DecodeSucccess(int messageCount);

        /// <summary>
        ///     Error occurred during decoding
        /// </summary>
        void DecodeFailure();

        /// <summary>
        ///     Had to skip N number of malformed bytes
        /// </summary>
        /// <param name="byteCount">The number of bytes skipped</param>
        void DecodeMalformedBytes(int byteCount);

        /// <summary>
        ///     Able to successfully encode a message
        /// </summary>
        void EncodeSuccess();

        /// <summary>
        ///     Failed to encode a message
        /// </summary>
        void EncodeFailure();

        #endregion
    }
}