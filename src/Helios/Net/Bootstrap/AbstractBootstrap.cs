using System.Net;
using Helios.Buffers;
using Helios.Serialization;

namespace Helios.Net.Bootstrap
{
    /// <summary>
    /// Base class for bootstrapping new <see cref="IConnection"/> objects
    /// </summary>
    public abstract class AbstractBootstrap
    {
        protected AbstractBootstrap()
        {
            Config = new DefaultConnectionConfig();
            Encoder = Encoders.DefaultEncoder;
            Decoder = Encoders.DefaultDecoder;
            Allocator = UnpooledByteBufAllocator.Default;
            Type = TransportType.Tcp;
        }

        protected AbstractBootstrap(AbstractBootstrap other) : this()
        {
            ReceivedData = other.ReceivedData != null ? (ReceivedDataCallback) other.ReceivedData.Clone() : null;
            ConnectionEstablishedCallback = other.ConnectionEstablishedCallback != null
                ? (ConnectionEstablishedCallback) other.ConnectionEstablishedCallback.Clone()
                : null;
            ConnectionTerminatedCallback = other.ConnectionTerminatedCallback != null
                ? (Net.ConnectionTerminatedCallback) other.ConnectionTerminatedCallback.Clone()
                : null;
            ExceptionCallback = other.ExceptionCallback != null
                ? (ExceptionCallback) other.ExceptionCallback.Clone()
                : null;

            foreach (var option in other.Config.Options)
            {
                Config.SetOption(option.Key, option.Value);
            }

            Encoder = other.Encoder;
            Decoder = other.Decoder;
        }

        /// <summary>
        /// Configuration to be used with the
        /// </summary>
        protected IHeliosConfig Config { get; set; }

        protected TransportType Type { get; set; }

        protected ReceivedDataCallback ReceivedData { get; set; }

        protected ConnectionEstablishedCallback ConnectionEstablishedCallback { get; set; }

        protected ConnectionTerminatedCallback ConnectionTerminatedCallback { get; set; }

        protected ExceptionCallback ExceptionCallback { get; set; }

        protected IMessageDecoder Decoder { get; set; }

        protected IMessageEncoder Encoder { get; set; }

        protected IByteBufAllocator Allocator { get; set; }

        public virtual AbstractBootstrap SetTransport(TransportType type)
        {
            Type = type;
            return this;
        }

        public virtual AbstractBootstrap SetDecoder(IMessageDecoder decoder)
        {
            Decoder = decoder;
            return this;
        }

        public virtual AbstractBootstrap SetEncoder(IMessageEncoder encoder)
        {
            Encoder = encoder;
            return this;
        }

        public virtual AbstractBootstrap SetAllocator(IByteBufAllocator allocator)
        {
            Allocator = allocator;
            return this;
        }

        public virtual AbstractBootstrap SetConfig(IHeliosConfig config)
        {
            Config = config;
            return this;
        }

        public virtual AbstractBootstrap SetOption(string optionKey, object optionValue)
        {
            Config = Config.SetOption(optionKey, optionValue);
            return this;
        }

        public virtual AbstractBootstrap OnReceive(ReceivedDataCallback receivedDataCallback)
        {
            ReceivedData = receivedDataCallback;
            return this;
        }

        public virtual AbstractBootstrap OnConnect(ConnectionEstablishedCallback connectionEstablishedCallback)
        {
            ConnectionEstablishedCallback = connectionEstablishedCallback;
            return this;
        }

        public virtual AbstractBootstrap OnDisconnect(ConnectionTerminatedCallback connectionTerminatedCallback)
        {
            ConnectionTerminatedCallback = connectionTerminatedCallback;
            return this;
        }

        public virtual AbstractBootstrap OnError(ExceptionCallback exceptionCallback)
        {
            ExceptionCallback = exceptionCallback;
            return this;
        }

        public abstract void Validate();

        protected abstract IConnectionFactory BuildInternal();

        public IConnectionFactory Build()
        {
            Validate();
            return BuildInternal();
        }

    }
}
