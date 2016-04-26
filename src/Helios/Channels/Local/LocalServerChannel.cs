using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Helios.Concurrency;

namespace Helios.Channels.Local
{
    public class LocalServerChannel : AbstractServerChannel
    {
        private readonly IChannelConfiguration _config;
        private readonly Queue<object> _inboundBuffer = new Queue<object>();

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

        private readonly ServerShutdownHook _shutdownHook;

        private volatile int _state; // 0 - open, 1 - active, 2 - closed
        private volatile LocalAddress _localAddress;
        private volatile bool _acceptInProgress;

        public LocalServerChannel()
        {
            _config = new DefaultChannelConfiguration(this);
            _shutdownHook = new ServerShutdownHook(this);
        }

        public override bool DisconnectSupported { get { return false; } }
        public override bool Open { get { return _state < 2; } }
        public override bool IsActive { get { return _state == 1; } }
        public override IChannelConfiguration Configuration => _config;
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
            ((SingleThreadEventLoop) EventLoop).AddShutdownHook(_shutdownHook);
        }

        protected override void DoDeregister()
        {
            ((SingleThreadEventLoop)EventLoop).RemoveShutdownHook(_shutdownHook);
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

        

        public new LocalAddress LocalAddress
        {
            get { return (LocalAddress)base.LocalAddress; }
        }

        public new LocalAddress RemoteAddress
        {
            get { return (LocalAddress) base.RemoteAddress; }
        }

        protected override EndPoint LocalAddressInternal { get { return _localAddress;} }

        private static readonly Action<object, object> ServeAction = (context, state) =>
        {
            var server = context as LocalServerChannel;
            var child = state as LocalChannel;
            server.Serve0(child);
        };

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
    }
}
