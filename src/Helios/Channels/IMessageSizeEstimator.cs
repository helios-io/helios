// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

namespace Helios.Channels
{
    public interface IMessageSizeEstimator
    {
        /// <summary>
        ///     Creates a new <see cref="IMessageSizeEstimatorHandle" />, which will perform all of the underlying work
        /// </summary>
        IMessageSizeEstimatorHandle NewHandle();
    }


    public interface IMessageSizeEstimatorHandle
    {
        /// <summary>
        ///     Estimsates the size, in bytes, of the underlying object.
        /// </summary>
        /// <param name="obj">The object whose size we're measuring.</param>
        /// <returns>The estimated length of the object in bytes.</returns>
        int Size(object obj);
    }
}