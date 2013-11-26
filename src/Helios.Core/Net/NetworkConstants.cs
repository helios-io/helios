using System;

namespace Helios.Core.Net
{
    /// <summary>
    /// constants used by Helios during network operations
    /// </summary>
    public static class NetworkConstants
    {
        /// <summary>
        /// The default keep-alive interval used to see if our servers are still alive
        /// </summary>
        public static readonly TimeSpan DefaultHealthCheckPollingInterval = TimeSpan.FromSeconds(1);

        /// <summary>
        /// The default connectivity timeout
        /// </summary>
        public static readonly TimeSpan DefaultConnectivityTimeout = TimeSpan.FromSeconds(30);

        /// <summary>
        /// All of the exponential back off intervals used for checking the health
        /// of blacked-out nodes
        /// </summary>
        public static readonly TimeSpan[] BackoffIntervals =
            {
                TimeSpan.FromSeconds(5), //5 seconds
                TimeSpan.FromSeconds(30), //30 seconds
                TimeSpan.FromMinutes(5), //5 minutes
                TimeSpan.FromMinutes(15), //15 minutes
                TimeSpan.FromMinutes(30), //30 minutes
                TimeSpan.FromHours(1), //1 hour
                TimeSpan.FromHours(2), //2 hours
                TimeSpan.FromHours(4), //4 hours
                TimeSpan.FromHours(12), //12 hours
                TimeSpan.FromDays(1), //1 day
                TimeSpan.FromDays(2) //2 days
            };
    }
}
