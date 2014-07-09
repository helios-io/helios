using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Helios.Buffers;
using Helios.Exceptions;
using Helios.Ops;
using Helios.Serialization;
using Helios.Topology;
using Helios.Util;
using Helios.Util.Collections;
using Helios.Util.TimedOps;

namespace Helios.Net.Connections
{
    internal static class SendBufferProcessingStatus
    {
        public const int Idle = 0;
        public const int Busy = 1;
    }

    public abstract class UnstreamedConnectionBase : IConnection
    {
        protected ConcurrentCircularBuffer<NetworkData> SendQueue = new ConcurrentCircularBuffer<NetworkData>(10, Int32.MaxValue);
        protected int Throughput = 10;
        protected int IsIdle = SendBufferProcessingStatus.Idle; //1 for busy, 0 for idle
        protected volatile bool HasUnsentMessages;

        protected UnstreamedConnectionBase(int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE) : this(EventLoopFactory.CreateNetworkEventLoop(), null, Encoders.DefaultEncoder, Encoders.DefaultDecoder, UnpooledByteBufAllocator.Default, bufferSize) { }

        protected UnstreamedConnectionBase(NetworkEventLoop eventLoop, INode binding, TimeSpan timeout, IMessageEncoder encoder, IMessageDecoder decoder, IByteBufAllocator allocator, int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE)
            : base()
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

        protected UnstreamedConnectionBase(NetworkEventLoop eventLoop, INode binding, IMessageEncoder encoder, IMessageDecoder decoder, IByteBufAllocator allocator, int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE) : this(eventLoop, binding, NetworkConstants.DefaultConnectivityTimeout, encoder, decoder, allocator, bufferSize) { }



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
            add{ NetworkEventLoop.SetExceptionHandler(value, this); }
// ReSharper disable once ValueParameterNotUsed
            remove { NetworkEventLoop.SetExceptionHandler(null, this); }
        }

        protected int BufferSize { get; set; }

        public IEventLoop EventLoop { get { return NetworkEventLoop; } }
        public IMessageEncoder Encoder { get; protected set; }
        public IMessageDecoder Decoder { get; protected set; }
        public IByteBufAllocator Allocator { get; protected set; }

        protected NetworkEventLoop NetworkEventLoop { get; set; }

        public DateTimeOffset Created { get; private set; }
        public INode RemoteHost { get; protected set; }
        public INode Local { get; protected set; }
        public INode Binding { get; protected set; }
        public TimeSpan Timeout { get; protected set; }
        public abstract TransportType Transport { get; }
        public abstract bool Blocking { get; set; }
        public bool WasDisposed { get; protected set; }
        public bool Receiving { get; protected set; }

        public abstract bool IsOpen();
        public abstract int Available { get; }
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
            if(Receiving) return;

            Receive += callback;
            Receiving = true;
            BeginReceiveInternal();
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
                if (!receiveState.Socket.Connected)
                {
                    Receiving = false;
                    InvokeDisconnectIfNotNull(receiveState.RemoteHost, new HeliosConnectionException(ExceptionType.Closed));
                    Dispose();
                    return;
                }

                var received = receiveState.Socket.EndReceive(ar);
                receiveState.Buffer.WriteBytes(receiveState.RawBuffer, 0, received);

                List<IByteBuf> decoded;
                Decoder.Decode(this, receiveState.Buffer, out decoded);

                foreach (var message in decoded)
                {
                    var networkData = NetworkData.Create(receiveState.RemoteHost, message);
                    InvokeReceiveIfNotNull(networkData);
                }

                //reuse the buffer
                if (receiveState.Buffer.ReadableBytes == 0)
                    receiveState.Buffer.SetIndex(0, 0);
                else
                    receiveState.Buffer.CompactIfNecessary();

                //continue receiving in a loop
                if (Receiving)
                {
                    receiveState.Socket.BeginReceive(receiveState.RawBuffer, 0, receiveState.RawBuffer.Length, SocketFlags.None, ReceiveCallback, receiveState);
                }
            }
            catch (SocketException ex) //typically means that the socket is now closed
            {
                Receiving = false;
                InvokeDisconnectIfNotNull(receiveState.RemoteHost, new HeliosConnectionException(ExceptionType.Closed, ex));
                Dispose();
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

        public virtual void StopReceive()
        {
            Receiving = false;
        }

        public abstract void Close(Exception reason);

        public abstract void Close();

        public void Send(NetworkData data)
        {
            HasUnsentMessages = true;
            SendQueue.Enqueue(data);
            Schedule();
        }

        public void Send(byte[] buffer, int index, int length, INode destination)
        {
            Send(NetworkData.Create(destination, buffer.Slice(index, length), length));
        }

        protected abstract void SendInternal(byte[] buffer, int index, int length, INode destination);

        /// <summary>
        /// Schedules the send buffer to begin draining
        /// </summary>
        protected void Schedule()
        {
            //only schedule if we're idle
            if (Interlocked.Exchange(ref IsIdle, SendBufferProcessingStatus.Busy) == SendBufferProcessingStatus.Idle)
            {
                EventLoop.Execute(Run);
            }
        }

        protected void Run()
        {
            if (WasDisposed || !IsOpen())
                return;

            //Set the deadline timer for this run
            var deadlineTimer = Deadline.Now + Timeout;

            //we are about to process all enqueued messages
            HasUnsentMessages = false;

            //we should process x messages in this run
            var left = Throughput;

            NetworkData message;
            while (SendQueue.TryTake(out message))
            {
                SendInternal(message.Buffer, 0, message.Length, message.RemoteHost);
                left--;
                if (WasDisposed)
                    return;

                //if the deadline has expired, stop and break
                if (deadlineTimer.IsOverdue || left == 0)
                {
                    break; //we're done for this run
                }
            }

            //there are still unsent messages that need to be processed
            if (SendQueue.Count > 0)
                HasUnsentMessages = true;

            if (HasUnsentMessages)
                EventLoop.Execute(Run);
            else
                Interlocked.Exchange(ref IsIdle, SendBufferProcessingStatus.Idle);
        }

        public override string ToString()
        {
            return string.Format("{0}/{1}", Binding, Created);
        }

        #region IDisposable members

        /// <summary>
        /// Prevents disposed connections from being re-used again
        /// </summary>
        protected void CheckWasDisposed()
        {
            if (WasDisposed)
                throw new ObjectDisposedException("connection has been disposed of");
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);

        #endregion
    }
}