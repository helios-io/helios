// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Linq;

namespace Helios.Util
{
    /// <summary>
    ///     Guard class for protecting against stupid input
    /// </summary>
    public static class Guard
    {
        public static void Against(this string str, string illegalSubstr, string message = null)
        {
            if (str.Contains(illegalSubstr))
                throw new ArgumentException(message ??
                                            string.Format("{0} is illegal in your input string {1}", illegalSubstr, str));
        }

        public static void Against(this string str, char illegalChar, string message = null)
        {
            if (str.Any(c => c == illegalChar))
                throw new ArgumentException(message ??
                                            string.Format("{0} is illegal in your input string {1}", illegalChar, str));
        }

        public static void NotNegative(this int value)
        {
            NotLessThan(value, 0);
        }

        public static void NotLessThan(this int value, int minimumValue)
        {
            if (value < minimumValue)
                throw new ArgumentOutOfRangeException("value",
                    string.Format("Value was {0} - cannot be less than {1}!", value, minimumValue));
        }

        public static void NotGreaterThan(this int value, int maximumValue)
        {
            if (value > maximumValue)
                throw new ArgumentOutOfRangeException("value",
                    string.Format("Value was {0} - cannot be greater than {1}!", value, maximumValue));
        }

        public static void True(bool boolean, string errorMessage = "Expression should be true, but was false")
        {
            if (!boolean)
                throw new ArgumentException(errorMessage);
        }
    }
}