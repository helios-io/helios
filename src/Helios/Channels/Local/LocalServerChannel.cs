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

        private static readonly Func<Task, object, bool> ShutdownHook = (task, o) =>
        {
            if (task.IsCanceled) return false;
            var server = o as LocalServerChannel;
            if (o == null)
                return false;
            server?.Unsafe.CloseAsync();
            return true;
        };

        private volatile int _state; // 0 - open, 1 - active, 2 - closed
        private volatile LocalAddress _localAddress;
        private volatile bool _acceptInProgress;
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        public LocalServerChannel()
        {
            _config = new DefaultChannelConfiguration(this);
        }

        public override bool DisconnectSupported { get { return false; } }
        public override bool Open { get { return _state < 2; } }
        public override bool Active { get { return _state == 1; } }
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
            // todo: termination hook support for single thread event executor
            ((SingleThreadEventLoop) EventLoop).TerminationTask.ContinueWith(ShutdownHook, this, _cancellationToken.Token);
        }

        protected override void DoDeregister()
        {
            // todo: termination hook support for single thread event executor
            _cancellationToken.Cancel(); // cancel the shutdown task
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
                
            }
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
    }
}
