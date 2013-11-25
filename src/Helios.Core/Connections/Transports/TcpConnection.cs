using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Helios.Core.Topology;

namespace Helios.Core.Connections.Transports
{
    public class TcpConnection : ConnectionBase
    {
        private TcpClient _client;

        public TcpConnection(INode node, TimeSpan timeout)
            : base(node, timeout)
        {
            InitClient();
        }

        public TcpConnection(INode node)
            : base(node)
        {
            InitClient();
        }

        public override TransportType Transport { get { return TransportType.Tcp; } }
        public override void Send(byte[] buffer, int offset, int size)
        {
            if (!IsOpen())

                throw new NotImplementedException();
        }

        public override void Receieve(byte[] buffer, int offset, int size)
        {
            throw new NotImplementedException();
        }

        public override bool IsOpen()
        {
            if (_client == null) return false;
            return _client.Connected;
        }

        public override void Open()
        {
            CheckWasDisposed();

            if (IsOpen()) return;

            if (Node == null || Node.Host == null)
            {
                throw new ArgumentNullException("Node", "Node and Node.Host cannot be null");
            }

            if (Node.Port <= 0)
            {
                throw new ArgumentOutOfRangeException("Node.Port", "Cannot open without port");
            }

            if (_client == null)
                InitClient();

            var ar = _client.BeginConnect(Node.Host, Node.Port, null, null);
            if (ar.AsyncWaitHandle.WaitOne(Timeout))
            {
                _client.EndConnect(ar);
            }
            else
            {
                _client.Close();
                throw new TimeoutException();
            }
        }

        public override void Close()
        {
            CheckWasDisposed();

            if (!IsOpen())
                return;

            _client.Close();
            _client = null;
        }

        #region IDisposable Members

        protected override void Dispose(bool disposing)
        {
            if (!WasDisposed)
            {
                if (disposing)
                {
                    if (_client != null)
                    {
                        ((IDisposable)_client).Dispose();
                    }
                }
            }
            WasDisposed = true;
        }

        #endregion

        private void InitClient()
        {
            _client = new TcpClient(Node.Host.ToString(), Node.Port)
            {
                ReceiveTimeout = Timeout.Seconds,
                SendTimeout = Timeout.Seconds,
                Client = {NoDelay = true}
            };
        }
    }
}