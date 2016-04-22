using System;
using System.Net;
using System.Threading.Tasks;
using Helios.Buffers;
using Helios.Channels;
using Helios.Channels.Embedded;
using Helios.Concurrency;

namespace Helios.FsCheck.Tests.Channels
{
    /// <summary>
    /// Internal <see cref="IChannel"/> implementation used for lightweight testing.
    /// 
    /// Mostly for testing things that go INSIDE the channel itself.
    /// </summary>
    internal class TestChannel
    {

        public static IChannel Instance
        {
            get
            {

                var instance = new EmbeddedChannel();
                instance.Configuration.WriteBufferHighWaterMark = ChannelOutboundBufferSpecs.WriteHighWaterMark;
                instance.Configuration.WriteBufferLowWaterMark = ChannelOutboundBufferSpecs.WriteLowWaterMark;

                return instance;
            }
        }
    }
}