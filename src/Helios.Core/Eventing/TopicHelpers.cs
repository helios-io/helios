using System;

namespace Helios.Core.Eventing
{
    /// <summary>
    /// Extension method class used to help create EventBroker
    /// topics from lambda methods
    /// </summary>
    public static class TopicHelpers
    {
        public static TopicSubscription Subscription(this Action<object, EventArgs> h)
        {
            return new TopicSubscription(h);
        }
    }
}
