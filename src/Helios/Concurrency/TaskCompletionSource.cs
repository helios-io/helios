using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Concurrency
{
    /// <summary>
    /// Non-generic implementation of a <see cref="TaskCompletionSource{TResult}"/>
    /// </summary>
    public sealed class TaskCompletionSource : TaskCompletionSource<int>
    {
        public static readonly TaskCompletionSource Void = CreateVoidTcs();

        public TaskCompletionSource() { } 

        public TaskCompletionSource(object state)
            : base(state)
        { }

        public bool TryComplete()
        {
            return this.TrySetResult(0);
        }

        public void Complete()
        {
            this.SetResult(0);
        }

        public bool SetUncancellable()
        {
            return true;
        }

        public override string ToString()
        {
            return "TaskCompletionSource[status: " + this.Task.Status.ToString() + "]";
        }

        static TaskCompletionSource CreateVoidTcs()
        {
            var tcs = new TaskCompletionSource();
            tcs.TryComplete();
            return tcs;
        }
    }
}
