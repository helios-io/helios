using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Helios.Buffers;
using Helios.Concurrency;
using Helios.Net;
using Helios.Net.Connections;
using Helios.Topology;
using Helios.Util.Collections;
using Helios.Util.TimedOps;

namespace Helios.Reactor.Response
{
    /// <summary>
    /// A <see cref="ReactorResponseChannel"/> instance which manages all of the socket I/O for the child connection directly.
    /// 
    /// Shares the same underlying <see cref="IFiber"/> as the parent <see cref="IReactor"/> responsible for creating this child.
    /// </summary>
    public class TcpReactorResponseChannel : ReactorResponseChannel
    {
        protected ConcurrentCircularBuffer<NetworkData> SendQueue = new ConcurrentCircularBuffer<NetworkData>(10, 1500);
        protected int Throughput = 10;
        protected int IsIdle = SendBufferProcessingStatus.Idle; //1 for busy, 0 for idle
        protected volatile bool HasUnsentMessages;

        public TcpReactorResponseChannel(ReactorBase reactor, Socket outboundSocket, NetworkEventLoop eventLoop, int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE)
            : this(reactor, outboundSocket, (IPEndPoint)outboundSocket.RemoteEndPoint, eventLoop, bufferSize)
        {
        }

        public TcpReactorResponseChannel(ReactorBase reactor, Socket outboundSocket, IPEndPoint endPoint, NetworkEventLoop eventLoop, int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE)
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
            HasUnsentMessages = true;
            SendQueue.Enqueue(data);
            Schedule();
        }

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

                }
            }
            catch (SocketException ex)
            {
                Close();
            }
            catch (Exception ex)
            {
                InvokeErrorIfNotNull(ex);
            }
        }
    }
}