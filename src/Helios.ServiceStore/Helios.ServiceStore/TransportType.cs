using System.ComponentModel;

namespace Helios.ServiceStore
{
    /// <summary>
    /// The type of transport used for a given node capability
    /// </summary>
    public enum TransportType
    {
        [Description("Tcp connection")]
        Tcp = 0, 
        [Description("Http connection")]
        Http = 1, 
        [Description("Udp connection")]
        Udp = 2
    }
}
