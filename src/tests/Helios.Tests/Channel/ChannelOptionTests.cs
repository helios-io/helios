using System;
using Helios.Channel;
using NUnit.Framework;

namespace Helios.Tests.Channel
{
    [TestFixture]
    public class ChannelOptionTests
    {
        [Test]
        public void TestExists()
        {
            var name = "test";
            Assert.False(ChannelOption.Exists(name));

            ChannelOption<string> option = ChannelOption.ValueOf(name);
            Assert.True(ChannelOption.Exists(name));
            Assert.NotNull(option);
        }

        [Test]
        public void TestValueOf()
        {
            var name = "test1";
            Assert.False(ChannelOption.Exists(name));
            ChannelOption<string> option = ChannelOption.ValueOf(name);
            ChannelOption<string> option2 = ChannelOption.ValueOf(name);

            Assert.AreSame(option, option2);
        }

        [ExpectedException(typeof(ArgumentException))]
        [Test]
        public void TestCreateOrFail()
        {
            var name = "test2";
            Assert.False(ChannelOption.Exists(name));
            ChannelOption<string> option = ChannelOption.NewInstance<string>(name);
            Assert.True(ChannelOption.Exists(name));
            Assert.NotNull(option);

            ChannelOption.NewInstance<string>(name);
        }
    }
}
