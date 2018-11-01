namespace Microsoft.Hpc.Scheduler.Session.QueueAdapter.Client
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session.Interface;
    using Microsoft.Hpc.Scheduler.Session.QueueAdapter.DTO;

    public class BrokerControllerCloudQueueClient : CloudQueueClientBase, IController
    {
        public BrokerControllerCloudQueueClient(string connectionString)
        {
            CloudQueueSerializer serializer = new CloudQueueSerializer(CloudQueueCmdTypeBinder.BrokerLauncherBinder);
            this.Listener = new CloudQueueListener<CloudQueueResponseDto>(connectionString, CloudQueueConstants.BrokerWorkerControllerResponseQueueName, serializer, this.ReceiveResponse);
            this.Writer = new CloudQueueWriter<CloudQueueCmdDto>(connectionString, CloudQueueConstants.BrokerWorkerControllerRequestQueueName, serializer);
            this.Listener.StartListen();
            this.RegisterResponseTypes();
        }

        private void RegisterResponseTypes()
        {
            this.RegisterResponseType(nameof(this.Flush));
            this.RegisterResponseType(nameof(this.EndRequests));
            this.RegisterResponseType(nameof(this.Purge));
            this.RegisterResponseType<BrokerClientStatus>(nameof(this.GetBrokerClientStatus));
            this.RegisterResponseType<string>(nameof(this.GetRequestsCount));
            this.RegisterResponseType<BrokerResponseMessages>(nameof(this.PullResponses));
            this.RegisterResponseType(nameof(this.GetResponsesAQ));
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
            throw new NotImplementedException();
        }

        public void Purge(string clientid)
        {
            throw new NotImplementedException();
        }

        public BrokerClientStatus GetBrokerClientStatus(string clientId)
        {
            throw new NotImplementedException();
        }

        public int GetRequestsCount(string clientId)
        {
            throw new NotImplementedException();
        }

        public BrokerResponseMessages PullResponses(string action, GetResponsePosition position, int count, string clientId)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void Ping()
        {
            throw new NotImplementedException();
        }
    }
}