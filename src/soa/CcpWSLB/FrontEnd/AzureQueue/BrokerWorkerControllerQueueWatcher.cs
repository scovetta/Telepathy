// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.ServiceBroker.FrontEnd.AzureQueue
{
    using System.Diagnostics;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.QueueAdapter;
    using Microsoft.Hpc.Scheduler.Session.QueueAdapter.DTO;
    using Microsoft.Hpc.Scheduler.Session.QueueAdapter.Interface;
    using Microsoft.Hpc.Scheduler.Session.QueueAdapter.Server;

    public class BrokerWorkerControllerQueueWatcher : CloudQueueWatcherBase
    {
        private IController instance;

        internal BrokerWorkerControllerQueueWatcher(IController instance, string connectionString, string sessionId)
        {
            this.instance = instance;
            CloudQueueSerializer serializer = new CloudQueueSerializer(CloudQueueCmdTypeBinder.BrokerLauncherBinder);

            this.QueueListener = new CloudQueueListener<CloudQueueCmdDto>(
                connectionString,
                CloudQueueConstants.GetBrokerWorkerControllerRequestQueueName(sessionId),
                serializer,
                this.InvokeInstanceMethodFromCmdObj);
            this.QueueWriter = new CloudQueueWriter<CloudQueueResponseDto>(connectionString, CloudQueueConstants.GetBrokerWorkerControllerResponseQueueName(sessionId), serializer);
            this.QueueListener.StartListen();

            this.RegisterCmdDelegates();
            Trace.TraceInformation("BrokerWorkerControllerQueueWatcher started.");
        }

        public BrokerWorkerControllerQueueWatcher(IController instance, IQueueListener<CloudQueueCmdDto> listener, IQueueWriter<CloudQueueResponseDto> writer) : base(listener, writer)
        {
            this.instance = instance;
            this.RegisterCmdDelegates();
        }

        private void RegisterCmdDelegates()
        {
            this.RegisterCmdDelegate(nameof(this.Flush), this.Flush);
            this.RegisterCmdDelegate(nameof(this.EndRequests), this.EndRequests);
            this.RegisterCmdDelegate(nameof(this.Purge), this.Purge);
            this.RegisterCmdDelegate(nameof(this.GetBrokerClientStatus), this.GetBrokerClientStatus);
            this.RegisterCmdDelegate(nameof(this.GetRequestsCount), this.GetRequestsCount);
            this.RegisterCmdDelegate(nameof(this.PullResponses), this.PullResponses);
            this.RegisterCmdDelegate(nameof(this.GetResponsesAQ), this.GetResponsesAQ);
            this.RegisterCmdDelegate(nameof(this.Ping), this.Ping);
        }

        public Task Flush(CloudQueueCmdDto cmdObj)
        {
            cmdObj.GetUnpacker().UnpackInt(out var count).UnpackString(out string clientid).UnpackInt(out int batchId).UnpackInt(out int timeoutThrottlingMs).UnpackInt(out int timeoutFlushMs);
            this.instance.Flush(count, clientid, batchId, timeoutThrottlingMs, timeoutFlushMs);
            return this.CreateAndSendEmptyResponse(cmdObj);
        }

        public Task EndRequests(CloudQueueCmdDto cmdObj)
        {
            cmdObj.GetUnpacker().UnpackInt(out var count).UnpackString(out string clientid).UnpackInt(out int batchId).UnpackInt(out int timeoutThrottlingMs).UnpackInt(out int timeoutEOMMs);
            this.instance.EndRequests(count, clientid, batchId, timeoutThrottlingMs, timeoutEOMMs);
            return this.CreateAndSendEmptyResponse(cmdObj);
        }

        public Task Purge(CloudQueueCmdDto cmdObj)
        {
            cmdObj.GetUnpacker().UnpackString(out var clientid);
            this.instance.Purge(clientid);
            return this.CreateAndSendEmptyResponse(cmdObj);
        }

        public Task GetBrokerClientStatus(CloudQueueCmdDto cmdObj)
        {
            cmdObj.GetUnpacker().UnpackString(out var clientId);
            var res = this.instance.GetBrokerClientStatus(clientId);
            return this.CreateAndSendResponse(cmdObj, res);
        }

        public Task GetRequestsCount(CloudQueueCmdDto cmdObj)
        {
            cmdObj.GetUnpacker().UnpackString(out var clientId);
            var res = this.instance.GetRequestsCount(clientId);
            return this.CreateAndSendResponse(cmdObj, res);
        }

        public Task PullResponses(CloudQueueCmdDto cmdObj)
        {
            cmdObj.GetUnpacker().UnpackString(out var action).Unpack<long>(out var position).UnpackInt(out int count).UnpackString(out string clientId);
            var res = this.instance.PullResponses(action, (GetResponsePosition)position, count, clientId);
            return this.CreateAndSendResponse(cmdObj, res);
        }

        public Task GetResponsesAQ(CloudQueueCmdDto cmdObj)
        {
            cmdObj.GetUnpacker()
                .UnpackString(out string action)
                .UnpackString(out string clientData)
                .Unpack<long>(out var resetToBegin)
                .UnpackInt(out int count)
                .UnpackString(out string clientId)
                .UnpackInt(out var sessionHash);
            this.instance.GetResponsesAQ(action, clientData, (GetResponsePosition)resetToBegin, count, clientId, sessionHash, out var azureResponseQueueUri, out var azureResponseBlobUri);
            var res = (azureResponseQueueUri, azureResponseBlobUri);
            return this.CreateAndSendResponse(cmdObj, res);
        }

        public Task Ping(CloudQueueCmdDto cmdObj)
        {
            this.instance.Ping();
            return this.CreateAndSendEmptyResponse(cmdObj);
        }

        public void StopWatch()
        {
            this.QueueListener.StopListen();
        }
    }
}