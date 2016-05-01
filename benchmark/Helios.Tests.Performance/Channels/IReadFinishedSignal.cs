using System.Threading;
using Helios.Concurrency;

namespace Helios.Tests.Performance.Channels
{
    public interface IReadFinishedSignal
    {
        void Signal();
        bool Finished { get; }
    }

    public class ManualResetEventSlimReadFinishedSignal : IReadFinishedSignal
    {
        private readonly ManualResetEventSlim _manualResetEventSlim;

        public ManualResetEventSlimReadFinishedSignal(ManualResetEventSlim manualResetEventSlim)
        {
            _manualResetEventSlim = manualResetEventSlim;
        }

        public void Signal()
        {
            _manualResetEventSlim.Set();
        }

        public bool Finished => _manualResetEventSlim.IsSet;
    }

    public class TaskCompletionSourceFinishedSignal : IReadFinishedSignal
    {
        private readonly TaskCompletionSource _tcs;

        public TaskCompletionSourceFinishedSignal(TaskCompletionSource tcs)
        {
            _tcs = tcs;
        }

        public void Signal()
        {
            _tcs.TryComplete();
        }

        public bool Finished => _tcs.Task.IsCompleted;
    }

    public class SimpleReadFinishedSignal : IReadFinishedSignal
    {
        public void Signal()
        {
            Finished = true;
        }

        public bool Finished { get; private set; }
    }
}