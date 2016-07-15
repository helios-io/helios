// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using Helios.Eventing;

namespace Helios.Exceptions.Events
{
    /// <summary>
    ///     Used by internal event brokers for routing events
    /// </summary>
    public class ExceptionTopicSubscription : ITopicSubscription
    {
        protected readonly Action<ExceptionEventArgs> _callback;

        public ExceptionTopicSubscription(Action<ExceptionEventArgs> callback)
        {
            _callback = callback;
        }

        public void Invoke()
        {
            //Do nothing
        }

        public void Invoke(object sender, EventArgs e)
        {
            _callback((ExceptionEventArgs) e);
        }
    }
}