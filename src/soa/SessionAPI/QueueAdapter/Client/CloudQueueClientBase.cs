// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.QueueAdapter.Client
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using Microsoft.Telepathy.Session.QueueAdapter.DTO;
    using Microsoft.Telepathy.Session.QueueAdapter.Interface;

    public abstract class CloudQueueClientBase : IDisposable
    {
        protected CloudQueueClientBase()
        {
            ResponsePool.OnMessagePut += this.CheckResponseFromPool;
        }

        protected CloudQueueClientBase(IQueueListener<CloudQueueResponseDto> listener, IQueueWriter<CloudQueueCmdDto> writer) : this()
        {
            this.Listener = listener;
            this.Writer = writer;
            this.Listener.MessageReceivedCallback = this.ReceiveResponse;
        }

        protected IQueueListener<CloudQueueResponseDto> Listener { get; set; }

        protected IQueueWriter<CloudQueueCmdDto> Writer { get; set; }

        private readonly ConcurrentDictionary<string, object> requestTrackDictionary = new ConcurrentDictionary<string, object>();

        private readonly Dictionary<string, Action<object, object>> responseTypeMapping = new Dictionary<string, Action<object, object>>();

        protected async Task<T> StartRequestAsync<T>(string cmdName, params object[] parameter)
        {
            CloudQueueCmdDto cmd = new CloudQueueCmdDto(cmdName, parameter);
            await this.Writer.WriteAsync(cmd);
            TaskCompletionSource<T> tsc = new TaskCompletionSource<T>();
            this.requestTrackDictionary.TryAdd(cmd.RequestId, tsc);
            return await tsc.Task;
        }

        protected async Task<object> StartRequestAsync(string cmdName, params object[] parameter)
        {
            CloudQueueCmdDto cmd = new CloudQueueCmdDto(cmdName, parameter);
            await this.Writer.WriteAsync(cmd);
            TaskCompletionSource<object> tsc = new TaskCompletionSource<object>();
            this.requestTrackDictionary.TryAdd(cmd.RequestId, tsc);
            return await tsc.Task;
        }

        protected async Task ReceiveResponse(CloudQueueResponseDto item)
        {
            if (this.requestTrackDictionary.TryRemove(item.RequestId, out var tcs))
            {
                if (this.responseTypeMapping.TryGetValue(item.CmdName, out var setRes))
                {
                    setRes(item.Response, tcs);
                }
                else
                {
                    throw new InvalidOperationException($"Unknown cmd for request {item.RequestId}: {item.CmdName}");
                }
            }
            else
            {
                Trace.TraceError($"Unknown request ID: {item.RequestId}");
                ResponsePool.Put(item);
            }
        }

        protected void CheckResponseFromPool(string requestId)
        {
            if (this.requestTrackDictionary.ContainsKey(requestId))
            {
                if (ResponsePool.TryGetAndRemove(requestId, out var item))
                {
                    if (this.requestTrackDictionary.TryRemove(item.RequestId, out var tcs))
                    {
                        if (this.responseTypeMapping.TryGetValue(item.CmdName, out var setRes))
                        {
                            setRes(item.Response, tcs);
                        }
                        else
                        {
                            throw new InvalidOperationException($"Unknown cmd for request {item.RequestId}: {item.CmdName}");
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException($"Task completion source for request {requestId} missing");
                    }
                }
            }
        }

        protected void RegisterResponseType<T>(string cmdName)
        {
            this.responseTypeMapping[cmdName] = this.SetResult<T>;
        }

        protected void RegisterResponseType(string cmdName)
        {
            this.responseTypeMapping[cmdName] = this.SetResult<object>;
        }

        private void SetResult<T>(object item, object tcs)
        {
            if (tcs is TaskCompletionSource<T> t)
            {
                if (item is T r)
                {
                    t.SetResult(r);
                }
                else
                {
                    throw new InvalidOperationException($"Response type mismatch.");
                }
            }
            else
            {
                throw new InvalidOperationException($"TaskCompletionSource type mismatch.");
            }
        }

        private void ReleaseUnmanagedResources()
        {
            ResponsePool.OnMessagePut -= this.CheckResponseFromPool;
        }

        protected virtual void Dispose(bool disposing)
        {
            this.ReleaseUnmanagedResources();
            if (disposing)
            {
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~CloudQueueClientBase()
        {
            this.Dispose(false);
        }
    }
}