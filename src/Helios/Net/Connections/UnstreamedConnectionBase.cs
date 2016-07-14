// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Helios.Buffers;
using Helios.Channels;
using Helios.Exceptions;
using Helios.Ops;
using Helios.Serialization;
using Helios.Topology;
using Helios.Tracing;
using Helios.Util;

namespace Helios.Net.Connections
{
    public abstract class UnstreamedConnectionBase : IConnection
    {
        protected UnstreamedConnectionBase(int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE)
            : this(
                EventLoopFactory.CreateNetworkEventLoop(), null, Encoders.DefaultEncoder, Encoders.DefaultDecoder,
                UnpooledByteBufAllocator.Default, bufferSize)
        {
        }

        protected UnstreamedConnectionBase(NetworkEventLoop eventLoop, INode binding, TimeSpan timeout,
            IMessageEncoder encoder, IMessageDecoder decoder, IByteBufAllocator allocator,
            int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE)
        {
            Decoder = decoder;
            Encoder = encoder;
            Allocator = allocator;
            Created = DateTimeOffset.UtcNow;
            Binding = binding;
            Timeout = timeout;
            BufferSize = bufferSize;
            NetworkEventLoop = eventLoop;
        }

        protected UnstreamedConnectionBase(NetworkEventLoop eventLoop, INode binding, IMessageEncoder encoder,
            IMessageDecoder decoder, IByteBufAllocator allocator, int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE)
            : this(
                eventLoop, binding, NetworkConstants.DefaultConnectivityTimeout, encoder, decoder, allocator, bufferSize
                )
        {
        }

        protected int BufferSize { get; set; }

        protected NetworkEventLoop NetworkEventLoop { get; set; }
        public INode Binding { get; protected set; }


        public event ReceivedDataCallback Receive
        {
            add { NetworkEventLoop.Receive = value; }
// ReSharper disable once ValueParameterNotUsed
            remove { NetworkEventLoop.Receive = null; }
        }

        public event ConnectionEstablishedCallback OnConnection
        {
            add { NetworkEventLoop.Connection = value; }
// ReSharper disable once ValueParameterNotUsed
            remove { NetworkEventLoop.Connection = null; }
        }

        public event ConnectionTerminatedCallback OnDisconnection
        {
            add { NetworkEventLoop.Disconnection = value; }
// ReSharper disable once ValueParameterNotUsed
            remove { NetworkEventLoop.Disconnection = null; }
        }

        public event ExceptionCallback OnError
        {
            add { NetworkEventLoop.SetExceptionHandler(value, this); }
// ReSharper disable once ValueParameterNotUsed
            remove { NetworkEventLoop.SetExceptionHandler(null, this); }
        }

        public IEventLoop EventLoop
        {
            get { return NetworkEventLoop; }
        }

        public IMessageEncoder Encoder { get; protected set; }
        public IMessageDecoder Decoder { get; protected set; }
        public IByteBufAllocator Allocator { get; protected set; }

        public DateTimeOffset Created { get; }
        public INode RemoteHost { get; protected set; }
        public INode Local { get; protected set; }
        public TimeSpan Timeout { get; protected set; }
        public abstract TransportType Transport { get; }
        public abstract bool Blocking { get; set; }
        public bool WasDisposed { get; protected set; }
        public bool Receiving { get; protected set; }

        public abstract bool IsOpen();
        public abstract int Available { get; }

        [Obsolete("No longer supported")]
        public int MessagesInSendQueue
        {
            get { return 0; }
        }

        public abstract Task<bool> OpenAsync();
        public abstract void Configure(IConnectionConfig config);

        public abstract void Open();

        public void BeginReceive()
        {
            if (NetworkEventLoop.Receive == null) throw new NullReferenceException("Receive cannot be null");
            if (Receiving) return;

            Receiving = true;
            BeginReceiveInternal();
        }

        public void BeginReceive(ReceivedDataCallback callback)
        {
            if (callback == null) throw new ArgumentNullException("callback");
            if (Receiving) return;

            Receive += callback;
            Receiving = true;
            BeginReceiveInternal();
        }

        public virtual void StopReceive()
        {
            Receiving = false;
        }

        public abstract void Close();

        public void Send(NetworkData data)
        {
            HeliosTrace.Instance.TcpClientSendQueued();
            SendInternal(data.Buffer, 0, data.Length, data.RemoteHost);
        }

        public void Send(byte[] buffer, int index, int length, INode destination)
        {
            Send(NetworkData.Create(destination, buffer.Slice(index, length), length));
        }

        protected NetworkState CreateNetworkState(Socket socket, INode remotehost)
        {
            return CreateNetworkState(socket, remotehost, Allocator.Buffer(), BufferSize);
        }

        protected NetworkState CreateNetworkState(Socket socket, INode remotehost, IByteBuf buffer, int bufferSize)
        {
            return new NetworkState(socket, remotehost, buffer, bufferSize);
        }

        protected virtual void ReceiveCallback(IAsyncResult ar)
        {
            var receiveState = (NetworkState) ar.AsyncState;
            try
            {
                var received = receiveState.Socket.EndReceive(ar);

                if (!receiveState.Socket.Connected || received == 0)
                {
                    Receiving = false;
                    HeliosTrace.Instance.TcpClientReceiveFailure();
                    Close(new HeliosConnectionException(ExceptionType.Closed));
                    return;
                }

                receiveState.Buffer.WriteBytes(receiveState.RawBuffer, 0, received);
                HeliosTrace.Instance.TcpClientReceive(received);
                List<IByteBuf> decoded;
                Decoder.Decode(this, receiveState.Buffer, out decoded);

                foreach (var message in decoded)
                {
                    var networkData = NetworkData.Create(receiveState.RemoteHost, message);
                    InvokeReceiveIfNotNull(networkData);
                    HeliosTrace.Instance.TcpClientReceiveSuccess();
                }

                //reuse the buffer
                if (receiveState.Buffer.ReadableBytes == 0)
                    receiveState.Buffer.SetIndex(0, 0);
                else
                    receiveState.Buffer.CompactIfNecessary();

                //continue receiving in a loop
                if (Receiving)
                {
                    receiveState.Socket.BeginReceive(receiveState.RawBuffer, 0, receiveState.RawBuffer.Length,
                        SocketFlags.None, ReceiveCallback, receiveState);
                }
            }
            catch (SocketException ex) //typically means that the socket is now closed
            {
                HeliosTrace.Instance.TcpClientReceiveFailure();
                Receiving = false;
                Close(new HeliosConnectionException(ExceptionType.Closed, ex));
            }
            catch (ObjectDisposedException ex) //socket was already disposed
            {
                HeliosTrace.Instance.TcpClientReceiveFailure();
                Receiving = false;
                InvokeDisconnectIfNotNull(RemoteHost,
                    new HeliosConnectionException(ExceptionType.Closed, ex));
            }
            catch (Exception ex)
            {
                HeliosTrace.Instance.TcpClientReceiveFailure();
                InvokeErrorIfNotNull(ex);
            }
        }

        public void InvokeReceiveIfNotNull(NetworkData data)
        {
            if (NetworkEventLoop.Receive != null)
            {
                NetworkEventLoop.Receive(data, this);
            }
        }

        protected void InvokeConnectIfNotNull(INode remoteHost)
        {
            if (NetworkEventLoop.Connection != null)
            {
                NetworkEventLoop.Connection(remoteHost, this);
            }
        }

        protected void InvokeDisconnectIfNotNull(INode remoteHost, HeliosConnectionException ex)
        {
            if (NetworkEventLoop.Disconnection != null)
            {
                NetworkEventLoop.Disconnection(ex, this);
            }
        }

        protected void InvokeErrorIfNotNull(Exception ex)
        {
            if (NetworkEventLoop.Exception != null)
            {
                NetworkEventLoop.Exception(ex, this);
            }
            else
            {
                throw new HeliosException("Unhandled exception on a connection with no error handler", ex);
            }
        }

        protected abstract void BeginReceiveInternal();

        public abstract void Close(Exception reason);

        protected abstract void SendInternal(byte[] buffer, int index, int length, INode destination);

        public override string ToString()
        {
            return string.Format("{0}/{1}", Binding, Created);
        }

        #region IDisposable members

        /// <summary>
        ///     Prevents disposed connections from being re-used again
        /// </summary>
        protected void CheckWasDisposed()
        {
            if (WasDisposed)
                throw new ObjectDisposedException("connection has been disposed of");
        }

        public virtual void Dispose()
        {
            try
            {
                Close();
            }
            catch
            {
            }
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);

        #endregion
    }
}