// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

namespace Helios.Serialization
{
    /// <summary>
    ///     Static factory class for generating encoder instances
    /// </summary>
    public static class Encoders
    {
        /// <summary>
        ///     The default decoder option
        /// </summary>
        public static IMessageDecoder DefaultDecoder
        {
            get { return LengthFieldFrameBasedDecoder.Default; }
        }


        /// <summary>
        ///     The default encoder option
        /// </summary>
        public static IMessageEncoder DefaultEncoder
        {
            get { return LengthFieldPrepender.Default; }
        }
    }
}