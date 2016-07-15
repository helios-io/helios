// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Util
{
    public static class BitOps
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RightUShift(this int value, int bits) => unchecked((int) ((uint) value >> bits));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long RightUShift(this long value, int bits) => unchecked((long) ((ulong) value >> bits));
    }
}