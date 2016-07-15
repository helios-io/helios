// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Generic;

namespace Helios.Util
{
    /// <summary>
    ///     A static helper class for protecting against pesky null reference errors
    /// </summary>
    public static class NullGuard
    {
        public static void NotNull(this object obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
        }

        public static TOut NotNull<TIn, TOut>(this TIn obj, Func<TIn, TOut> nextOp)
            where TOut : class
            where TIn : class
        {
            if (obj == null)
                return default(TOut);
            return nextOp.Invoke(obj);
        }

        public static void NotNull<TIn>(this TIn obj, Action<TIn> nextOp) where TIn : class
        {
            if (obj == null)
                return;
            nextOp.Invoke(obj);
        }

        public static void InitializeIfNull<TIn>(this IEnumerable<TIn> obj, int initialSize = 10)
        {
            if (obj == null) obj = new List<TIn>(initialSize);
        }

        public static void InitializeIfNull<TIn>(this TIn obj, TIn defaultValue) where TIn : class
        {
            if (obj == null) obj = defaultValue;
        }
    }
}