// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;

namespace Helios.Eventing.Subscriptions
{
    /// <summary>
    ///     Basic implementation of a topic subscription
    /// </summary>
    public class NormalTopicSubscription : ITopicSubscription
    {
        protected Action<object, EventArgs> InternalCallback;

        public NormalTopicSubscription(Action<object, EventArgs> callback)
        {
            InternalCallback = callback;
        }

        public void Invoke()
        {
            Invoke(this, new EventArgs());
        }

        public void Invoke(object sender, EventArgs e)
        {
            var h = InternalCallback;
            if (h == null) return;
            h.Invoke(sender, e);
        }
    }
}