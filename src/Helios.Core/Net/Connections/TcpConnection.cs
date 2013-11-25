using System;
using System.Net;
using System.Net.Sockets;
using Helios.Core.Net.Exceptions;
using Helios.Core.Topology;

namespace Helios.Core.Net.Connections
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
                throw new HeliosConnectionException(ExceptionType.NotOpen, "Connection is not open");

            _client.Client.Send(buffer, offset, size, SocketFlags.None);
        }

        public override void Receieve(byte[] buffer, int offset, int size)
        {
            if (!IsOpen())
                throw new HeliosConnectionException(ExceptionType.NotOpen, "Connection is not open");

            _client.Client.Receive(buffer, offset, size, SocketFlags.None);
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
                throw new HeliosConnectionException(ExceptionType.NotOpen, "Cannot open a connection to a null Node or null Node.Host");
            }

            if (Node.Port <= 0)
            {
                throw new HeliosConnectionException(ExceptionType.NotOpen, "Cannot open a connection to an invalid port");
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
                throw new HeliosConnectionException(ExceptionType.TimedOut, "Timed out on connect");
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