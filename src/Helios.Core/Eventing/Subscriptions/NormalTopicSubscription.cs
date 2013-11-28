using System;

namespace Helios.Core.Eventing.Subscriptions
{
    /// <summary>
    /// Basic implementation of a topic subscription
    /// </summary>
    public class NormalTopicSubscription : ITopicSubscription
    {
        public NormalTopicSubscription(Action<object, EventArgs> callback)
        {
            InternalCallback = callback;
        }

        protected Action<object, EventArgs> InternalCallback;

        public void Invoke()
        {
            Invoke(this, new EventArgs());
        }

        public void Invoke(object sender, EventArgs e)
        {
            var h = InternalCallback;
            if (h == null) return;
            h.Invoke(sender, e);
        }
    }
}