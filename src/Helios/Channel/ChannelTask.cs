using System;
using System.Threading.Tasks;

namespace Helios.Channel
{
    public class ChannelTask : Task
    {
        public ChannelTask(IChannel channel, Action action) : base(action)
        {
            Channel = channel;
        }

        /// <summary>
        /// The <see cref="IChannel"/> that this task is tied to.
        /// </summary>
        public IChannel Channel { get; private set; }
    }
}
