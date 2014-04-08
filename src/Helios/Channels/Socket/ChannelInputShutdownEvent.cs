namespace Helios.Channels.Socket
{
    /// <summary>
    /// User event that lets our channel handler know that we're shutting down
    /// </summary>
    public sealed class ChannelInputShutdownEvent
    {
        public static readonly ChannelInputShutdownEvent Instance = new ChannelInputShutdownEvent();

        private ChannelInputShutdownEvent() { }
    }
}
