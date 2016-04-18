namespace Helios.Channels
{
    public interface IMessageSizeEstimator
    {
        /// <summary>
        /// Creates a new <see cref="IMessageSizeEstimatorHandle"/>, which will perform all of the underlying work
        /// </summary>
        IMessageSizeEstimatorHandle NewHandle();
    }


    public interface IMessageSizeEstimatorHandle
    {
        /// <summary>
        /// Estimsates the size, in bytes, of the underlying object.
        /// </summary>
        /// <param name="obj">The object whose size we're measuring.</param>
        /// <returns>The estimated length of the object in bytes.</returns>
        int Size(object obj);
    }
}