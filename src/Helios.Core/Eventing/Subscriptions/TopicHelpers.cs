using System;

namespace Helios.Core.Eventing.Subscriptions
{
    /// <summary>
    /// Extension method class used to help create EventBroker
    /// topics from lambda methods
    /// </summary>
    public static class TopicHelpers
    {
        public static NormalTopicSubscription Subscription(this Action<object, EventArgs> h)
        {
            return new NormalTopicSubscription(h);
        }
    }
}
