// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session.QueueAdapter.Client
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session.Interface;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.Scheduler.Session.QueueAdapter.DTO;
    using Microsoft.Hpc.Scheduler.Session.QueueAdapter.Interface;

    public class BrokerLauncherCloudQueueClient : CloudQueueClientBase, IBrokerLauncher
    {
        // TODO: multi-client single broker launcher
        public BrokerLauncherCloudQueueClient(string connectionString)
        {
            CloudQueueSerializer serializer = new CloudQueueSerializer(CloudQueueCmdTypeBinder.BrokerLauncherBinder);
            this.Listener = new CloudQueueListener<CloudQueueResponseDto>(connectionString, CloudQueueConstants.BrokerLauncherResponseQueueName, serializer, this.ReceiveResponse);
            this.Writer = new CloudQueueWriter<CloudQueueCmdDto>(connectionString, CloudQueueConstants.BrokerLauncherRequestQueueName, serializer);
            this.Listener.StartListen();
            this.RegisterResponseTypes();
        }

        public BrokerLauncherCloudQueueClient(IQueueListener<CloudQueueResponseDto> listener, IQueueWriter<CloudQueueCmdDto> writer) :base(listener, writer)
        {
            this.RegisterResponseTypes();
        }

        private void RegisterResponseTypes()
        {
            this.RegisterResponseType<BrokerInitializationResult>(nameof(this.Create));
            this.RegisterResponseType<BrokerInitializationResult>(nameof(this.Attach));
            this.RegisterResponseType<BrokerInitializationResult>(nameof(this.CreateDurable));
            this.RegisterResponseType<bool>(nameof(this.PingBroker));
            this.RegisterResponseType<string>(nameof(this.PingBroker2));
            this.RegisterResponseType<int[]>(nameof(this.GetActiveBrokerIdList));
            this.RegisterResponseType<object>(nameof(this.Close));
        }

        public BrokerInitializationResult Create(SessionStartInfoContract info, string sessionId)
        {
            return this.CreateAsync(info, sessionId).GetAwaiter().GetResult();
        }

        public IAsyncResult BeginCreate(SessionStartInfoContract info, string sessionId, AsyncCallback callback, object state)
        {
            return AsApm(this.CreateAsync(info, sessionId), callback, state);
        }

        public BrokerInitializationResult EndCreate(IAsyncResult ar)
        {
            return ((Task<BrokerInitializationResult>)ar).Result;
        }

        public Task<BrokerInitializationResult> CreateAsync(SessionStartInfoContract info, string sessionId)
        {
            return this.StartRequestAsync<BrokerInitializationResult>(nameof(this.Create), info, sessionId);
        }

        public bool PingBroker(string sessionID)
        {
            return this.PingBrokerAsync(sessionID).GetAwaiter().GetResult();
        }

        public IAsyncResult BeginPingBroker(string sessionID, AsyncCallback callback, object state)
        {
            return AsApm(this.PingBrokerAsync(sessionID), callback, state);
        }

        public bool EndPingBroker(IAsyncResult result)
        {
            return ((Task<bool>)result).Result;
        }

        public Task<bool> PingBrokerAsync(string sessionID)
        {
            return this.StartRequestAsync<bool>(nameof(this.PingBroker), sessionID);
        }

        public string PingBroker2(string sessionID)
        {
            return this.PingBroker2Async(sessionID).GetAwaiter().GetResult();
        }

        public IAsyncResult BeginPingBroker2(string sessionID, AsyncCallback callback, object state)
        {
            return AsApm(this.PingBroker2Async(sessionID), callback, state);
        }

        public string EndPingBroker2(IAsyncResult result)
        {
            return ((Task<string>)result).Result;
        }

        public Task<string> PingBroker2Async(string sessionID)
        {
            return this.StartRequestAsync<string>(nameof(this.PingBroker2), sessionID);
        }

        public BrokerInitializationResult CreateDurable(SessionStartInfoContract info, string sessionId)
        {
            return this.CreateDurableAsync(info, sessionId).GetAwaiter().GetResult();
        }

        public IAsyncResult BeginCreateDurable(SessionStartInfoContract info, string sessionId, AsyncCallback callback, object state)
        {
            return AsApm(this.CreateDurableAsync(info, sessionId), callback, state);
        }

        public BrokerInitializationResult EndCreateDurable(IAsyncResult ar)
        {
            return ((Task<BrokerInitializationResult>)ar).Result;
        }

        public Task<BrokerInitializationResult> CreateDurableAsync(SessionStartInfoContract info, string sessionId)
        {
            return this.StartRequestAsync<BrokerInitializationResult>(nameof(this.CreateDurable), info, sessionId);
        }

        public BrokerInitializationResult Attach(string sessionId)
        {
            return this.AttachAsync(sessionId).GetAwaiter().GetResult();
        }

        public IAsyncResult BeginAttach(string sessionId, AsyncCallback callback, object state)
        {
            return AsApm(this.AttachAsync(sessionId), callback, state);
        }

        public BrokerInitializationResult EndAttach(IAsyncResult result)
        {
            return ((Task<BrokerInitializationResult>)result).Result;
        }

        public Task<BrokerInitializationResult> AttachAsync(string sessionId)
        {
            return this.StartRequestAsync<BrokerInitializationResult>(nameof(this.Attach), sessionId);
        }

        public void Close(string sessionId)
        {
            this.CloseAsync(sessionId).GetAwaiter().GetResult();
        }

        public Task CloseAsync(string sessionId)
        {
            return this.CloseAsyncAux(sessionId);
        }

        private Task<object> CloseAsyncAux(string sessionId)
        {
            return this.StartRequestAsync<object>(nameof(this.Close), sessionId);
        }

        public IAsyncResult BeginClose(string sessionId, AsyncCallback callback, object state)
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