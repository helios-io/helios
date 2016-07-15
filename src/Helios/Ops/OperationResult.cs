// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Generic;

namespace Helios.Ops
{
    public class OperationResult : IOperationResult
    {
        protected OperationResult()
        {
            Errors = new Dictionary<string, object>();
            Generated = DateTimeOffset.UtcNow;
        }

        public int StatusCode { get; private set; }
        public bool IsSuccess { get; private set; }
        public string Message { get; private set; }
        public DateTimeOffset Generated { get; }
        public IDictionary<string, object> Errors { get; }

        public static OperationResult Create(int statusCode, bool isSuccess, string message)
        {
            return new OperationResult {StatusCode = statusCode, IsSuccess = isSuccess, Message = message};
        }

        public static OperationResult<T> Create<T>(int statusCode, bool isSuccess, string message, T payload)
        {
            return new OperationResult<T>
            {
                StatusCode = statusCode,
                IsSuccess = isSuccess,
                Message = message,
                Payload = payload
            };
        }
    }

    public class OperationResult<T> : OperationResult, IOperationResult<T>
    {
        public T Payload { get; set; }
    }
}