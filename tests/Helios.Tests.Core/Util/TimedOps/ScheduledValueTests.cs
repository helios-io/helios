using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Helios.Core.Util.TimedOps;
using NUnit.Framework;

namespace Helios.Tests.Core.Util.TimedOps
{
    [TestFixture]
    public class ScheduledValueTests
    {
        #region Setup / Teardown
        #endregion

        #region Tests

        [Test]
        public void Should_not_have_IsScheduled_set()
        {
            //arrange
            var scheduledSet = new ScheduledValue<bool>(false);

            //act


            //assert
            Assert.IsFalse(scheduledSet.IsScheduled);
            Assert.IsFalse(scheduledSet.WasSet);
        }

        [Test]
        public void Should_schedule_future_set()
        {
            //arrange
            var scheduledSet = new ScheduledValue<int>(0);

            //act
            scheduledSet.Schedule(12, TimeSpan.FromSeconds(1));
            Assert.IsTrue(scheduledSet.IsScheduled);
            Thread.Sleep(TimeSpan.FromSeconds(2));

            //assert
            Assert.AreEqual(12, scheduledSet.Value);
            Assert.IsTrue(scheduledSet.WasSet);
            Assert.IsFalse(scheduledSet.IsScheduled);
        }

        [Test]
        public void Should_cancel_scheduled_set()
        {
            //arrange
            var scheduledSet = new ScheduledValue<int>(0);

            //act
            scheduledSet.Schedule(12, TimeSpan.FromSeconds(1));
            Assert.IsTrue(scheduledSet.IsScheduled);
            scheduledSet.Cancel();

            //assert
            Assert.IsFalse(scheduledSet.IsScheduled);
            Assert.IsFalse(scheduledSet.WasSet);
        }

        #endregion
    }
}
