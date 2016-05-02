namespace Helios.FsCheck.Tests.Channels.Sockets.Models
{
    public enum ConnectionState
    {
        Connecting = 1 << 1,
        Active = 1 << 2,
        Shutdown = 1 << 3
    };
}