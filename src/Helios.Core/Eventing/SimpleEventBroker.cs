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
    /// <typeparam name="TKey">The type of the key used for identifying the topic (usually a string or Guid)</typeparam>
    public class SimpleEventBroker<TKey> : IEventBroker<TKey>
    {
        public event EventHandler<EventSubscriptionEventArgs<TKey>> SubscriptionAdded = delegate { };
        public event EventHandler<EventSubscriptionEventArgs<TKey>> SubscriptionRemoved = delegate { };

        protected readonly IDictionary<TKey, HashSet<Delegate>> Subscribers;

        public SimpleEventBroker()
        {
            Subscribers = new Dictionary<TKey, HashSet<Delegate>>();
        } 

        public void Subscribe(TKey id, Delegate method)
        {
            if (!Subscribers.ContainsKey(id))
            {
                Subscribers[id] = new HashSet<Delegate>();
            }

            Subscribers[id].Add(method);
            InvokeSubscriptionAdded(new EventSubscriptionEventArgs<TKey>(id, Subscribers[id].Count));
        }

        public void Unsubscribe(TKey id, Delegate method)
        {
            if (Subscribers.ContainsKey(id))
            {
                if (Subscribers[id].Contains(method))
                {
                    Subscribers[id].Remove(method);
                    InvokeSubscriptionRemoved(new EventSubscriptionEventArgs<TKey>(id, Subscribers[id].Count));
                }

                //No one is subscribed to this topic any more
                if (Subscribers[id].Count == 0)
                    Subscribers.Remove(id);
            }
        }

        public void InvokeEvent(TKey id, object sender, EventArgs e)
        {
            if (Subscribers.ContainsKey(id))
            {
                foreach (var subscriber in Subscribers[id])
                {
                    var h = subscriber;
                    if(h == null || h.Method == null || h.Target == null) continue; //shouldn't happen, but in case any delegates have been GC-ed...
                    h.DynamicInvoke(sender, e);
                }
            }
        }

        #region Invokers

        public void InvokeSubscriptionAdded(EventSubscriptionEventArgs<TKey> e)
        {
            var h = SubscriptionAdded;
            if (h == null) return;
            h(this, e);
        }

        public void InvokeSubscriptionRemoved(EventSubscriptionEventArgs<TKey> e)
        {
            var h = SubscriptionRemoved;
            if (h == null) return;
            h(this, e);
        }

        #endregion
    }
}