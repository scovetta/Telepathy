namespace Microsoft.Hpc.Scheduler.Session.QueueAdapter.Client
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session.Interface;
    using Microsoft.Hpc.Scheduler.Session.QueueAdapter.DTO;
    using Microsoft.Hpc.Scheduler.Session.QueueAdapter.Interface;

    public class BrokerControllerCloudQueueClient : CloudQueueClientBase, IController
    {
        public BrokerControllerCloudQueueClient(string connectionString, int sessionId)
        {
            CloudQueueSerializer serializer = this.DefaultSerializer;
            this.Listener = new CloudQueueListener<CloudQueueResponseDto>(connectionString, CloudQueueConstants.GetBrokerWorkerControllerResponseQueueName(sessionId), serializer, this.ReceiveResponse);
            this.Writer = new CloudQueueWriter<CloudQueueCmdDto>(connectionString, CloudQueueConstants.GetBrokerWorkerControllerRequestQueueName(sessionId), serializer);
            this.Init();
        }

        public BrokerControllerCloudQueueClient(string requestQueueUri, string responseQueueUri)
        {
            CloudQueueSerializer serializer = this.DefaultSerializer;
            this.Listener = new CloudQueueListener<CloudQueueResponseDto>(responseQueueUri, serializer, this.ReceiveResponse);
            this.Writer = new CloudQueueWriter<CloudQueueCmdDto>(requestQueueUri, serializer);
            this.Init();
        }

        public BrokerControllerCloudQueueClient(IQueueListener<CloudQueueResponseDto> listener, IQueueWriter<CloudQueueCmdDto> writer) : base(listener, writer)
        {
            this.RegisterResponseTypes();
        }

        private CloudQueueSerializer DefaultSerializer => new CloudQueueSerializer(CloudQueueCmdTypeBinder.BrokerLauncherBinder);

        private void Init()
        {
            this.Listener.StartListen();
            this.RegisterResponseTypes();
        }

        private void RegisterResponseTypes()
        {
            this.RegisterResponseType(nameof(this.Flush));
            this.RegisterResponseType(nameof(this.EndRequests));
            this.RegisterResponseType(nameof(this.Purge));
            this.RegisterResponseType<long>(nameof(this.GetBrokerClientStatus));
            this.RegisterResponseType<long>(nameof(this.GetRequestsCount));
            this.RegisterResponseType<BrokerResponseMessages>(nameof(this.PullResponses));
            this.RegisterResponseType<(string, string)>(nameof(this.GetResponsesAQ));
            this.RegisterResponseType(nameof(this.Ping));
        }

        public void Flush(int count, string clientid, int batchId, int timeoutThrottlingMs, int timeoutFlushMs)
        {
            this.FlushAsync(count, clientid, batchId, timeoutThrottlingMs, timeoutFlushMs).GetAwaiter().GetResult();
        }

        public Task FlushAsync(int count, string clientid, int batchId, int timeoutThrottlingMs, int timeoutFlushMs)
        {
            return this.StartRequestAsync(nameof(this.Flush), count, clientid, batchId, timeoutThrottlingMs, timeoutFlushMs);
        }

        public void EndRequests(int count, string clientid, int batchId, int timeoutThrottlingMs, int timeoutEOMMs)
        {
            this.EndRequestsAsync(count, clientid, batchId, timeoutThrottlingMs, timeoutEOMMs).GetAwaiter().GetResult();
        }

        public Task EndRequestsAsync(int count, string clientid, int batchId, int timeoutThrottlingMs, int timeoutEOMMs)
        {
            return this.StartRequestAsync(nameof(this.EndRequests), count, clientid, batchId, timeoutThrottlingMs, timeoutEOMMs);
        }

        public void Purge(string clientid)
        {
            this.PurgeAsync(clientid).GetAwaiter().GetResult();
        }

        public Task PurgeAsync(string clientid)
        {
            return this.StartRequestAsync(nameof(this.Purge), clientid);
        }

        public BrokerClientStatus GetBrokerClientStatus(string clientId)
        {
            return this.GetBrokerClientStatusAsync(clientId).GetAwaiter().GetResult();
        }

        public async Task<BrokerClientStatus> GetBrokerClientStatusAsync(string clientId)
        {
            var res = await this.StartRequestAsync<long>(nameof(this.GetBrokerClientStatus), clientId);
            return (BrokerClientStatus)res;
        }

        public int GetRequestsCount(string clientId)
        {
            return this.GetRequestsCountAsync(clientId).GetAwaiter().GetResult();
        }

        public async Task<int> GetRequestsCountAsync(string clientId)
        {
            return (int)await this.StartRequestAsync<long>(nameof(this.GetRequestsCount), clientId);
        }

        public BrokerResponseMessages PullResponses(string action, GetResponsePosition position, int count, string clientId)
        {
            return this.PullResponsesAsync(action, position, count, clientId).GetAwaiter().GetResult();
        }

        public Task<BrokerResponseMessages> PullResponsesAsync(string action, GetResponsePosition position, int count, string clientId)
        {
            return this.StartRequestAsync<BrokerResponseMessages>(nameof(this.PullResponses), action, position, count, clientId);
        }

        public void GetResponsesAQ(
            string action,
            string clientData,
            GetResponsePosition resetToBegin,
            int count,
            string clientId,
            int sessionHash,
            out string azureResponseQueueUri,
            out string azureResponseBlobUri)
        {
            var res = this.GetResponsesAQAsync(action, clientData, resetToBegin, count, clientId, sessionHash).GetAwaiter().GetResult();
            azureResponseQueueUri = res.azureResponseQueueUri;
            azureResponseBlobUri = res.azureResponseBlobUr;
        }

        public Task<(string azureResponseQueueUri, string azureResponseBlobUr)> GetResponsesAQAsync(
            string action,
            string clientData,
            GetResponsePosition resetToBegin,
            int count,
            string clientId,
            int sessionHash)
        {
            return this.StartRequestAsync<(string, string)>(nameof(this.GetResponsesAQ), action, clientData, resetToBegin, count, clientId, sessionHash);
        }

        public void Ping()
        {
            this.PingAsync().GetAwaiter().GetResult();
        }

        public Task PingAsync()
        {
            return this.StartRequestAsync(nameof(this.Ping));
        }
    }
}