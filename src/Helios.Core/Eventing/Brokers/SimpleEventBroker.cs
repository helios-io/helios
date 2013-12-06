using System;
using System.Collections.Generic;
using Helios.Ops;
using Helios.Ops.Executors;

namespace Helios.Eventing.Brokers
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

        protected IExecutor Executor;
        protected IDictionary<TTopic, IDictionary<TSubscriber, ITopicSubscription>> Subscribers;

        public SimpleEventBroker() : this(new BasicExecutor()){}

        public SimpleEventBroker(IExecutor executor)
        {
            Executor = executor;
            Subscribers = new Dictionary<TTopic, IDictionary<TSubscriber, ITopicSubscription>>();
        } 

        public void Subscribe(TTopic id, TSubscriber subscriber, ITopicSubscription normalTopicSubscription)
        {
            if (!Subscribers.ContainsKey(id))
            {
                Subscribers[id] = new Dictionary<TSubscriber, ITopicSubscription>();
            }

            if (Subscribers[id].ContainsKey(subscriber)) return;

            Subscribers[id].Add(subscriber, normalTopicSubscription);
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
                    Executor.Execute(() => h.Invoke(sender, e));
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