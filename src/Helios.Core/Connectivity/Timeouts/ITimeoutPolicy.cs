namespace Helios.Core.Connectivity.Timeouts
{
    /// <summary>
    /// Interface used to describe timeout policies and rules
    /// </summary>
    public interface ITimeoutPolicy
    {
        /// <summary>
        /// The permitted timeout window in seconds
        /// </summary>
        int TimeoutSeconds { get; }
    }
}
