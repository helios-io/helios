using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Concurrency
{
    /// <summary>
    /// Any aynchronous operation, whether it be a delegate, <see cref="Task"/>, etc, which will
    /// be executed later by an <see cref="IEventExecutor"/>
    /// </summary>
    public interface IRunnable
    {
        void Run();
    }
}
