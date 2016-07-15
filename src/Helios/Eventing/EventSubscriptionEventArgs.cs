// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

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