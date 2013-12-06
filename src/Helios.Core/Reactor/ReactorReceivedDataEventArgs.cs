using System;
using Helios.Core.Net;

namespace Helios.Core.Reactor
{
    public class ReactorReceivedDataEventArgs : EventArgs
    {
        public IConnection ResponseChannel { get; protected set; }

        public NetworkData Data { get; protected set; }

        public static ReactorReceivedDataEventArgs Create(NetworkData data, IConnection connection)
        {
            return new ReactorReceivedDataEventArgs()
            {
               Data = data,
               ResponseChannel = connection
            };
        }
    }
}