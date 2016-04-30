namespace Helios.Tests.Performance.Channels
{
    public interface IReadFinishedSignal
    {
        void Signal();
        bool Finished { get; }
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