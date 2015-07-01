using System;
using System.Collections.Generic;

namespace Helios.Ops
{
    /// <summary>
    /// Returns meta-data used to process the results of critical operations, such
    /// as working with a service repository
    /// </summary>
    public interface IOperationResult
    {
        /// <summary>
        /// A status code representing the outcome of an operation
        /// </summary>
        int StatusCode { get; }

        /// <summary>
        /// true if the operation was successful, false otherwise
        /// </summary>
        bool IsSuccess { get; }

        /// <summary>
        /// A human-readable message describing the outcome of the operation
        /// </summary>
        string Message { get; }

        /// <summary>
        /// The time this operation was generated (in UTC)
        /// </summary>
        DateTimeOffset Generated { get; }

        /// <summary>
        /// A list of errors that occurred during the operation
        /// </summary>
        IDictionary<string, object> Errors { get; }
    }

    public interface IOperationResult<T> : IOperationResult
    {
        /// <summary>
        /// An object payload to return to the caller
        /// </summary>
        T Payload { get; set; }
    }
}
