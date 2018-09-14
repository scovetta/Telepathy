//----------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="AzureQueueListener.cs" company="Microsoft">
//     Copyright(C) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Listen messages in an Azure queue
// </summary>
//-----------------------------------------------------------------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Threading;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.ServiceBroker;
    using Microsoft.Hpc.ServiceBroker.BackEnd;
    using Microsoft.WindowsAzure.Storage.Queue;

    /// <summary>
    /// AzureQueueListener listens to an Azure queue and receives messages
    /// from it.
    /// </summary>
    /// <typeparam name="T">object type that message in Azure queue represents</typeparam>
    internal class AzureQueueListener<T> : IDisposable where T : class
    {
        /// <summary>
        /// Message retrieve concurrency
        /// </summary>
        private const int MessageRetrieveConcurrency = 16;

        /// <summary>
        /// Azure queue it listens
        /// </summary>
        private CloudQueue queue;

        /// <summary>
        /// MessageRetriever that helps retrieve messages from target Azure queue
        /// </summary>
        private MessageRetriever messageRetriever;

        /// <summary>
        /// Message serializer
        /// </summary>
        private DataContractSerializer messageSerializer = new DataContractSerializer(typeof(T));

        /// <summary>
        /// Handler of received message
        /// </summary>
        private MessageHandler handler;

        /// <summary>
        /// Handler of exception
        /// </summary>
        private ExceptionHandler exceptionHandler;

        /// <summary>
        /// Semphore that controls maximum number of message that can be handled via message handler concurrently
        /// </summary>
        private SemaphoreSlim semSubscription;

        /// <summary>
        /// Initializes a new instance of the AzureQueueListener class
        /// </summary>
        /// <param name="queue">target Azure queue</param>
        /// <param name="handler">message handler</param>
        /// <param name="exceptionHandler">exception handler</param>
        /// <param name="subscription">subscription size that indicates maximum number of message can be handled via message handler concurrently</param>
        public AzureQueueListener(CloudQueue queue, MessageHandler handler, ExceptionHandler exceptionHandler, int subscription)
        {
            Debug.Assert(queue != null, "queue");
            Debug.Assert(handler != null, "handler");
            Debug.Assert(subscription > 0, "subscription");
            this.queue = queue;
            this.handler = handler;
            this.exceptionHandler = exceptionHandler;
            this.semSubscription = new SemaphoreSlim(subscription);

            this.messageRetriever = new MessageRetriever(queue, MessageRetrieveConcurrency, TimeSpan.FromMinutes(Constant.DataOperationTimeoutInMinutes), this.ReceiveMessages, null);
        }

        /// <summary>
        /// The delegate for receive message callback
        /// </summary>
        /// <param name="t">converted message object</param>
        public delegate void MessageHandler(T t);

        /// <summary>
        /// The delegate for receive exception
        /// </summary>
        /// <param name="ex">exception caught</param>
        public delegate void ExceptionHandler(Exception ex);

        /// <summary>
        /// Get data request queue name
        /// </summary>
        /// <param name="clusterId">unique cluster id</param>
        /// <returns>data request queue name</returns>
        public static string GetDataRequestQueueName(string clusterId)
        {
            Debug.Assert(!string.IsNullOrEmpty(clusterId), "clusterId");
            return string.Format(Constant.DataRequestQueueNameFormat, SoaHelper.Md5Hash(clusterId));
        }

        /// <summary>
        /// Get data response queue name for a data proxy role instance
        /// </summary>
        /// <param name="clusterId">unique cluster id</param>
        /// <param name="proxyId">data proxy role instance id</param>
        /// <returns>data response queue name for a specified data proxy role instance</returns>
        public static string GetDataResponseQueueName(string clusterId, string proxyId)
        {
            Debug.Assert(!string.IsNullOrEmpty(clusterId), "clusterId");
            Debug.Assert(!string.IsNullOrEmpty(proxyId), "proxyId");
            return string.Format(Constant.DataResponseQueueNameFormat, SoaHelper.Md5Hash(clusterId + proxyId));
        }

        /// <summary>
        /// Listen to the Azure queue
        /// </summary>
        public void Listen()
        {
            this.messageRetriever.Start();
        }

        /// <summary>
        /// Implement IDisposable.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose the instance
        /// </summary>
        /// <param name="disposing">indicating whether it is called directly or indirectly by user's code</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.messageRetriever.Close();

                // dispose resources
                if (this.semSubscription != null)
                {
                    this.semSubscription.Dispose();
                    this.semSubscription = null;
                }
            }
        }

        /// <summary>
        /// Handler for messages retrieved from target Azure queue
        /// </summary>
        /// <param name="messages">received messages</param>
        private void ReceiveMessages(IEnumerable<CloudQueueMessage> messages)
        {
            // Process all messages first, and then delete them.
            // Here we don't delete each message after processing it because
            // DeleteMessage is slow and we don't want other messages
            // to wait on DeleteMessage operation.  It is hoped to save message
            // round-trip time in case of small load.
            foreach (CloudQueueMessage message in messages)
            {
                using (MemoryStream ms = new MemoryStream(message.AsBytes))
                {
                    T t = (T)this.messageSerializer.ReadObject(ms);
                    this.ConcurrentHandle(t);
                }
            }

            foreach (CloudQueueMessage message in messages)
            {
                try
                {
                    this.queue.DeleteMessage(message);
                }
                catch (Exception ex)
                {
                    if (this.exceptionHandler != null)
                    {
                        this.exceptionHandler(ex);
                    }
                }
            }
        }

        /// <summary>
        /// Handle received message
        /// </summary>
        /// <param name="t">received message object</param>
        private void ConcurrentHandle(T t)
        {
            this.semSubscription.Wait(TimeSpan.FromMinutes(Constant.DataProxyOperationTimeoutInMinutes));

            ThreadPool.QueueUserWorkItem(
                delegate(object state)
                {
                    try
                    {
                        this.handler(t);
                    }
                    catch (Exception ex)
                    {
                        // message handler should never throw exception.
                        TraceUtils.TraceInfo(
                            "AzureQueueListener", "ConcurrentHandle", "Exception {0} while handle message {1}", ex, t);
                    }
                    finally
                    {
                        try
                        {
                            this.semSubscription.Release();
                        }
                        catch (Exception ex)
                        {
                            // swallow any exception including ObjectDisposedException
                            TraceUtils.TraceInfo(
                                "AzureQueueListener", "ConcurrentHandle", "Exception {0} while release semSubscription", ex);
                        }
                    }
                });
        }
    }
}