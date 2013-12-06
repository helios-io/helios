using System;
using Helios.Eventing;
using Helios.Eventing.Brokers;
using Helios.Eventing.Subscriptions;
using NUnit.Framework;

namespace Helios.Tests.Eventing
{
    /// <summary>
    /// Tests for validating the approach of our SimpleEventBroker implementation
    /// </summary>
    [TestFixture]
    public class SimpleEventBrokerTests
    {
        #region Setup / Teardown

        private IEventBroker<int, int> eventBroker;

        public class SampleEventBrokerSubscriber
        {
            public bool ReceivedEvent { get; set; }
        }

        [SetUp]
        public void SetUp()
        {
            eventBroker = new SimpleEventBroker<int, int>();
        }

        #endregion

        #region Tests

        [Test(Description = "Should fire its notification event for when we successfully add / remove subscribers")]
        public void Should_notify_changes_in_subscribers()
        {
            //arrange
            var subscriberCount = 0;
            var changes = 0;
            var subscriber1 = new SampleEventBrokerSubscriber();
            
            //act
            eventBroker.SubscriptionAdded += (sender, args) =>
            {
                subscriberCount = args.SubscriberCount;
                changes++;
            };

            eventBroker.SubscriptionRemoved += (sender, args) =>
            {
                subscriberCount = args.SubscriberCount;
                changes++;
            };

            eventBroker.Subscribe(0, subscriber1.GetHashCode(), new NormalTopicSubscription((o,e)=> { }));
            eventBroker.Unsubscribe(0, subscriber1.GetHashCode());

            //assert
            Assert.AreEqual(0, subscriberCount);
            Assert.AreEqual(2, changes);
        }

        [Test(Description = "Should be able to notify all subscribers when an update happens to a topic")]
        public void Should_notify_subscribers()
        {
            //arrange
            var subscriber1 = new SampleEventBrokerSubscriber();
            var subscriber2 = new SampleEventBrokerSubscriber();

            //act
            eventBroker.Subscribe(0, subscriber1.GetHashCode(), new NormalTopicSubscription((o, e) =>
            {
                subscriber1.ReceivedEvent = true;
            }));
            eventBroker.Subscribe(0, subscriber2.GetHashCode(), new NormalTopicSubscription((o, e) =>
            {
                subscriber2.ReceivedEvent = true;
            }));
            eventBroker.InvokeEvent(0, this, new EventArgs());

            //assert
            Assert.IsTrue(subscriber1.ReceivedEvent);
            Assert.IsTrue(subscriber2.ReceivedEvent);
        }

        [Test(Description = "Should only notify subscribers who are on the relevant topic")]
        public void Should_only_notify_subscribers_on_relevant_topic()
        {
            //arrange
            var invoked = 0;
            var subscriber1 = new SampleEventBrokerSubscriber();
            var subscriber2 = new SampleEventBrokerSubscriber();
            var subscriber3 = new SampleEventBrokerSubscriber();
            var subscriber4 = new SampleEventBrokerSubscriber();

            
            eventBroker.Subscribe(0, subscriber1.GetHashCode(), new NormalTopicSubscription((o, e) =>
            {
                subscriber1.ReceivedEvent = true;
                invoked++;
            }));
            eventBroker.Subscribe(0, subscriber2.GetHashCode(), new NormalTopicSubscription((o, e) =>
            {
                subscriber2.ReceivedEvent = true;
                invoked++;
            }));

            eventBroker.Subscribe(2, subscriber3.GetHashCode(), new NormalTopicSubscription((o, e) =>
            {
                subscriber3.ReceivedEvent = true;
                invoked++;
            }));
            eventBroker.Subscribe(1, subscriber4.GetHashCode(), new NormalTopicSubscription((o, e) =>
            {
                subscriber4.ReceivedEvent = true;
                invoked++;
            }));

            //act
            eventBroker.InvokeEvent(1, this, new EventArgs());
            eventBroker.InvokeEvent(0, this, new EventArgs());

            //assert
            Assert.AreEqual(3, invoked);
            Assert.IsTrue(subscriber1.ReceivedEvent);
            Assert.IsTrue(subscriber2.ReceivedEvent);
            Assert.IsFalse(subscriber3.ReceivedEvent);
            Assert.IsTrue(subscriber4.ReceivedEvent);
        }

        #endregion
    }
}
