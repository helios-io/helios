using System;
using System.Collections.Generic;

namespace Helios.ServiceStore
{
    public class OperationResult : IOperationResult
    {
        public int StatusCode { get; private set; }
        public bool IsSuccess { get; private set; }
        public string Message { get; private set; }
        public DateTimeOffset Generated { get; private set; }
        public IDictionary<string, object> Errors { get; private set; }

        protected OperationResult()
        {
            Errors = new Dictionary<string, object>();
            Generated = DateTimeOffset.UtcNow;
        }

        public static OperationResult Create(int statusCode, bool isSuccess, string message)
        {
            return new OperationResult() {StatusCode = statusCode, IsSuccess = isSuccess, Message = message};
        }

        public static OperationResult<T> Create<T>(int statusCode, bool isSuccess, string message, T payload)
        {
            return new OperationResult<T>() { StatusCode = statusCode, IsSuccess = isSuccess, Message = message, Payload = payload};
        }
    }

    public class OperationResult<T> : OperationResult, IOperationResult<T>
    {
        public T Payload { get; set; }
    }
}