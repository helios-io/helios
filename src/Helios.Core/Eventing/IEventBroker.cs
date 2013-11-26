using System;
namespace Helios.Core.Eventing
{
    /// <summary>
    /// An interface for notifying classes inside a single process about 
    /// available events.
    /// 
    /// Truth be told - this class can be used over sockets and inter-process communication; it's
    /// a configuration detail.
    /// </summary>
    public interface IEventBroker<TKey>
    {
        event EventHandler<EventSubscriptionEventArgs<TKey>> SubscriptionAdded;
        event EventHandler<EventSubscriptionEventArgs<TKey>> SubscriptionRemoved;

        void Subscribe(TKey id, Delegate method);

        void Unsubscribe(TKey id, Delegate method);

        /// <summary>
        /// Fire the event specified by the identifier
        /// with its associated sender and parameters
        /// </summary>
        /// <param name="id">The id of the event to fire</param>
        /// <param name="sender">The object responsible for firing the event</param>
        /// <param name="e">The arguments for this event</param>
        void InvokeEvent(TKey id, object sender, EventArgs e);
    }
}
