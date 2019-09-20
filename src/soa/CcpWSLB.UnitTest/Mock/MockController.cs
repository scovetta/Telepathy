// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.UnitTest.Mock
{
    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Interface;

    public class MockController : IController
    {
        public void Flush(int count, string clientid, int batchId, int timeoutThrottlingMs, int timeoutFlushMs)
        {
        }

        public void EndRequests(int count, string clientid, int batchId, int timeoutThrottlingMs, int timeoutEOMMs)
        {
        }

        public void Purge(string clientid)
        {
        }

        public BrokerClientStatus GetBrokerClientStatus(string clientId)
        {
            return BrokerClientStatus.Processing;
        }

        public int GetRequestsCount(string clientId)
        {
            return 1;
        }

        public BrokerResponseMessages PullResponses(string action, GetResponsePosition position, int count, string clientId)
        {
            return new BrokerResponseMessages(){EOM = true};
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
            azureResponseQueueUri = "queue";
            azureResponseBlobUri = "blob";
        }

        public void Ping()
        {
        }
    }
}