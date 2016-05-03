using System.Net;
using System.Threading.Tasks;

namespace Helios.Channels.Bootstrap
{
    public class DefaultNameResolver : INameResolver
    {
        public bool IsResolved(EndPoint address)
        {
            return !(address is DnsEndPoint);
        }

        public async Task<EndPoint> ResolveAsync(EndPoint address)
        {
            var asDns = address as DnsEndPoint;
            if (asDns != null)
            {
                IPHostEntry resolved = await Dns.GetHostEntryAsync(asDns.Host);
                return new IPEndPoint(resolved.AddressList[0], asDns.Port);
            }
            else
            {
                return address;
            }
        }
    }
}