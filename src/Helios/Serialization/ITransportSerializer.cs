// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.IO;
using Helios.Net;

namespace Helios.Serialization
{
    /// <summary>
    ///     A binary serializer interface for working with messages
    ///     sent over IConnection and ITransport objects
    /// </summary>
    public interface ITransportSerializer
    {
        bool TryDeserialize<T>(out T obj, Stream stream);

        bool TryDeserialize<T>(out T obj, NetworkData data);

        T Deserialize<T>(Stream stream);

        T Deserialize<T>(NetworkData data);

        void Serialize<T>(T obj, Stream stream);

        void Serialize<T>(T obj, NetworkData data);
    }
}