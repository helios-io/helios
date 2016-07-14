// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;

namespace Helios.Exceptions.Events
{
    /// <summary>
    ///     Event arguments used for topic subscriptions that subscribe to Exception Events
    /// </summary>
    public class ExceptionEventArgs : EventArgs
    {
        public ExceptionEventArgs(Exception ex)
        {
            Exception = ex;
        }

        public Exception Exception { get; protected set; }
    }
}