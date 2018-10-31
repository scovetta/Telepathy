namespace Microsoft.Hpc.Scheduler.Session.QueueAdapter.Client
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session.Interface;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.Scheduler.Session.QueueAdapter.DTO;
    using Microsoft.Hpc.Scheduler.Session.QueueAdapter.Interface;

    public class BrokerLauncherCloudQueueClient : IBrokerLauncher
    {
        private readonly IQueueListener<BrokerLauncherCloudQueueResponseDto> listener;

        private readonly IQueueWriter<BrokerLauncherCloudQueueCmdDto> writer;

        private readonly ConcurrentDictionary<string, object> requestTrackDictionary = new ConcurrentDictionary<string, object>();

        // TODO: multi-client single broker launcher
        public BrokerLauncherCloudQueueClient(string connectionString)
        {
            CloudQueueSerializer serializer = new CloudQueueSerializer(BrokerLauncherCloudQueueCmdTypeBinder.Default);
            this.listener = new CloudQueueListener<BrokerLauncherCloudQueueResponseDto>(connectionString, CloudQueueConstants.BrokerLauncherResponseQueueName, serializer, this.ReceiveResponse);
            this.writer = new CloudQueueWriter<BrokerLauncherCloudQueueCmdDto>(connectionString, CloudQueueConstants.BrokerLauncherRequestQueueName, serializer);
            this.listener.StartListen();
        }

        public BrokerLauncherCloudQueueClient(IQueueListener<BrokerLauncherCloudQueueResponseDto> listener, IQueueWriter<BrokerLauncherCloudQueueCmdDto> writer)
        {
            this.listener = listener;
            this.writer = writer;
            this.listener.MessageReceivedCallback = this.ReceiveResponse;
        }

        private async Task<T> StartRequestAsync<T>(string cmdName, params object[] parameter)
        {
            BrokerLauncherCloudQueueCmdDto cmd = new BrokerLauncherCloudQueueCmdDto(cmdName, parameter);
            await this.writer.WriteAsync(cmd);
            TaskCompletionSource<T> tsc = new TaskCompletionSource<T>();
            this.requestTrackDictionary.TryAdd(cmd.RequestId, tsc);
            return await tsc.Task;
        }

        private async Task ReceiveResponse(BrokerLauncherCloudQueueResponseDto item)
        {
            if (this.requestTrackDictionary.TryRemove(item.RequestId, out var tcs))
            {
                void SetResult<T>()
                {
                    if (item.Response is T r && tcs is TaskCompletionSource<T> t)
                    {
                        t.SetResult(r);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Response type mismatch for request {item.RequestId}");
                    }
                }

                switch (item.CmdName)
                {
                    case nameof(this.Create):
                    case nameof(this.Attach):
                    case nameof(this.CreateDurable):
                        SetResult<BrokerInitializationResult>();
                        break;
                    case nameof(this.PingBroker):
                        SetResult<bool>();
                        break;
                    case nameof(this.PingBroker2):
                        SetResult<string>();
                        break;
                    case nameof(this.GetActiveBrokerIdList):
                        SetResult<int[]>();
                        break;
                    case nameof(this.Close):
                        SetResult<object>();
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown cmd for request {item.RequestId}: {item.CmdName}");
                }
            }
            else
            {
                throw new InvalidOperationException($"Unknown request ID: {item.RequestId}");
            }
        }

        public BrokerInitializationResult Create(SessionStartInfoContract info, int sessionId)
        {
            return this.CreateAsync(info, sessionId).GetAwaiter().GetResult();
        }

        public IAsyncResult BeginCreate(SessionStartInfoContract info, int sessionId, AsyncCallback callback, object state)
        {
            return AsApm(this.CreateAsync(info, sessionId), callback, state);
        }

        public BrokerInitializationResult EndCreate(IAsyncResult ar)
        {
            return ((Task<BrokerInitializationResult>)ar).Result;
        }

        public Task<BrokerInitializationResult> CreateAsync(SessionStartInfoContract info, int sessionId)
        {
            return this.StartRequestAsync<BrokerInitializationResult>(nameof(this.Create), info, sessionId);
        }

        public bool PingBroker(int sessionID)
        {
            return this.PingBrokerAsync(sessionID).GetAwaiter().GetResult();
        }

        public IAsyncResult BeginPingBroker(int sessionID, AsyncCallback callback, object state)
        {
            return AsApm(this.PingBrokerAsync(sessionID), callback, state);
        }

        public bool EndPingBroker(IAsyncResult result)
        {
            return ((Task<bool>)result).Result;
        }

        public Task<bool> PingBrokerAsync(int sessionID)
        {
            return this.StartRequestAsync<bool>(nameof(this.PingBroker), sessionID);
        }

        public string PingBroker2(int sessionID)
        {
            return this.PingBroker2Async(sessionID).GetAwaiter().GetResult();
        }

        public IAsyncResult BeginPingBroker2(int sessionID, AsyncCallback callback, object state)
        {
            return AsApm(this.PingBroker2Async(sessionID), callback, state);
        }

        public string EndPingBroker2(IAsyncResult result)
        {
            return ((Task<string>)result).Result;
        }

        public Task<string> PingBroker2Async(int sessionID)
        {
            return this.StartRequestAsync<string>(nameof(this.PingBroker2), sessionID);
        }

        public BrokerInitializationResult CreateDurable(SessionStartInfoContract info, int sessionId)
        {
            return this.CreateDurableAsync(info, sessionId).GetAwaiter().GetResult();
        }

        public IAsyncResult BeginCreateDurable(SessionStartInfoContract info, int sessionId, AsyncCallback callback, object state)
        {
            return AsApm(this.CreateDurableAsync(info, sessionId), callback, state);
        }

        public BrokerInitializationResult EndCreateDurable(IAsyncResult ar)
        {
            return ((Task<BrokerInitializationResult>)ar).Result;
        }

        public Task<BrokerInitializationResult> CreateDurableAsync(SessionStartInfoContract info, int sessionId)
        {
            return this.StartRequestAsync<BrokerInitializationResult>(nameof(this.CreateDurable), info, sessionId);
        }

        public BrokerInitializationResult Attach(int sessionId)
        {
            return this.AttachAsync(sessionId).GetAwaiter().GetResult();
        }

        public IAsyncResult BeginAttach(int sessionId, AsyncCallback callback, object state)
        {
            return AsApm(this.AttachAsync(sessionId), callback, state);
        }

        public BrokerInitializationResult EndAttach(IAsyncResult result)
        {
            return ((Task<BrokerInitializationResult>)result).Result;
        }

        public Task<BrokerInitializationResult> AttachAsync(int sessionId)
        {
            return this.StartRequestAsync<BrokerInitializationResult>(nameof(this.Attach), sessionId);
        }

        public void Close(int sessionId)
        {
            this.CloseAsync(sessionId).GetAwaiter().GetResult();
        }

        public Task CloseAsync(int sessionId)
        {
            return this.CloseAsyncAux(sessionId);
        }

        private Task<object> CloseAsyncAux(int sessionId)
        {
            return this.StartRequestAsync<object>(nameof(this.Close), sessionId);
        }

        public IAsyncResult BeginClose(int sessionId, AsyncCallback callback, object state)
        {
            return AsApm(this.CloseAsyncAux(sessionId), callback, state);
        }

        public void EndClose(IAsyncResult result)
        {
            ((Task<object>)result).GetAwaiter().GetResult();
        }

        public int[] GetActiveBrokerIdList()
        {
            return this.GetActiveBrokerIdListAsync().GetAwaiter().GetResult();
        }

        public IAsyncResult BeginGetActiveBrokerIdList(AsyncCallback callback, object state)
        {
            return AsApm(this.GetActiveBrokerIdListAsync(), callback, state);
        }

        public int[] EndGetActiveBrokerIdList(IAsyncResult result)
        {
            return ((Task<int[]>)result).Result;
        }

        public Task<int[]> GetActiveBrokerIdListAsync()
        {
            return this.StartRequestAsync<int[]>(nameof(this.GetActiveBrokerIdList));
        }

        public static IAsyncResult AsApm<T>(Task<T> task, AsyncCallback callback, object state)
        {
            if (task == null)
            {
                throw new ArgumentNullException("task");
            }

            var tcs = new TaskCompletionSource<T>(state);
            task.ContinueWith(
                t =>
                    {
                        if (t.IsFaulted)
                        {
                            tcs.TrySetException(t.Exception.InnerExceptions);
                        }
                        else if (t.IsCanceled)
                        {
                            tcs.TrySetCanceled();
                        }
                        else
                        {
                            tcs.TrySetResult(t.Result);
                        }

                        if (callback != null)
                        {
                            callback(tcs.Task);
                        }
                    },
                TaskScheduler.Default);
            return tcs.Task;
        }
    }
}