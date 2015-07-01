using System;

namespace Helios.Exceptions.Events
{
    /// <summary>
    /// Event arguments used for topic subscriptions that subscribe to Exception Events
    /// </summary>
    public class ExceptionEventArgs : EventArgs
    {
        public ExceptionEventArgs(Exception ex)
        {
            Exception = ex;
        }

        public Exception Exception { get; protected set; }
    }
}
