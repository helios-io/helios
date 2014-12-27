using System;
using System.Net;
using System.Threading.Tasks;
using Helios.Buffers;
using Helios.Net;
using Helios.Ops;
using Helios.Serialization;
using Helios.Topology;

namespace Helios.Tests
{
    /// <summary>
    /// Fake connection used for testing purposes
    /// </summary>
    public class DummyConnection : IConnection
    {
        public DummyConnection(IByteBufAllocator allocator)
        {
            Allocator = allocator;
        }


        public void Dispose()
        {
            
        }

        public event ReceivedDataCallback Receive;
        public event ConnectionEstablishedCallback OnConnection;
        public event ConnectionTerminatedCallback OnDisconnection;
        public event ExceptionCallback OnError;
        public IEventLoop EventLoop { get; private set; }
        public IMessageEncoder Encoder { get; private set; }
        public IMessageDecoder Decoder { get; private set; }
        public IByteBufAllocator Allocator { get; private set; }
        public DateTimeOffset Created { get; private set; }
        public INode RemoteHost { get; private set; }
        public INode Local { get; private set; }
        public TimeSpan Timeout { get; private set; }
        public TransportType Transport { get; private set; }
        public bool Blocking { get; set; }
        public bool WasDisposed { get; private set; }
        public bool Receiving { get; private set; }
        public bool IsOpen()
        {
            throw new NotImplementedException();
        }

        public int Available { get; private set; }
        public int MessagesInSendQueue { get; private set; }

        public Task<bool> OpenAsync()
        {
            throw new NotImplementedException();
        }

        public void Configure(IHeliosConfig config)
        {
            throw new NotImplementedException();
        }

        public void Open()
        {
            throw new NotImplementedException();
        }

        public void BeginReceive()
        {
            throw new NotImplementedException();
        }

        public void BeginReceive(ReceivedDataCallback callback)
        {
            throw new NotImplementedException();
        }

        public void StopReceive()
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Send(NetworkData data)
        {
            throw new NotImplementedException();
        }

        public void Send(byte[] buffer, int index, int length, INode destination)
        {
            throw new NotImplementedException();
        }
    }
}
