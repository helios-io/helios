// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Helios.Concurrency;
using Helios.Logging;
using Helios.Util;
using Helios.Util.Concurrency;

namespace Helios.Channels.Local
{
    public class LocalChannel : AbstractChannel
    {
        private const int MAX_READER_STACK_DEPTH = 8;
        private static readonly ILogger Logger = LoggingFactory.GetLogger<LocalChannel>();

        private static readonly Func<object, object, bool> DoFinishPeerReadAsync = (context, o) =>
        {
            var self = (LocalChannel) context;
            var peer = (LocalChannel) o;
            self.FinishPeerRead0(peer);
            return true;
        };

        private static readonly Action<object, object> DoFinishPeerRead = (context, o) =>
        {
            var self = (LocalChannel) context;
            var peer = (LocalChannel) o;
            self.FinishPeerRead0(peer);
        };

        private readonly Queue<object> _inboundBuffer = new Queue<object>();
        private readonly ReadTask _readTask;
        private readonly ShutdownHook _shutdownHook;
        private volatile TaskCompletionSource _connectPromise;
        private volatile Task _finishReadTask;
        private volatile LocalAddress _localAddress;
        private volatile LocalChannel _peer;
        private volatile bool _readInProgress;
        private volatile bool _registerInProgress;
        private volatile LocalAddress _remoteAddress;
        private readonly ThreadLocal<int> _stackDepth = new ThreadLocal<int>(() => 0);


        private volatile State _state;
        private volatile bool _writeInProgress;

        public LocalChannel() : base(null)
        {
            Configuration = new DefaultChannelConfiguration(this);
            _shutdownHook = new ShutdownHook(this);
            _readTask = new ReadTask(this);
        }

        public LocalChannel(LocalServerChannel parent, LocalChannel peer) : base(parent)
        {
            _peer = peer;
            _localAddress = parent.LocalAddress;
            _remoteAddress = peer.LocalAddress;

            Configuration = new DefaultChannelConfiguration(this);
            _shutdownHook = new ShutdownHook(this);
            _readTask = new ReadTask(this);
        }

        public override bool DisconnectSupported
        {
            get { return true; }
        }

        public override bool IsOpen
        {
            get { return _state != State.Closed; }
        }

        public override bool IsActive
        {
            get { return _state == State.Connected; }
        }

        public new LocalServerChannel Parent
        {
            get { return base.Parent as LocalServerChannel; }
        }

        public new LocalAddress LocalAddress
        {
            get { return LocalAddressInternal as LocalAddress; }
        }

        public new LocalAddress RemoteAddress
        {
            get { return RemoteAddressInternal as LocalAddress; }
        }

        protected override EndPoint LocalAddressInternal
        {
            get { return _localAddress; }
        }

        protected override EndPoint RemoteAddressInternal
        {
            get { return _remoteAddress; }
        }

        public override IChannelConfiguration Configuration { get; }

        protected override IChannelUnsafe NewUnsafe()
        {
            return new LocalUnsafe(this);
        }

        protected override bool IsCompatible(IEventLoop eventLoop)
        {
            return eventLoop is SingleThreadEventLoop;
        }


        protected override void DoRegister()
        {
            // Check if both peer and parent are non-null because this channel was created by a LocalServerChannel.
            // This is needed as a peer may not be null also if a LocalChannel was connected before and
            // deregistered / registered later again.
            if (_peer != null && Parent != null)
            {
                // Store the peer in a local variable as it may be set to null if DoClose() is called.
                // Because of this we also set registerInProgress to true as we check for this in DoClose() and make sure
                // we delay the FireChannelInactive() to be fired after the FireChannelActive() and so keep the correct
                // order of events.

                var peer = _peer;
                _registerInProgress = true;
                _state = State.Connected;

                _peer._remoteAddress = Parent?.LocalAddress;
                _peer._state = State.Connected;

                // Always call peer.eventLoop().execute() even if peer.eventLoop().inEventLoop() is true.
                // This ensures that if both channels are on the same event loop, the peer's channelActive
                // event is triggered *after* this channel's channelRegistered event, so that this channel's
                // pipeline is fully initialized by ChannelInitializer before any channelRead events.
                _peer.EventLoop.Execute(() => //todo: allocation
                {
                    _registerInProgress = false;
                    var promise = peer._connectPromise;

                    // Only trigger fireChannelActive() if the promise was not null and was not completed yet.
                    // connectPromise may be set to null if doClose() was called in the meantime.
                    if (promise != null && promise.TryComplete())
                    {
                        peer.Pipeline.FireChannelActive();
                    }
                });
            }

            ((SingleThreadEventExecutor) EventLoop.Unwrap()).AddShutdownHook(_shutdownHook);
        }

        protected override void DoBind(EndPoint localAddress)
        {
            _localAddress = LocalChannelRegistry.Register(this, _localAddress, localAddress);
            _state = State.Bound;
        }

        protected override void DoDisconnect()
        {
            DoClose();
        }

        protected override void DoClose()
        {
            var peer = _peer;
            if (_state != State.Closed)
            {
                // Update all internal state before the CloseTask is notified
                if (_localAddress != null)
                {
                    if (Parent == null)
                    {
                        LocalChannelRegistry.Unregister(_localAddress);
                    }
                    _localAddress = null;
                }

                // State change must happen before finishPeerRead to ensure writes are released either in doWrite or
                // channelRead.
                _state = State.Closed;

                var promise = _connectPromise;
                if (promise != null)
                {
                    promise.TrySetException(ClosedChannelException.Instance);
                    _connectPromise = null;
                }

                // To preserve ordering of events we must process any pending reads
                if (_writeInProgress && peer != null)
                {
                    FinishPeerRead(peer);
                }
            }

            if (peer != null && peer.IsActive)
            {
                if (peer.EventLoop.InEventLoop && !_registerInProgress)
                {
                    DoPeerClose(peer, peer._writeInProgress);
                }
                else
                {
                    // This value may change, and so we should save it before executing the IRunnable
                    var peerWriteInProgress = peer._writeInProgress;
                    try
                    {
                        peer.EventLoop.Execute((context, state) => // todo: allocation
                        {
                            var p = (LocalChannel) context;
                            var wIP = (bool) state;
                            DoPeerClose(p, wIP);
                        }, peer, peerWriteInProgress);
                    }
                    catch (Exception ex)
                    {
                        // The peer close may attempt to drain this._inboundBuffers. If that fails make sure it is drained.
                        ReleaseInboundBuffers();
                        throw;
                    }
                }

                _peer = null;
            }
        }

        protected override void DoBeginRead()
        {
            if (_readInProgress)
            {
                return;
            }

            var pipeline = Pipeline;
            var inboundBuffer = _inboundBuffer;
            if (!inboundBuffer.Any())
            {
                _readInProgress = true;
                return;
            }

            var stackDepth = _stackDepth.Value;
            if (stackDepth < MAX_READER_STACK_DEPTH)
            {
                _stackDepth.Value = stackDepth + 1;
                try
                {
                    while (true)
                    {
                        if (inboundBuffer.Count == 0)
                            break;
                        var received = inboundBuffer.Dequeue();
                        pipeline.FireChannelRead(received);
                    }
                    pipeline.FireChannelReadComplete();
                }
                finally
                {
                    _stackDepth.Value = stackDepth;
                }
            }
            else
            {
                try
                {
                    EventLoop.Execute(_readTask);
                }
                catch (Exception)
                {
                    ReleaseInboundBuffers();
                    throw;
                }
            }
        }

        protected override void DoWrite(ChannelOutboundBuffer input)
        {
            switch (_state)
            {
                case State.Open:
                case State.Bound:
                    throw NotYetConnectedException.Instance;
                case State.Closed:
                    throw ClosedChannelException.Instance;
                case State.Connected:
                    break;
            }

            var peer = _peer;
            _writeInProgress = true;
            try
            {
                while (true)
                {
                    var msg = input.Current;
                    if (msg == null)
                    {
                        break;
                    }
                    try
                    {
                        // It is possible the peer could have closed while we are writing, and in this case we should
                        // simulate real socket behavior and ensure the write operation is failed.
                        if (peer._state == State.Connected)
                        {
                            peer._inboundBuffer.Enqueue(ReferenceCountUtil.Retain(msg));
                            input.Remove();
                        }
                        else
                        {
                            input.Remove(ClosedChannelException.Instance);
                        }
                    }
                    catch (Exception ex)
                    {
                        input.Remove(ex);
                    }
                }
            }
            finally
            {
                _writeInProgress = false;
            }

            FinishPeerRead(peer);
        }

        private void ReleaseInboundBuffers()
        {
            foreach (var o in _inboundBuffer)
            {
                ReferenceCountUtil.Release(o);
            }
        }

        private void DoPeerClose(LocalChannel peer, bool peerWriteInProgress)
        {
            if (peerWriteInProgress)
            {
                FinishPeerRead0(peer);
            }
            peer.Unsafe.CloseAsync();
        }

        private void FinishPeerRead(LocalChannel peer)
        {
            // If the peer is also writing, then we must schedule the event on the event loop to preserve read order.
            if (peer.EventLoop == EventLoop && !peer._writeInProgress)
            {
                FinishPeerRead0(peer);
            }
            else
            {
                RunFinishPeerReadTask(peer);
            }
        }

        private void RunFinishPeerReadTask(LocalChannel peer)
        {
            try
            {
                if (peer._writeInProgress)
                {
                    peer._finishReadTask = _peer.EventLoop.SubmitAsync(DoFinishPeerReadAsync, this, peer);
                }
                else
                {
                    peer.EventLoop.Execute(DoFinishPeerRead, this, peer);
                }
            }
            catch (Exception)
            {
                _peer.ReleaseInboundBuffers();
                throw;
            }
        }

        private void FinishPeerRead0(LocalChannel peer)
        {
            var finishPeerReadTask = peer._finishReadTask;
            if (finishPeerReadTask != null)
            {
                if (!finishPeerReadTask.IsCompleted)
                {
                    RunFinishPeerReadTask(peer);
                    return;
                }
                // TODO: might need to make this lazy in order to avoid a premature unset while scheduling a new task
                peer._finishReadTask = null;
            }

            var peerPipeline = peer.Pipeline;
            if (peer._readInProgress)
            {
                peer._readInProgress = false;
                while (true)
                {
                    if (peer._inboundBuffer.Count == 0)
                    {
                        break;
                    }
                    var received = peer._inboundBuffer.Dequeue();
                    peerPipeline.FireChannelRead(received);
                }
                peerPipeline.FireChannelReadComplete();
            }
        }

        private enum State
        {
            Open,
            Bound,
            Connected,
            Closed
        }

        private class ReadTask : IRunnable
        {
            private readonly LocalChannel _channel;

            public ReadTask(LocalChannel channel)
            {
                _channel = channel;
            }

            public void Run()
            {
                var pipeline = _channel.Pipeline;
                var inboundBuffer = _channel._inboundBuffer;
                while (true)
                {
                    if (inboundBuffer.Count == 0)
                        break;
                    var msg = inboundBuffer.Dequeue();
                    pipeline.FireChannelRead(msg);
                }
                pipeline.FireChannelReadComplete();
            }
        }

        private class ShutdownHook : IRunnable
        {
            private readonly LocalChannel _channel;

            public ShutdownHook(LocalChannel channel)
            {
                _channel = channel;
            }

            public void Run()
            {
                _channel.Unsafe.CloseAsync();
            }
        }

        private class LocalUnsafe : AbstractUnsafe
        {
            public LocalUnsafe(LocalChannel channel) : base(channel)
            {
                Local = channel;
            }

            private LocalChannel Local { get; }

            public override Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress)
            {
                if (Local._state == State.Connected)
                {
                    var cause = new AlreadyConnectedException();
                    Local.Pipeline.FireExceptionCaught(cause);
                    return TaskEx.FromException(cause);
                }

                if (Local._connectPromise != null)
                {
                    throw new ConnectionPendingException();
                }

                Local._connectPromise = new TaskCompletionSource();
                if (Local._state != State.Bound)
                {
                    // Not bound yet and no LocalAddress specified. Get one
                    if (localAddress == null)
                    {
                        localAddress = new LocalAddress(Local);
                    }
                }

                if (localAddress != null)
                {
                    try
                    {
                        Local.DoBind(localAddress);
                    }
                    catch (Exception ex)
                    {
                        PromiseUtil.SafeSetFailure(Local._connectPromise, ex, Logger);
                        return CloseAsync();
                    }
                }

                var boundChannel = LocalChannelRegistry.Get(remoteAddress);
                if (!(boundChannel is LocalServerChannel))
                {
                    var cause = new ChannelException("connection refused");
                    PromiseUtil.SafeSetFailure(Local._connectPromise, cause, Logger);
                    return CloseAsync();
                }

                var serverChannel = boundChannel as LocalServerChannel;
                Local._peer = serverChannel.Serve(Local);
                return TaskEx.Completed;
            }
        }
    }
}