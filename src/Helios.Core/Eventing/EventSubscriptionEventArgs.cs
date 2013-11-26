using System;

namespace Helios.Core.Eventing
{
    public class EventSubscriptionEventArgs<TKey> : EventArgs
    {
        public EventSubscriptionEventArgs(TKey topic, int subcribers)
        {
            Topic = topic;
            Subscribers = subcribers;
        }

        public TKey Topic { get; protected set; }

        public int Subscribers { get; protected set; }
    }
}
