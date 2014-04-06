using Helios.Channels;
using NUnit.Framework;

namespace Helios.Tests.Channels
{
    [TestFixture]
    public class DefaultChannelIdTests
    {
        #region Tests

        [Test]
        public void Should_create_new_ChannelId()
        {
            var channelId = DefaultChannelId.NewChannelId();
            Assert.IsNotNull(channelId);
        }

        [Test(Description = "We should be able to create two different channels at roughly the same time, despite sharing the same machine / process")]
        public void Should_create_different_ChannelIds_on_same_process_and_machine()
        {
            var channel1 = DefaultChannelId.NewChannelId();
            var channel2 = DefaultChannelId.NewChannelId();

            Assert.AreNotEqual(channel1, channel2);
        }

        #endregion
    }
}
