// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;

namespace Helios.Eventing.Subscriptions
{
    /// <summary>
    ///     Extension method class used to help create EventBroker
    ///     topics from lambda methods
    /// </summary>
    public static class TopicHelpers
    {
        public static NormalTopicSubscription Subscription(this Action<object, EventArgs> h)
        {
            return new NormalTopicSubscription(h);
        }
    }
}