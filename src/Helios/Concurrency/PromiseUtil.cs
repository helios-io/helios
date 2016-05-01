using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helios.Logging;

namespace Helios.Concurrency
{
    /// <summary>
    /// Utility class for working with <see cref="TaskCompletionSource"/>
    /// </summary>
    internal static class PromiseUtil
    {
        public static void SafeSetSuccess(TaskCompletionSource promise, ILogger logger)
        {
            if (promise != TaskCompletionSource.Void && !promise.TryComplete())
            {
                logger.Warning("Failed to complete task successfully because it is done already: {0}", promise);
            }
        }

        public static void SafeSetFailure(TaskCompletionSource promise, Exception cause, ILogger logger)
        {
            if (promise != TaskCompletionSource.Void && !promise.TrySetException(cause))
            {
                logger.Warning("Failed to set exception on task successfully because it is done already: {0}; Cause: {1}", promise, cause);
            }
        }
    }
}
