// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.IO;

namespace Helios.Net
{
    /// <summary>
    ///     Extension methods for working with NetworkData objects - deals primarily with Stream conversion
    /// </summary>
    public static class NetworkDataExtensions
    {
        public static Stream ToStream(this NetworkData nd)
        {
            return new MemoryStream(nd.Buffer, 0, nd.Length);
        }
    }
}