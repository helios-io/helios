// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

namespace Helios.FsCheck.Tests.Buffers
{
    public static class BufferHelpers
    {
        public static string PrintByteArray(byte[] bytes)
        {
            return "byte[" + string.Join("|", bytes) + "]";
        }

        public static object PrintByteBufferItem(object item)
        {
            if (item is byte[])
            {
                var bytes = item as byte[];
                return PrintByteArray(bytes);
            }

            return item;
        }
    }
}