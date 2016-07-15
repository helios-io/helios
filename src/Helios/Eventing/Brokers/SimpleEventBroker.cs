// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Helios.Concurrency;

namespace Helios.Eventing.Brokers
{
    /// <summary>
    ///     Basic implementation of an EventBroker - designed
    ///     for just firing plain-old events without any fancy
    ///     network / concurrency / blah blah
    /// </summary>
    /// <typeparam name="TTopic">The type used to identify a topic</typeparam>
    /// <typeparam name="TSubscriber">The type used to identify a subscriber</typeparam>
    public class SimpleEventBroker<TTopic, TSubscriber> : IEventBroker<TTopic, TSubscriber>
    {
        protected IFiber Executor;
        protected IDictionary<TTopic, IDictionary<TSubscriber, ITopicSubscription>> Subscribers;

        public SimpleEventBroker() : this(FiberFactory.CreateFiber(FiberMode.Synchronous))
        {
        }

        public SimpleEventBroker(IFiber executor)
        {
            Executor = executor;
            Subscribers = new ConcurrentDictionary<TTopic, IDictionary<TSubscriber, ITopicSubscription>>();
        }

        public event EventHandler<EventSubscriptionEventArgs<TTopic, TSubscriber>> SubscriptionAdded = delegate { };
        public event EventHandler<EventSubscriptionEventArgs<TTopic, TSubscriber>> SubscriptionRemoved = delegate { };

        public void Subscribe(TTopic id, TSubscriber subscriber, ITopicSubscription normalTopicSubscription)
        {
            if (!Subscribers.ContainsKey(id))
            {
                Subscribers[id] = new Dictionary<TSubscriber, ITopicSubscription>();
            }

            if (Subscribers[id].ContainsKey(subscriber)) return;

            Subscribers[id].Add(subscriber, normalTopicSubscription);
            InvokeSubscriptionAdded(new EventSubscriptionEventArgs<TTopic, TSubscriber>(id, subscriber,
                Subscribers[id].Count));
        }

        public void Unsubscribe(TTopic id, TSubscriber subscriber)
        {
            if (!Subscribers.ContainsKey(id) || !Subscribers[id].ContainsKey(subscriber)) return;
            Subscribers[id].Remove(subscriber);
            InvokeSubscriptionRemoved(new EventSubscriptionEventArgs<TTopic, TSubscriber>(id, subscriber,
                Subscribers[id].Count));
        }

        public void InvokeEvent(TTopic id, object sender, EventArgs e)
        {
            if (Subscribers.ContainsKey(id))
            {
                var subscribers = Subscribers[id].Values.ToArray();
                foreach (var subscriber in subscribers)
                {
                    var h = subscriber;
                    if (h == null) continue; //shouldn't happen, but in case any delegates have been GC-ed...
                    Executor.Add(() => h.Invoke(sender, e));
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

        private void InvokeSubscriptionRemoved(
            EventSubscriptionEventArgs<TTopic, TSubscriber> eventSubscriptionEventArgs)
        {
            var h = SubscriptionRemoved;
            if (h == null) return;
            h(this, eventSubscriptionEventArgs);
        }

        #endregion
    }
}