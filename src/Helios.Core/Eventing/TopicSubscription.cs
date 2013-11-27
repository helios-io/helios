using System;

namespace Helios.Core.Eventing
{
    /// <summary>
    /// A subscription object - exists primarily to make subscription callbacks
    /// refactorable in the future
    /// </summary>
    public class TopicSubscription
    {
        public TopicSubscription(Action<object, EventArgs> callback)
        {
            InternalCallback = callback;
        }

        protected Action<object, EventArgs> InternalCallback;

        public void Invoke(object sender, EventArgs e)
        {
            var h = InternalCallback;
            if (h == null) return;
            h.Invoke(sender, e);
        }
    }
}