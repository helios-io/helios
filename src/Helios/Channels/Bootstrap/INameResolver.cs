using System.Net;
using System.Threading.Tasks;

namespace Helios.Channels.Bootstrap
{
    public interface INameResolver
    {
        bool IsResolved(EndPoint address);

        Task<EndPoint> ResolveAsync(EndPoint address);
    }
}
