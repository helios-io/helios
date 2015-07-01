using System;

namespace Helios.Eventing
{
    public class EventSubscriptionEventArgs<TTopic, TSubscriber> : EventArgs
    {
        public EventSubscriptionEventArgs(TTopic topic, TSubscriber subscriber, int subscriberCount)
        {
            Topic = topic;
            SubscriberCount = subscriberCount;
        }

        public TTopic Topic { get; protected set; }

        public TSubscriber Subscriber { get; protected set; }

        public int SubscriberCount { get; protected set; }
    }
}
