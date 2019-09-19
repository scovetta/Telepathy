// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session.QueueAdapter.Client
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session.QueueAdapter.DTO;

    /// <summary>
    /// If multiple clients are listening to a single Cloud Queue for responses,
    /// they can get other clients' response by mistake. The pool is used to
    /// collect such responses, and notify other clients to check.
    /// </summary>
    public class ResponsePool
    {
        private ResponsePool()
        {
        }

        // public static ResponsePool Instance { get; } = new ResponsePool();

        private static readonly ConcurrentDictionary<string, CloudQueueResponseDto> responsePool = new ConcurrentDictionary<string, CloudQueueResponseDto>();

        public static void Put(CloudQueueResponseDto item)
        {
            if (responsePool.TryAdd(item.RequestId, item))
            {
                Trace.TraceInformation($"[{nameof(ResponsePool)}] response with ID {item.RequestId} added.");
                OnMessagePut?.Invoke(item.RequestId);
            }
            else
            {
                Trace.TraceError($"[{nameof(ResponsePool)}] response with ID {item.RequestId} already existed. Discard new value.");
            }
        }

        public static bool TryGetAndRemove(string requestId, out CloudQueueResponseDto item)
        {
            if (responsePool.TryRemove(requestId, out item))
            {
                Trace.TraceInformation($"[{nameof(ResponsePool)}] response with ID {item.RequestId} popped.");
                return true;
            }
            else
            {
                item = null;
                Trace.TraceWarning($"[{nameof(ResponsePool)}] response with ID {item.RequestId} doesn't exist.");
                return false;
            }
        }

        public delegate void OnMessagePutDelegate(string requestId);

        public static event OnMessagePutDelegate OnMessagePut;
    }
}
