using System;

namespace Helios.Core.Eventing
{
    /// <summary>
    /// A subscription object - exists primarily to make subscription callbacks
    /// refactorable in the future
    /// </summary>
    public interface ITopicSubscription
    {
        void Invoke();

        void Invoke(object sender, EventArgs e);
    }
}