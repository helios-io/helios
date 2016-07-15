// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using Helios.Eventing;
using Helios.Eventing.Brokers;
using Helios.Eventing.Subscriptions;
using Xunit;

namespace Helios.Tests.Eventing
{
    /// <summary>
    ///     Tests for validating the approach of our SimpleEventBroker implementation
    /// </summary>
    public class SimpleEventBrokerTests
    {
        #region Setup / Teardown

        private readonly IEventBroker<int, int> eventBroker;

        public class SampleEventBrokerSubscriber
        {
            public bool ReceivedEvent { get; set; }
        }

        public SimpleEventBrokerTests()
        {
            eventBroker = new SimpleEventBroker<int, int>();
        }

        #endregion

        #region Tests

        [Fact]
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

            eventBroker.Subscribe(0, subscriber1.GetHashCode(), new NormalTopicSubscription((o, e) => { }));
            eventBroker.Unsubscribe(0, subscriber1.GetHashCode());

            //assert
            Assert.Equal(0, subscriberCount);
            Assert.Equal(2, changes);
        }

        [Fact]
        public void Should_notify_subscribers()
        {
            //arrange
            var subscriber1 = new SampleEventBrokerSubscriber();
            var subscriber2 = new SampleEventBrokerSubscriber();

            //act
            eventBroker.Subscribe(0, subscriber1.GetHashCode(),
                new NormalTopicSubscription((o, e) => { subscriber1.ReceivedEvent = true; }));
            eventBroker.Subscribe(0, subscriber2.GetHashCode(),
                new NormalTopicSubscription((o, e) => { subscriber2.ReceivedEvent = true; }));
            eventBroker.InvokeEvent(0, this, new EventArgs());

            //assert
            Assert.True(subscriber1.ReceivedEvent);
            Assert.True(subscriber2.ReceivedEvent);
        }

        [Fact]
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
            Assert.Equal(3, invoked);
            Assert.True(subscriber1.ReceivedEvent);
            Assert.True(subscriber2.ReceivedEvent);
            Assert.False(subscriber3.ReceivedEvent);
            Assert.True(subscriber4.ReceivedEvent);
        }

        #endregion
    }
}