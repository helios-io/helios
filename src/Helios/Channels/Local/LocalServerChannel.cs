// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Helios.Concurrency;

namespace Helios.Channels.Local
{
    public class LocalServerChannel : AbstractServerChannel
    {
        private static readonly Action<object, object> ServeAction = (context, state) =>
        {
            var server = context as LocalServerChannel;
            var child = state as LocalChannel;
            server.Serve0(child);
        };

        private readonly Queue<object> _inboundBuffer = new Queue<object>();

        private readonly ServerShutdownHook _shutdownHook;
        private volatile bool _acceptInProgress;
        private volatile LocalAddress _localAddress;

        private volatile int _state; // 0 - open, 1 - active, 2 - closed

        public LocalServerChannel()
        {
            Configuration = new DefaultChannelConfiguration(this);
            _shutdownHook = new ServerShutdownHook(this);
        }

        public override bool DisconnectSupported
        {
            get { return false; }
        }

        public override bool IsOpen
        {
            get { return _state < 2; }
        }

        public override bool IsActive
        {
            get { return _state == 1; }
        }

        public override IChannelConfiguration Configuration { get; }


        public new LocalAddress LocalAddress
        {
            get { return (LocalAddress) base.LocalAddress; }
        }

        public new LocalAddress RemoteAddress
        {
            get { return (LocalAddress) base.RemoteAddress; }
        }

        protected override EndPoint LocalAddressInternal
        {
            get { return _localAddress; }
        }

        protected override bool IsCompatible(IEventLoop eventLoop)
        {
            return eventLoop is SingleThreadEventLoop;
        }

        protected override void DoBind(EndPoint localAddress)
        {
            _localAddress = LocalChannelRegistry.Register(this, _localAddress, localAddress);
            _state = 1;
        }

        protected override void DoRegister()
        {
            ((SingleThreadEventExecutor) EventLoop.Unwrap()).AddShutdownHook(_shutdownHook);
        }

        protected override void DoDeregister()
        {
            ((SingleThreadEventExecutor) EventLoop.Unwrap()).RemoveShutdownHook(_shutdownHook);
        }

        protected override void DoClose()
        {
            if (_state <= 1)
            {
                // Update all internal state before the CloseTask is notified
                if (_localAddress != null)
                {
                    LocalChannelRegistry.Unregister(_localAddress);
                    _localAddress = null;
                }
                _state = 2;
            }
        }

        protected override void DoBeginRead()
        {
            if (_acceptInProgress)
            {
                return;
            }

            var inboundBuffer = _inboundBuffer;
            if (!inboundBuffer.Any())
            {
                _acceptInProgress = true;
                return;
            }

            var pipeline = Pipeline;
            while (true)
            {
                if (inboundBuffer.Count == 0)
                    break;
                var msg = inboundBuffer.Dequeue();
                pipeline.FireChannelRead(msg);
            }
            pipeline.FireChannelReadComplete();
        }

        public LocalChannel Serve(LocalChannel peer)
        {
            var child = new LocalChannel(this, peer);
            if (EventLoop.InEventLoop)
            {
                Serve0(child);
            }
            else
            {
                EventLoop.Execute(ServeAction, this, child);
            }
            return child;
        }

        private void Serve0(LocalChannel child)
        {
            _inboundBuffer.Enqueue(child);
            if (_acceptInProgress)
            {
                _acceptInProgress = false;
                var pipeline = Pipeline;
                while (true)
                {
                    if (_inboundBuffer.Count == 0)
                        break;
                    var m = _inboundBuffer.Dequeue();
                    pipeline.FireChannelRead(m);
                }
                pipeline.FireChannelReadComplete();
            }
        }

        private class ServerShutdownHook : IRunnable
        {
            private readonly LocalServerChannel _channel;

            public ServerShutdownHook(LocalServerChannel channel)
            {
                _channel = channel;
            }

            public void Run()
            {
                _channel.Unsafe.CloseAsync();
            }
        }
    }
}