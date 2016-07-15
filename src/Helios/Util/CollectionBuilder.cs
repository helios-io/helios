// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Generic;

namespace Helios.Util
{
    /// <summary>
    ///     Utility class for synthesizing arrays
    /// </summary>
    public static class CollectionBuilder
    {
        /// <summary>
        ///     Create N number of instances of this object with <see cref="initialValue" />
        /// </summary>
        /// <typeparam name="TOut">The type of object we want to produce</typeparam>
        /// <param name="i">The number of objects we want</param>
        /// <param name="initialValue">The initial value to set each object in the collection</param>
        /// <returns>An IEnumerable
        ///     <typeparam name="TOut"></typeparam>
        ///     with
        ///     <para>i</para>
        ///     instances set to value
        ///     <para>initialValue</para>
        /// </returns>
        public static IEnumerable<TOut> Of<TOut>(this int i, TOut initialValue)
        {
            i.NotLessThan(0);

            var output = new List<TOut>(i);
            for (var n = 0; n < i; n++)
            {
                output.Add(initialValue);
            }
            return output;
        }

        /// <summary>
        ///     Create N number of instances of this object by invoking <see cref="generator" /> method N times
        /// </summary>
        /// <typeparam name="TOut">The type of object we want to produce</typeparam>
        /// <param name="i">The number of objects we want</param>
        /// <param name="generator">A function that produces the value we want</param>
        /// <returns>An IEnumerable
        ///     <typeparam name="TOut"></typeparam>
        ///     with
        ///     <para>i</para>
        ///     instances set to value
        ///     <para>initialValue</para>
        /// </returns>
        public static IEnumerable<TOut> Times<TOut>(this int i, Func<TOut> generator)
        {
            i.NotLessThan(0);
            var output = new List<TOut>(i);
            for (var n = 0; n < i; n++)
            {
                output.Add(generator());
            }
            return output;
        }
    }
}