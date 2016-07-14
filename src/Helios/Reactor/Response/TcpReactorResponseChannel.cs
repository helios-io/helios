// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Helios.Buffers;
using Helios.Channels;
using Helios.Concurrency;
using Helios.Net;
using Helios.Topology;
using Helios.Tracing;

namespace Helios.Reactor.Response
{
    /// <summary>
    ///     A <see cref="ReactorResponseChannel" /> instance which manages all of the socket I/O for the child connection
    ///     directly.
    ///     Shares the same underlying <see cref="IFiber" /> as the parent <see cref="IReactor" /> responsible for creating
    ///     this child.
    /// </summary>
    public class TcpReactorResponseChannel : ReactorResponseChannel
    {
        public TcpReactorResponseChannel(ReactorBase reactor, Socket outboundSocket, NetworkEventLoop eventLoop,
            int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE)
            : this(reactor, outboundSocket, (IPEndPoint) outboundSocket.RemoteEndPoint, eventLoop, bufferSize)
        {
        }

        public TcpReactorResponseChannel(ReactorBase reactor, Socket outboundSocket, IPEndPoint endPoint,
            NetworkEventLoop eventLoop, int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE)
            : base(reactor, outboundSocket, endPoint, eventLoop)
        {
        }

        public override void Configure(IConnectionConfig config)
        {
        }

        protected override void BeginReceiveInternal()
        {
        }

        protected override void StopReceiveInternal()
        {
        }

        public override void Send(NetworkData data)
        {
            HeliosTrace.Instance.TcpInboundSendQueued();
            SendInternal(data.Buffer, 0, data.Length, data.RemoteHost);
        }

        private void SendInternal(byte[] buffer, int index, int length, INode remoteHost)
        {
            try
            {
                if (WasDisposed || Socket == null || !Socket.Connected)
                {
                    Close();
                    return;
                }

                var buf = Allocator.Buffer(length);
                buf.WriteBytes(buffer, index, length);
                List<IByteBuf> encodedMessages;
                Encoder.Encode(this, buf, out encodedMessages);
                foreach (var message in encodedMessages)
                {
                    var bytesToSend = message.ToArray();
                    var bytesSent = 0;
                    while (bytesSent < bytesToSend.Length)
                    {
                        bytesSent += Socket.Send(bytesToSend, bytesSent, bytesToSend.Length - bytesSent,
                            SocketFlags.None);
                    }
                    HeliosTrace.Instance.TcpInboundClientSend(bytesSent);
                    HeliosTrace.Instance.TcpInboundSendSuccess();
                }
            }
            catch (SocketException ex)
            {
                HeliosTrace.Instance.TcpClientSendFailure();
                Close();
            }
            catch (Exception ex)
            {
                HeliosTrace.Instance.TcpClientSendFailure();
                InvokeErrorIfNotNull(ex);
            }
        }
    }
}