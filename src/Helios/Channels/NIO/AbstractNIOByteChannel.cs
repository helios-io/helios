using System;
using System.Configuration;
using System.IO;
using Helios.Channels.Socket;
using Helios.Net;
using Helios.Ops;
using Helios.Topology;

namespace Helios.Channels.NIO
{

    /// <summary>
    /// A <see cref="AbstractNioChannel"/> which uses <see cref="byte"/>s as the underlying message-passing store
    /// </summary>
    public abstract class AbstractNioByteChannel : AbstractNioChannel
    {
        protected AbstractNioByteChannel(IChannel parent, IEventLoop loop, IConnection connection) : base(parent, loop, connection)
        {
        }

        protected abstract int DoWriteBytes(byte[] buff);



        #region NioByteUnsafe implementation

        private sealed class NioByteUnsafe : AbstractNioUnsafe
        {
            private IByteBufHandle allocHandle;

            public NioByteUnsafe(AbstractNioByteChannel channel)
                : base(channel)
            {
            }

            private new AbstractNioByteChannel Channel { get { return (AbstractNioByteChannel)base.Channel; } }

            protected override INode LocalAddressInternal()
            {
                return Connection.Local;
            }

            protected override INode RemoteAddressInternal()
            {
                return Connection.RemoteHost;
            }

            protected override void DoBind(INode localAddress)
            {
                Connection.Open();
            }

            protected override void DoDisconnect()
            {
                Connection.Close();
            }

            protected override void DoClose()
            {
                Connection.Dispose();
            }

            protected override void DoWrite(ChannelOutboundBuffer buff)
            {
                var writeSpinCount = -1;
                for (;;)
                {
                    var msg = buff.Current;
                    if (msg == null || msg.Length == 0)
                    {
                        //wrote all messages
                        break;
                    }

                    var done = false;
                    if (writeSpinCount == -1)
                    {
                        writeSpinCount = Channel.Config.WriteSpinCount;
                    }

                    for (var i = writeSpinCount - 1; i >= 0; i--)
                    {
                        var localFlushedAmount = Channel.DoWriteBytes(msg);
                        if (localFlushedAmount == 0)
                        {
                            break;
                        }

                        if (localFlushedAmount == msg.Length)
                        {
                            done = true;
                            break;
                        }
                    }

                    if (done)
                    {
                        buff.Remove();
                    }
                }
            }

            public override void Read()
            {
                var pipeline = Channel.Pipeline;

                ((NioEventLoop) EventLoop).Receive = (data, channel) =>
                {
                    try
                    {
                        pipeline.FireChannelRead(data);
                        pipeline.FireChannelReadComplete();
                    }
                    catch (Exception ex)
                    {
                        HandleReadException(pipeline, data, ex, false);
                    }
                };
                
            }

            private void CloseOnRead(IChannelPipeline pipeline)
            {
                Channel.IsInputShutdown = true;
                if (Channel.IsOpen)
                {
                    pipeline.FireUserEventTriggered(ChannelInputShutdownEvent.Instance);
                }
                else
                {
                    Close(VoidPromise());
                }
            }

            private void HandleReadException(IChannelPipeline pipeline, NetworkData message, Exception cause, bool close)
            {
                if (message.Buffer != null)
                {
                    pipeline.FireChannelRead(message);
                }
                pipeline.FireExceptionCaught(cause);
                if (close || cause is IOException)
                {
                    CloseOnRead(pipeline);   
                }
            }
        }

        #endregion
    }
}
