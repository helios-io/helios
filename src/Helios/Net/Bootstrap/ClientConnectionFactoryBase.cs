namespace Helios.Net.Bootstrap
{
    public abstract class ClientConnectionFactoryBase : ClientBootstrap, IConnectionFactory
    {
        protected ClientConnectionFactoryBase(ClientBootstrap clientBootstrap) : base(clientBootstrap) { }

        /// <summary>
        /// Spawns an <see cref="IConnection"/> object internally
        /// </summary>
        protected abstract IConnection CreateConnection();

        public IConnection NewConnection()
        {
            var connection = CreateConnection();
            connection.Configure(Config);

            if (ReceivedData != null)
                connection.Receive = (ReceivedDataCallback)ReceivedData.Clone();
            if (ConnectionEstablishedCallback != null)
                connection.OnConnection += (ConnectionEstablishedCallback)ConnectionEstablishedCallback.Clone();
            if (ConnectionTerminatedCallback != null)
                connection.OnDisconnection += (ConnectionTerminatedCallback)ConnectionTerminatedCallback.Clone();

            return connection;
        }
    }
}