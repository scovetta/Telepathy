namespace Microsoft.Hpc.Scheduler.Session.QueueAdapter.Client
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session.Interface;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.Scheduler.Session.QueueAdapter.DTO;

    public class BrokerLauncherCloudQueueClient : IBrokerLauncher
    {
        private readonly CloudQueueListener<BrokerLauncherCloudQueueResponseDto> listener;

        private readonly CloudQueueWriter<BrokerLauncherCloudQueueCmdDto> writer;

        private readonly ConcurrentDictionary<string, object> requestTrackDictionary = new ConcurrentDictionary<string, object>();

        // TODO: multi-client single broker launcher
        public BrokerLauncherCloudQueueClient(string connectionString)
        {
            CloudQueueSerializer serializer = new CloudQueueSerializer(BrokerLauncherCloudQueueCmdTypeBinder.Default);
            this.listener = new CloudQueueListener<BrokerLauncherCloudQueueResponseDto>(connectionString, CloudQueueConstants.BrokerLauncherResponseQueueName, serializer, this.ReceiveResponse);
            this.writer = new CloudQueueWriter<BrokerLauncherCloudQueueCmdDto>(connectionString, CloudQueueConstants.BrokerLauncherRequestQueueName, serializer);
            this.listener.StartListen();
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
                switch (item.CmdName)
                {
                    case nameof(this.Create):
                    case nameof(this.Attach):
                    case nameof(this.CreateDurable):
                        if (item.Response is BrokerInitializationResult rb && tcs is TaskCompletionSource<BrokerInitializationResult> tb)
                        {
                            tb.SetResult(rb);
                        }
                        else
                        {
                            throw new InvalidOperationException($"Response type mismatch for request {item.RequestId}");
                        }

                        break;
                    case nameof(this.PingBroker):
                    case nameof(this.PingBroker2):
                        if (item.Response is string rs && tcs is TaskCompletionSource<string> ts)
                        {
                            ts.SetResult(rs);
                        }
                        else
                        {
                            throw new InvalidOperationException($"Response type mismatch for request {item.RequestId}");
                        }

                        break;
                    case nameof(this.GetActiveBrokerIdList):
                        if (item.Response is int[] ria && tcs is TaskCompletionSource<int[]> tia)
                        {
                            tia.SetResult(ria);
                        }
                        else
                        {
                            throw new InvalidOperationException($"Response type mismatch for request {item.RequestId}");
                        }

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

        private Task<BrokerInitializationResult> CreateAsync(SessionStartInfoContract info, int sessionId)
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

        private Task<bool> PingBrokerAsync(int sessionID)
        {
            return this.StartRequestAsync<bool>(nameof(this.PingBroker), sessionID);
        }

        public string PingBroker2(int sessionID)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginPingBroker2(int sessionID, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public string EndPingBroker2(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public BrokerInitializationResult CreateDurable(SessionStartInfoContract info, int sessionId)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginCreateDurable(SessionStartInfoContract info, int sessionId, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public BrokerInitializationResult EndCreateDurable(IAsyncResult ar)
        {
            throw new NotImplementedException();
        }

        public BrokerInitializationResult Attach(int sessionId)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginAttach(int sessionId, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public BrokerInitializationResult EndAttach(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public void Close(int sessionId)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginClose(int sessionId, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public void EndClose(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public int[] GetActiveBrokerIdList()
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginGetActiveBrokerIdList(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public int[] EndGetActiveBrokerIdList(IAsyncResult result)
        {
            throw new NotImplementedException();
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