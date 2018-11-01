namespace Microsoft.Hpc.Scheduler.Session.QueueAdapter.Client
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session.QueueAdapter.DTO;
    using Microsoft.Hpc.Scheduler.Session.QueueAdapter.Interface;

    public class CloudQueueClientBase
    {
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
                throw new InvalidOperationException($"Unknown request ID: {item.RequestId}");
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
            if (item is T r && tcs is TaskCompletionSource<T> t)
            {
                t.SetResult(r);
            }
            else
            {
                throw new InvalidOperationException($"Response type mismatch.");
            }
        }
    }
}