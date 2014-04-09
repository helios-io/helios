using Helios.Topology;

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

            foreach (var option in other.Config.Options)
            {
                Config.SetOption(option.Key, option.Value);
            }
        }

        /// <summary>
        /// Configuration to be used with the
        /// </summary>
        protected IConnectionConfig Config { get; set; }

        protected ReceivedDataCallback ReceivedData { get; set; }

        protected ConnectionEstablishedCallback ConnectionEstablishedCallback { get; set; }

        protected ConnectionTerminatedCallback ConnectionTerminatedCallback { get; set; }

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

        public abstract void Validate();

        protected abstract IConnectionFactory BuildInternal();

        public IConnectionFactory Build()
        {
            Validate();
            return BuildInternal();
        }

    }
}
