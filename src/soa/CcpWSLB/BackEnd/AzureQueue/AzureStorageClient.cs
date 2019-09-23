// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.BackEnd.AzureQueue
{
    using System;
    using System.IO;
    using System.ServiceModel.Channels;
    using System.Xml;

    using Microsoft.Hpc.BrokerBurst;
    using Microsoft.Telepathy.ServiceBroker.Common;
    using Microsoft.Telepathy.Session.Internal;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Queue;

    /// <summary>
    /// It is the client to send message to Azure storage queue or blob based
    /// on the message size.
    /// </summary>
    internal class AzureStorageClient : DisposableObject
    {
        /// <summary>
        /// When payload size is less than/equals 48KB, it can fit in a queue
        /// message.
        /// </summary>
        private const int MessageSizeBoundary = 48 * 1024;

        /// <summary>
        /// Blob container to store the large messages. Each message is a blob
        /// inside this container. Container name is the same as corresponding
        /// queue name.
        /// </summary>
        private CloudBlobContainer container;

        /// <summary>
        /// it is a flag indicating if it is on Azure node.
        /// </summary>
        private bool onAzure;

        /// <summary>
        /// Initializes a new instance of the AzureStorageClient class.
        /// </summary>
        /// <param name="queue">cloud queue</param>
        /// <param name="container">cloud blob container</param>
        public AzureStorageClient(CloudQueue queue, CloudBlobContainer container)
            : this(queue, container, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the AzureStorageClient class.
        /// </summary>
        /// <param name="queue">cloud queue</param>
        /// <param name="container">cloud blob container</param>
        /// <param name="onAzure">
        /// a flag indicating current process is on Azure or not
        /// Notice: proxy node does not have the CCP_ONAZURE env var.
        /// </param>
        public AzureStorageClient(CloudQueue queue, CloudBlobContainer container, bool onAzure)
        {
            this.Queue = queue;

            this.ReliableQueue = new ReliableQueueClient(this.Queue);

            this.container = container;

            this.onAzure = onAzure;

            this.QueueName = this.Queue.Name;
        }

        /// <summary>
        /// Gets the cloud queue.
        /// </summary>
        public CloudQueue Queue
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the reliable queue client.
        /// </summary>
        public ReliableQueueClient ReliableQueue
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the cloud queue name.
        /// </summary>
        public string QueueName
        {
            get;
            private set;
        }

        #region blob operation

        // TODO: keep it simple, invoke sync method of blob operations now.

        /// <summary>
        /// Update wcf message to blob.
        /// </summary>
        /// <param name="messageData">raw data of wcf message</param>
        /// <param name="messageId">message Id, it is used as blob name</param>
        public void UploadBlob(byte[] messageData, UniqueId messageId)
        {
            // we only support guid based UniqueId, so it is a valid blob name
            string blobName = messageId.ToString();

            CloudBlockBlob blob = this.container.GetBlockBlobReference(blobName);

            using (MemoryStream stream = new MemoryStream(messageData))
            {
                blob.UploadFromStream(stream);
            }
        }

        /// <summary>
        /// Get wcf message from blob.
        /// </summary>
        /// <param name="messageId">message Id, it is used as blob name</param>
        /// <returns>raw data of wcf message</returns>
        public byte[] DownloadBlob(UniqueId messageId)
        {
            string blobName = messageId.ToString();

            CloudBlockBlob blob = this.container.GetBlockBlobReference(blobName);

            using (MemoryStream stream = new MemoryStream())
            {
                blob.DownloadToStream(stream);

                return stream.ToArray();
            }
        }

        /// <summary>
        /// Delete wcf message from blob.
        /// </summary>
        /// <param name="messageId">message Id, it is used as blob name</param>
        public void DeleteBlob(UniqueId messageId)
        {
            string blobName = messageId.ToString();

            CloudBlockBlob blob = this.container.GetBlockBlobReference(blobName);

            blob.DeleteIfExists();
        }

        #endregion

        /// <summary>
        /// Convert a cloud queue message to wcf message, may need to access
        /// blob to get large message.
        /// </summary>
        /// <param name="message">queue message</param>
        /// <returns>wcf message</returns>
        public Message GetWcfMessageFromQueueMessage(CloudQueueMessage message)
        {
            Message wcfMessage = AzureQueueItem.Deserialize(message.AsBytes);

            if (wcfMessage.Headers.FindHeader(Constant.MessageHeaderBlob, Constant.HpcHeaderNS) > 0)
            {
                using (wcfMessage)
                {
                    byte[] data = this.DownloadBlob(wcfMessage.Headers.MessageId);

                    return AzureQueueItem.Deserialize(data);
                }
            }
            else
            {
                return wcfMessage;
            }
        }

        /// <summary>
        /// Async method for add message. If the message size hits limit, store
        /// it in blob and add a referral message in queue.
        /// </summary>
        /// <param name="messageData">raw data of message</param>
        /// <param name="messageId">wcf message Id</param>
        /// <param name="callback">callback method</param>
        /// <param name="state">state object</param>
        /// <returns>async state</returns>
        public IAsyncResult BeginAddMessage(byte[] messageData, UniqueId messageId, AsyncCallback callback, ReliableQueueClient.ReliableState state)
        {
            CloudQueueMessage message = new CloudQueueMessage(messageData);

            try
            {
                if (messageData.Length <= MessageSizeBoundary)
                {
                    message = new CloudQueueMessage(messageData);

                    return this.ReliableQueue.BeginAddMessage(message, callback, state);
                }
            }
            catch (ArgumentException e)
            {
                // according to the test, when payload is <= 48KB, it can fit
                // in a queue message. otherwise, ArgumentException occurs. but
                // there is no doc about this. so catch ArgumentException here,
                // and store message to blob below.
                TraceUtils.TraceWarning("AzureStorageClient", "BeginAddMessage", "BeginAddMessage failed, {0}", e);
            }
            catch (Exception ex)
            {
                TraceUtils.TraceWarning("AzureStorageClient", "BeginAddMessage", "BeginAddMessage failed, {0}", ex);
                throw;
            }

            TraceUtils.TraceVerbose("AzureStorageClient", "BeginAddMessage", "Upload message {0} to storage blob.", messageId);

            this.UploadBlob(messageData, messageId);

            // Store a referral message in Azure queue with the same message Id
            // as the original message. It redirects proxy to get real message
            // from the blob.
            Message referralMessage = Message.CreateMessage(MessageVersion.Default, string.Empty);

            referralMessage.Headers.MessageId = messageId;

            referralMessage.Headers.Add(
                MessageHeader.CreateHeader(Constant.MessageHeaderBlob, Constant.HpcHeaderNS, string.Empty));

            message = new CloudQueueMessage(AzureQueueItem.Serialize(referralMessage));

            return this.ReliableQueue.BeginAddMessage(message, callback, state);
        }

        /// <summary>
        /// Async method for add message.
        /// </summary>
        /// <param name="asyncResult">async result</param>
        public void EndAddMessage(IAsyncResult asyncResult)
        {
            this.ReliableQueue.EndAddMessage(asyncResult);
        }

        /// <summary>
        /// Delete the message from queue or blob if necessary.
        /// </summary>
        /// <param name="message">queue message</param>
        /// <param name="messageId">wcf message Id</param>
        public void DeleteMessageAsync(CloudQueueMessage message, UniqueId messageId)
        {
            this.BeginDeleteMessage(message, messageId, this.EndDeleteMessage, null);
        }

        /// <summary>
        /// Delete the message from queue or blob if necessary.
        /// </summary>
        /// <remarks>
        /// In order to reduce concurrency access to queue, it is a sync method.
        /// </remarks>
        /// <param name="message">queue message</param>
        /// <param name="messageId">wcf message Id</param>
        public void DeleteMessageSync(CloudQueueMessage message, UniqueId messageId)
        {
            IAsyncResult asyncResult = this.BeginDeleteMessage(message, messageId, null, null);
            this.EndDeleteMessage(asyncResult);
        }

        /// <summary>
        /// Dispose current object.
        /// </summary>
        /// <param name="disposing">disposing flag</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (this.ReliableQueue != null)
                {
                    try
                    {
                        this.ReliableQueue.Close();
                    }
                    catch (Exception e)
                    {
                        TraceUtils.TraceVerbose("AzureStorageClient", "Dispose", "Closing ReliableQueue failed, {0}", e);
                    }

                    this.ReliableQueue = null;
                }
            }
        }

        /// <summary>
        /// Async method for delete message.
        /// </summary>
        /// <param name="message">queue message</param>
        /// <param name="messageId">message Id</param>
        /// <param name="callback">callback method</param>
        /// <param name="state">state object</param>
        /// <returns>async result</returns>
        private IAsyncResult BeginDeleteMessage(CloudQueueMessage message, UniqueId messageId, AsyncCallback callback, object state)
        {
            return this.Queue.BeginDeleteMessage(message, null, null, callback, new Tuple<object, UniqueId>(state, messageId));
        }

        /// <summary>
        /// Async method for delete message.
        /// </summary>
        /// <param name="asyncResult">async result</param>
        private void EndDeleteMessage(IAsyncResult asyncResult)
        {
            UniqueId messageId = null;

            try
            {
                Tuple<object, UniqueId> tuple = asyncResult.AsyncState as Tuple<object, UniqueId>;

                messageId = tuple.Item2;

                // Notice: (azure burst) for perf consideration, on-premise
                // broker does not need to delete blob (for large message),
                // they will be removed when broker terminates. And broker
                // service also conducts cleanup.
                if (this.onAzure)
                {
                    if (messageId != null)
                    {
                        TraceUtils.TraceVerbose(
                            "AzureStorageClient",
                            "EndDeleteMessage",
                            "Delete message {0} from storage blob.",
                            messageId);

                        try
                        {
                            this.DeleteBlob(messageId);
                        }
                        catch (Exception e)
                        {
                            TraceUtils.TraceError("AzureStorageClient", "EndDeleteMessage", "DeleteBlob failed, {0}", e);
                        }
                    }
                }

                this.Queue.EndDeleteMessage(asyncResult);
            }
            catch (Exception e)
            {
                string messageIdString = string.Empty;

                if (messageId != null)
                {
                    messageIdString = messageId.ToString();
                }

                TraceUtils.TraceError(
                    "AzureStorageClient",
                    "EndDeleteMessage",
                    "Error occurs when delete message {0}, {1}",
                    messageIdString,
                    e);
            }
        }
    }
}
