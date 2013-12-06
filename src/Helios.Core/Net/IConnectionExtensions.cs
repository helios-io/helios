using Helios.Net.Builders;
using Helios.Topology;

namespace Helios.Net
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
