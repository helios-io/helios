namespace Helios.Serialization
{
    /// <summary>
    /// Static factory class for generating encoder instances
    /// </summary>
    public static class Encoders
    {
        /// <summary>
        /// The default decoder option
        /// </summary>
        public static readonly IMessageDecoder DefaultDecoder = new LengthFieldFrameBasedDecoder(128000, 0, 4,0,4,true);

        /// <summary>
        /// The default encoder option
        /// </summary>
        public static readonly IMessageEncoder DefaultEncoder =new LengthFieldPrepender(4, false);
    }
}
