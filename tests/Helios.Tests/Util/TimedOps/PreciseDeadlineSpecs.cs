using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Helios.Util.TimedOps;
using Xunit;

namespace Helios.Tests.Util.TimedOps
{
    public class PreciseDeadlineSpecs
    {
        [Fact]
        public void PreciseDeadline_should_correctly_report_overdue_time()
        {
            var deadline = new PreciseDeadline(TimeSpan.FromMilliseconds(20));
            Task.Delay(TimeSpan.FromMilliseconds(50)).Wait();
            Assert.True(deadline.IsOverdue);
        }

        [Fact]
        public void PrecideDeadline_from_addition_should_correctly_report_overduetime()
        {
            var timeout = TimeSpan.FromMilliseconds(20);
            var deadline = PreciseDeadline.Now + timeout;
            Assert.True(deadline > PreciseDeadline.Now);
            Assert.True(PreciseDeadline.Now < deadline);
            Task.Delay(TimeSpan.FromMilliseconds(50)).Wait();
            Assert.True(deadline.IsOverdue);
        }
    }
}
