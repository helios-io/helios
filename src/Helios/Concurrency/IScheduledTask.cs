using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Helios.Util.TimedOps;

namespace Helios.Concurrency
{
    public interface IScheduledTask
    {
        bool Cancel();

        PreciseDeadline Deadline { get; }

        Task Completion { get; }

        TaskAwaiter GetAwaiter();
    }
}
