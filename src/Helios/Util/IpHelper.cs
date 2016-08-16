using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Sockets;

namespace Helios.Util
{
    /// <summary>
    /// Used to help with determining the correct <see cref="AddressFamily"/> to specify
    /// when binding to a <see cref="Socket"/>.
    /// </summary>
    /// <remarks>
    /// Used to help resolve https://github.com/akkadotnet/akka.net/issues/2194 and 
    /// https://github.com/helios-io/helios/issues/118, which were caused by inconsistencies
    /// between .NET and Mono / Linux.
    /// </remarks>
    public static class IpHelper
    {
        /// <summary>
        /// Resolves the most specific <see cref="AddressFamily"/> for a given endpoint.
        /// </summary>
        /// <param name="ep">The <see cref="EndPoint"/> we're going to find a fit for.</param>
        /// <returns>The closest fitting InterNetwork <see cref="AddressFamily"/>.</returns>
        public static AddressFamily BestAddressFamily(this EndPoint ep)
        {
            Contract.Requires(ep != null);
            if (ep.AddressFamily == AddressFamily.Unspecified)
            {
                return Socket.OSSupportsIPv6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork;
            }

            return ep.AddressFamily;
        }
    }
}
