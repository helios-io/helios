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
        public static IMessageDecoder DefaultDecoder
        {
            get
            {
                return LengthFieldFrameBasedDecoder.Default;
            }
        }



        /// <summary>
        /// The default encoder option
        /// </summary>
        public static IMessageEncoder DefaultEncoder
        {
            get { return LengthFieldPrepender.Default; }
        }
    }
}
