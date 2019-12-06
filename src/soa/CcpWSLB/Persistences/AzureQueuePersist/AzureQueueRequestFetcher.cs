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

    using Microsoft.Telepathy.Common;
    using Microsoft.Telepathy.Session.Common;
    using Microsoft.Telepathy.Session.Internal;

    internal class AzureQueueRequestFetcher : AzureQueueMessageFetcher
    {
        private readonly CloudQueue requestQueue;

        private readonly CloudQueue pendingQueue;

        private ConcurrentDictionary<string,bool> requestDic = new ConcurrentDictionary<string, bool>();

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
                    Debug.WriteLine("[AzureQueueRequestFetcher] .prefetchTimer done.");
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

                while (this.pendingFetchCount > 0)
                {
                    try
                    {
                        tasks.Add(Task.Run(async () =>
                        {
                            RetryManager retry = SoaHelper.GetDefaultExponentialRetryManager();
                            var message = await RetryHelper<CloudQueueMessage>.InvokeOperationAsync(
                                async () => await this.requestQueue.GetMessageAsync(),
                                async (e, r) =>
                                    {
                                        await Task.FromResult(
                                            new Func<object>(
                                                () =>
                                                    {
                                                        BrokerTracing.TraceEvent(
                                                            System.Diagnostics.TraceEventType.Error,
                                                            0,
                                                            "[AzureQueueRequestFetcher] .DequeueMessageAsync: Exception thrown while get message from queue: {0} with retry: {1}",
                                                            e,
                                                            r.RetryCount);
                                                        return null;
                                                    }).Invoke());
                                    },
                                retry);

                            if (message != null)
                            {
                                await this.DeserializeMessage(message);
                            }
                            else
                            {
                                Interlocked.Increment(ref this.pendingFetchCount);
                            }
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

                try
                {
                    await Task.WhenAll(tasks);
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError(
                        "[AzureQueueRequestFetcher] .DequeueMessageAsync: exception raises in dequeue tasks, Exception:{0}",
                        e.ToString());
                }

                this.CheckAndGetMoreMessages();
            }

            foreach (var exception in exceptions)
            {
                this.HandleMessageResult(new MessageResult(null, exception));
            }
        }

        private async Task DeserializeMessage(CloudQueueMessage message)
        {
            Exception exception = null;
            BrokerQueueItem brokerQueueItem = null;

            if (message.AsBytes != null && message.AsBytes.Length > 0)
            {
                // Deserialize message to BrokerQueueItem
                try
                {
                    brokerQueueItem = (BrokerQueueItem)this.formatter.Deserialize(
                        await AzureStorageTool.GetMsgBody(this.blobContainer, message.AsBytes));
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
            }

            if (brokerQueueItem != null && !this.requestDic.TryAdd(
                    brokerQueueItem.PersistAsyncToken.AsyncToken.ToString(),
                    true))
            {
                Interlocked.Increment(ref this.pendingFetchCount);
                try
                {
                    await this.requestQueue.DeleteMessageAsync(message);
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError(
                        "[AzureQueueRequestFetcher] .DeserializeMessage: delete duplicate message in request queue failed, Exception:{0}",
                        e.ToString());
                }
            }
            else
            {
                var copyMessage = new CloudQueueMessage(message.AsBytes);
                RetryManager retry = SoaHelper.GetDefaultExponentialRetryManager();
                await RetryHelper<object>.InvokeOperationAsync(
                    async () =>
                        {
                            await this.pendingQueue.AddMessageAsync(copyMessage);
                            return null;
                        },
                    async (e, r) =>
                        {
                            await Task.FromResult(
                                new Func<object>(
                                    () =>
                                        {
                                            BrokerTracing.TraceEvent(
                                                System.Diagnostics.TraceEventType.Error,
                                                0,
                                                "[AzureQueueRequestFetcher] .DeserializeMessage: Exception thrown while add message into pending queue: {0} with retry: {1}",
                                                e,
                                                r.RetryCount);
                                            return null;
                                        }).Invoke());
                        },
                    retry);
                
                try
                {
                    await this.requestQueue.DeleteMessageAsync(message);
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError(
                        "[AzureQueueRequestFetcher] .DeserializeMessage: delete message in request queue failed, Exception:{0}",
                        e.ToString());
                }

                this.HandleMessageResult(new MessageResult(brokerQueueItem, exception));
            }
        }
    }
}