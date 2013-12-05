using Helios.Core.Net.Builders;
using Helios.Core.Topology;

namespace Helios.Core.Net
{
    /// <summary>
    /// Extension methods using a default connection builder, to help make it easier to establish ad-hoc connections with INode instances
    /// </summary>
    public static class IConnectionExtensions
    {
        public static IConnectionBuilder DefaultConnectionBuilder = new NormalConnectionBuilder(NetworkConstants.DefaultConnectivityTimeout);

        public static IConnection GetConnection(this INode node)
        {
            return DefaultConnectionBuilder.BuildConnection(node);
        }
    }
}
