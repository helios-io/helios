using System;

namespace Helios.Eventing
{
    /// <summary>
    /// An interface for notifying classes inside a single process about 
    /// available events.
    /// 
    /// Truth be told - this class can be used over sockets and inter-process communication; it's
    /// a configuration detail.
    /// </summary>
    /// <typeparam name="TTopic">The type used to identify the topic</typeparam>
    /// <typeparam name="TSubscriber">The type used to identify the subscriber</typeparam>
    public interface IEventBroker<TTopic, TSubscriber>
    {
        event EventHandler<EventSubscriptionEventArgs<TTopic, TSubscriber>> SubscriptionAdded;
        event EventHandler<EventSubscriptionEventArgs<TTopic, TSubscriber>> SubscriptionRemoved;

        void Subscribe(TTopic id, TSubscriber subscriber, ITopicSubscription normalTopicSubscription);

        void Unsubscribe(TTopic id, TSubscriber subscriber);

        /// <summary>
        /// Fire the event with the topic specified by the identifier
        /// with its associated sender and parameters
        /// </summary>
        /// <param name="id">The id of the event to fire</param>
        /// <param name="sender">The object responsible for firing the event</param>
        /// <param name="e">The arguments for this event</param>
        void InvokeEvent(TTopic id, object sender, EventArgs e);
    }
}
