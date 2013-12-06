using System;
using Helios.Eventing;

namespace Helios.Exceptions.Events
{
    /// <summary>
    /// Used by internal event brokers for routing events
    /// </summary>
    public class ExceptionTopicSubscription : ITopicSubscription
    {
        protected readonly Action<ExceptionEventArgs> _callback;

        public ExceptionTopicSubscription(Action<ExceptionEventArgs> callback)
        {
            _callback = callback;
        }

        public void Invoke()
        {
            //Do nothing
        }

        public void Invoke(object sender, EventArgs e)
        {
            _callback((ExceptionEventArgs) e);
        }
    }
}
