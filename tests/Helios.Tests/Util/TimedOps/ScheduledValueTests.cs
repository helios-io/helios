using System;
using System.Threading;
using Helios.Util.TimedOps;
using Xunit;

namespace Helios.Tests.Util.TimedOps
{
    
    public class ScheduledValueTests
    {
        #region Setup / Teardown
        #endregion

        #region Tests

        [Fact]
        public void Should_not_have_IsScheduled_set()
        {
            //arrange
            var scheduledSet = new ScheduledValue<bool>(false);

            //act


            //assert
            Assert.False(scheduledSet.IsScheduled);
            Assert.False(scheduledSet.WasSet);
        }

        [Fact]
        public void Should_schedule_future_set()
        {
            //arrange
            var scheduledSet = new ScheduledValue<int>(0);

            //act
            scheduledSet.Schedule(12, TimeSpan.FromSeconds(1));
            Assert.True(scheduledSet.IsScheduled);
            Thread.Sleep(TimeSpan.FromSeconds(2));

            //assert
            Assert.Equal(12, scheduledSet.Value);
            Assert.True(scheduledSet.WasSet);
            Assert.False(scheduledSet.IsScheduled);
        }

        [Fact]
        public void Should_cancel_scheduled_set()
        {
            //arrange
            var scheduledSet = new ScheduledValue<int>(0);

            //act
            scheduledSet.Schedule(12, TimeSpan.FromSeconds(1));
            Assert.True(scheduledSet.IsScheduled);
            scheduledSet.Cancel();

            //assert
            Assert.False(scheduledSet.IsScheduled);
            Assert.False(scheduledSet.WasSet);
        }

        #endregion
    }
}
