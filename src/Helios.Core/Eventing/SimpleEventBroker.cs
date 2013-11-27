using System;
using System.Collections;
using System.Collections.Generic;

namespace Helios.Core.Eventing
{
    /// <summary>
    /// Basic implementation of an EventBroker - designed
    /// for just firing plain-old events without any fancy
    /// network / concurrency / blah blah
    /// </summary>
    public class SimpleEventBroker<TTopic, TSubscriber> : IEventBroker<TTopic, TSubscriber>
    {
        public event EventHandler<EventSubscriptionEventArgs<TTopic, TSubscriber>> SubscriptionAdded = delegate { };
        public event EventHandler<EventSubscriptionEventArgs<TTopic, TSubscriber>> SubscriptionRemoved = delegate { };

        protected IDictionary<TTopic, IDictionary<TSubscriber, TopicSubscription>> Subscribers;

        public SimpleEventBroker()
        {
            Subscribers = new Dictionary<TTopic, IDictionary<TSubscriber, TopicSubscription>>();
        } 

        public void Subscribe(TTopic id, TSubscriber subscriber, TopicSubscription topicSubscription)
        {
            if (!Subscribers.ContainsKey(id))
            {
                Subscribers[id] = new Dictionary<TSubscriber, TopicSubscription>();
            }

            if (Subscribers[id].ContainsKey(subscriber)) return;

            Subscribers[id].Add(subscriber, topicSubscription);
            InvokeSubscriptionAdded(new EventSubscriptionEventArgs<TTopic, TSubscriber>(id, subscriber, Subscribers[id].Count));
        }

        public void Unsubscribe(TTopic id, TSubscriber subscriber)
        {
            if (!Subscribers.ContainsKey(id) || !Subscribers[id].ContainsKey(subscriber)) return;
            Subscribers[id].Remove(subscriber);
            InvokeSubscriptionRemoved(new EventSubscriptionEventArgs<TTopic, TSubscriber>(id, subscriber, Subscribers[id].Count));
        }

        public void InvokeEvent(TTopic id, object sender, EventArgs e)
        {
            if (Subscribers.ContainsKey(id))
            {
                foreach (var subscriber in Subscribers[id].Values)
                {
                    var h = subscriber;
                    if (h == null) continue; //shouldn't happen, but in case any delegates have been GC-ed...
                    h.Invoke(sender, e);
                }
            }
        }

        #region Invokers

        private void InvokeSubscriptionAdded(EventSubscriptionEventArgs<TTopic, TSubscriber> eventSubscriptionEventArgs)
        {
            var h = SubscriptionAdded;
            if (h == null) return;
            h(this, eventSubscriptionEventArgs);
        }

        private void InvokeSubscriptionRemoved(EventSubscriptionEventArgs<TTopic, TSubscriber> eventSubscriptionEventArgs)
        {
            var h = SubscriptionRemoved;
            if (h == null) return;
            h(this, eventSubscriptionEventArgs);
        }

        #endregion
    }
}