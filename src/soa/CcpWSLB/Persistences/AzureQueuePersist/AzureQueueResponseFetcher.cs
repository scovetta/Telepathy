// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.Persistences.AzureQueuePersist
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Telepathy.ServiceBroker.BrokerQueue;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Table;

    using Nito.AsyncEx;

    internal class AzureQueueResponseFetcher : AzureQueueMessageFetcher
    {
        private readonly CloudTable responseTable;

        private long lastIndex;

        private long ackIndex = int.MaxValue >> 1;

        private int index = 0;

        private List<ResponseEntity> responseList = null;

        public long AckIndex => this.ackIndex;

        public AzureQueueResponseFetcher(
            CloudTable responseTable,
            long messageCount,
            IFormatter messageFormatter,
            CloudBlobContainer blobContainer,
            long lastReponseIndex)
            : base(
                messageCount,
                messageFormatter,
                DefaultPrefetchCacheCapacity,
                Environment.ProcessorCount > DefaultMaxOutstandingFetchCount
                    ? Environment.ProcessorCount
                    : DefaultMaxOutstandingFetchCount,
                blobContainer)
        {
            this.responseTable = responseTable;
            if (lastReponseIndex > this.ackIndex)
            {
                Interlocked.Exchange(ref this.ackIndex, lastReponseIndex);
            }

            this.prefetchTimer.Elapsed += (sender, args) =>
            {
                Debug.WriteLine("[AzureQueueResponseFetch] .prefetchTimer raised.");
                this.PeekMessageAsync().GetAwaiter().GetResult();
                if (!this.isDisposedField)
                {
                    this.prefetchTimer.Enabled = true;
                }
                Debug.WriteLine("[AzureQueueResponseFetch] .prefetchTimer done.");
            };
            this.prefetchTimer.Enabled = true;
        }

        private async Task PeekMessageAsync()
        {
            if (this.isDisposedField)
            {
                this.RevertFetchCount();
                return;
            }

            while (true)
            {
                if (this.pendingFetchCount < 1)
                {
                    break;
                }

                List<Task> tasks = new List<Task>();

                while (this.pendingFetchCount > 0)
                {
                    Exception exception = null;
                    byte[] messageBody = null;
                    try
                    {
                        if (responseList == null || responseList.Count <= index)
                        {
                            BrokerTracing.TraceVerbose(
                                "[AzureQueueResponseFetch] .PeekMessageAsync: lastIndex={0}, ackIndex={1}",
                                lastIndex, ackIndex);
                            responseList = await AzureStorageTool.GetBatchEntityAsync(
                                this.responseTable,
                                this.lastIndex,
                                this.ackIndex);
                            index = 0;
                            BrokerTracing.TraceVerbose(
                                "[AzureQueueResponseFetch] .PeekMessageAsync: get batch entity count={0}",
                                responseList.Count);
                        }

                        if (responseList.Count > index)
                        {
                            messageBody = responseList[index].Message;
                            if (long.TryParse(responseList[index].RowKey, out var tempIndex)
                                && tempIndex > this.lastIndex)
                            {
                                this.lastIndex = tempIndex;
                            }

                            index++;
                        }
                    }
                    catch (Exception e)
                    {
                        BrokerTracing.TraceError(
                            "[AzureQueueResponseFetch] .PeekMessageAsync: peek batch messages failed, Exception:{0}",
                            e.ToString());
                        exception = e;
                    }

                    if (messageBody == null && exception == null)
                    {
                        BrokerTracing.TraceWarning("[AzureQueueResponseFetch] .PeekMessageAsync: null message and exception, lastIndex = {0}, ack = {1}, Count = {2}, index = {3}.",
                            this.lastIndex, this.ackIndex, responseList.Count, index);
                    }
                    else
                    {
                        tasks.Add(Task.Run(async () => await this.DeserializeMessage(messageBody, exception)));
                        Interlocked.Decrement(ref this.pendingFetchCount);
                    }
                }

                try
                {
                    await tasks.WhenAll();
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError(
                        "[AzureQueueResponseFetch] .PeekMessageAsync: exception raises in dequeue tasks, Exception:{0}",
                        e.ToString());
                }

                this.CheckAndGetMoreMessages();
            }
        }

        private async Task DeserializeMessage(byte[] messageBody, Exception exception)
        {
            BrokerQueueItem brokerQueueItem = null;

            if (messageBody != null && messageBody.Length > 0)
            {
                // Deserialize message to BrokerQueueItem
                try
                {
                    brokerQueueItem = (BrokerQueueItem)this.formatter.Deserialize(
                        await AzureStorageTool.GetMsgBody(this.blobContainer, messageBody));
                    brokerQueueItem.PersistAsyncToken.AsyncToken =
                        brokerQueueItem.Message.Headers.RelatesTo.ToString();
                    BrokerTracing.TraceVerbose(
                        "[AzureQueueResponseFetch] .PeekMessage: deserialize header={0} property={1}",
                        brokerQueueItem.Message.Headers.RelatesTo,
                        brokerQueueItem.Message.Properties);
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError(
                        "[AzureQueueResponseFetch] .PeekMessage: deserialize message failed, Exception:{0}",
                        e.ToString());
                    exception = e;
                }
            }

            this.HandleMessageResult(new MessageResult(brokerQueueItem, exception));
        }

        public void ChangeAck(long ack)
        {
            if (ackIndex < ack)
                Interlocked.Exchange(ref ackIndex, ack);
        }
    }   
}