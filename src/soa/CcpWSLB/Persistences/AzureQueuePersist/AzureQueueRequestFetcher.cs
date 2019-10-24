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
    using Microsoft.WindowsAzure.Storage.Queue;
    using System.Collections.Concurrent;

    internal class AzureQueueRequestFetcher : AzureQueueMessageFetcher
    {
        private readonly CloudQueue requestQueue;

        private readonly CloudQueue pendingQueue;

        public AzureQueueRequestFetcher(
            CloudQueue requestQueue,
            CloudQueue pendingQueue,
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
            this.pendingQueue = pendingQueue;
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
                if (this.pendingFetchCount < 1)
                {
                    break;
                }

                List<Task> tasks = new List<Task>();
                ConcurrentDictionary<string, Task> requestConcurrentDictionary = new ConcurrentDictionary<string, Task>();

                while (this.pendingFetchCount > 0)
                {
                    try
                    {
                        tasks.Add(Task.Run(async () =>
                        {
                            var message = await this.requestQueue.GetMessageAsync();
                            await requestConcurrentDictionary.GetOrAdd(message.Id, async (m) =>
                            {
                                var copyMessage = new CloudQueueMessage(message.AsBytes);
                                await this.pendingQueue.AddMessageAsync(copyMessage);
                                await this.requestQueue.DeleteMessageAsync(message);
                                await DeserializeMessage(message.AsBytes);
                            });
                        }));
                    }
                    catch (Exception e)
                    {
                        BrokerTracing.TraceError(
                            "[AzureQueueRequestFetcher] .DequeueMessageAsync: dequeue message failed, Exception:{0}",
                            e.ToString());
                        exceptions.Add(e);
                    }

                    Interlocked.Decrement(ref this.pendingFetchCount);
                }
                await Task.WhenAll(tasks);
                this.CheckAndGetMoreMessages();
            }

            foreach (var exception in exceptions)
            {
                this.HandleMessageResult(new MessageResult(null, exception));
            }
        }

        private async Task DeserializeMessage(byte[] messageBody)
        {
            Exception exception = null;
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
                        "[AzureQueueRequestFetcher] .DeserializeMessage: deserialize header={0} property={1}",
                        brokerQueueItem.Message.Headers.MessageId,
                        brokerQueueItem.Message.Properties);
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError(
                        "[AzureQueueRequestFetcher] .DeserializeMessage: deserialize message failed, Exception:{0}",
                        e.ToString());
                    exception = e;
                }

                this.HandleMessageResult(new MessageResult(brokerQueueItem, exception));
            }
        }
    }
}