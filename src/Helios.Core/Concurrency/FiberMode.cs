namespace Helios.Core.Concurrency
{
    public enum FiberMode
    {
        Synchronous,
        MultiThreaded,
        SingleThreaded,
        MaximumConcurrency
    };
}