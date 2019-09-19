// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.ServiceBroker.BrokerStorage.AzureQueuePersist
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Hpc.ServiceBroker.BrokerStorage.AzureStorageTool;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Queue;

    internal class AzureQueueRequestFetcher : AzureQueueMessageFetcher
    {
        private readonly CloudQueue requestQueue;

        private readonly CloudQueue waitQueue;

        public AzureQueueRequestFetcher(
            CloudQueue requestQueue,
            CloudQueue waitQueue,
            long messageCount,
            IFormatter messageFormatter,
            CloudBlobContainer blobContainer)
            : base(
                messageCount,
                messageFormatter,
                DefaultPrefetchCacheCapacity,
                Environment.ProcessorCount > DefaultMaxOutstandingFetchCount
                    ? Environment.ProcessorCount
                    : DefaultMaxOutstandingFetchCount,
                blobContainer)
        {
            this.requestQueue = requestQueue;
            this.waitQueue = waitQueue;
            this.prefetchTimer.Elapsed += (sender, args) =>
                {
                    Debug.WriteLine("[AzureQueueRequestFetcher] .prefetchTimer raised.");
                    this.DequeueMessageAsync().GetAwaiter().GetResult();
                    if (!this.isDisposedField)
                    {
                        this.prefetchTimer.Enabled = true;
                    }
                };
            this.prefetchTimer.Enabled = true;
        }

        private async Task DequeueMessageAsync()
        {
            if (this.isDisposedField)
            {
                this.RevertFetchCount();
                return;
            }

            var exceptions = new List<Exception>();
            while (true)
            {
                Exception exception = null;
                if (this.pendingFetchCount < 1)
                {
                    break;
                }

                while (this.pendingFetchCount > 0)
                {
                    byte[] messageBody = null;
                    try
                    {
                        var message = await this.requestQueue.GetMessageAsync();
                        var copyMessage = new CloudQueueMessage(message.AsBytes);
                        await this.waitQueue.AddMessageAsync(copyMessage);
                        await this.requestQueue.DeleteMessageAsync(message);
                        messageBody = message.AsBytes;
                    }
                    catch (Exception e)
                    {
                        BrokerTracing.TraceError(
                            "[AzureQueueRequestFetcher] .DequeueMessageAsync: dequeue message failed, Exception:{0}",
                            e.ToString());
                        exceptions.Add(e);
                    }

                    if (messageBody != null && messageBody.Length > 0)
                    {
                        BrokerQueueItem brokerQueueItem = null;

                        // Deserialize message to BrokerQueueItem
                        try
                        {
                            brokerQueueItem = (BrokerQueueItem)this.formatter.Deserialize(
                                await AzureStorageTool.GetMsgBody(this.blobContainer, messageBody));
                            brokerQueueItem.PersistAsyncToken.AsyncToken =
                                brokerQueueItem.Message.Headers.MessageId.ToString();
                            BrokerTracing.TraceVerbose(
                                "[AzureQueueRequestFetcher] .PeekMessageAsync: deserialize header={0} property={1}",
                                brokerQueueItem.Message.Headers.MessageId,
                                brokerQueueItem.Message.Properties);
                        }
                        catch (Exception e)
                        {
                            BrokerTracing.TraceError(
                                "[AzureQueueRequestFetcher] .PeekMessageAsync: deserialize message failed, Exception:{0}",
                                e.ToString());
                            exception = e;
                            exceptions.Add(e);
                        }

                        this.HandleMessageResult(new MessageResult(brokerQueueItem, exception));
                    }

                    Interlocked.Decrement(ref this.pendingFetchCount);
                }

                this.CheckAndGetMoreMessages();
            }

            foreach (var exception in exceptions)
            {
                this.HandleMessageResult(new MessageResult(null, exception));
            }
        }
    }
}