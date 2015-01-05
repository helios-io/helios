namespace Helios.Channel
{
    /// <summary>
    /// Configuration object for <see cref="IChannel"/> instances.
    /// </summary>
    public class ChannelConfig : DefaultHeliosConfig
    {
        /// <summary>
        /// Keywords for all built-in configuration options
        /// </summary>
        internal static class ChannelConfigOptionNames
        {
            public const string AutoRead = "CHANNEL_AUTO_READ";

        }

        /// <summary>
        /// Sets if <see cref="IChannelHandlerContext.Read"/> will be invoked automatically so that
        /// a userapplication doesn't need to call it repeatedly. The default value is true.
        /// </summary>
        /// <param name="autoRead">true if autoread should be turned on.</param>
        /// <returns>An updated <see cref="ChannelConfig"/> instance.</returns>
        public ChannelConfig SetAutoRead(bool autoRead)
        {
            SetOption(ChannelConfigOptionNames.AutoRead, autoRead);
            return this;
        }

        /// <summary>
        /// Gets the AutoRead option for this <see cref="IChannel"/>.
        /// 
        /// If set to true, <see cref="IChannelHandlerContext.Read"/> will be invoked automatically so that
        /// a userapplication doesn't need to call it repeatedly. The default value is true.
        /// </summary>
        public bool AutoRead
        {
            get { return GetOption<bool>(ChannelConfigOptionNames.AutoRead); }
        }
    }
}
