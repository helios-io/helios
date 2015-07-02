using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Helios.Util.TimedOps;
using NUnit.Framework;

namespace Helios.Tests.Util.TimedOps
{
    [TestFixture]
    public class DeadlineTests
    {
        #region Setup / Teardown
        #endregion

        #region Tests

        [Test]
        public void Deadlines_with_same_values_should_be_equal()
        {
            var deadline1 = Deadline.Now;
            var deadline2 = new Deadline(deadline1.When);
            Assert.True(deadline1 == deadline2);
        }

        [Test]
        public void Deadlines_should_be_hit_when_time_elapses()
        {
            var deadline1 = Deadline.Now + TimeSpan.FromSeconds(0.5);
            Assert.False(deadline1.IsOverdue); // not overdue yet
            Thread.Sleep(515);   
            Assert.True(deadline1.IsOverdue); // overdue
        }

        #endregion
    }
}
