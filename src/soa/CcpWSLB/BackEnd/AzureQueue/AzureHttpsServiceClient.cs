// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.BackEnd.AzureQueue
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel.Channels;
    using System.Xml;

    using Microsoft.Hpc.BrokerBurst;
    using Microsoft.Telepathy.Session;
    using Microsoft.Telepathy.Session.Internal;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Queue;

    /// <summary>
    /// It is the client to pass request to broker proxy via Azure queue.
    /// </summary>
    internal class AzureHttpsServiceClient : DisposableObject, IService
    {
        /// <summary>
        /// Instance of the AzureQueueManager.
        /// </summary>
        private IAzureQueueManager manager;

        /// <summary>
        /// Storage client assesses the request queue or blob if necessary.
        /// </summary>
        private AzureStorageClient requestStorageClient;

        /// <summary>
        /// Response storage name.
        /// </summary>
        private string responseStorageName;

        /// <summary>
        /// Initializes a new instance of the AzureHttpsServiceClient class.
        /// </summary>
        /// <param name="manager">Azure queue manager</param>
        /// <param name="azureServiceName">azure service name</param>
        /// <param name="responseStorageName">response storage name</param>
        public AzureHttpsServiceClient(IAzureQueueManager manager, string azureServiceName, string responseStorageName)
        {
            this.manager = manager;

            Tuple<CloudQueue, CloudBlobContainer> tuple;

            if (!this.manager.RequestStorage.TryGetValue(azureServiceName, out tuple))
            {
                throw new Exception(
                    string.Format(
                        "[AzureServiceClient].AzureServiceClient: Failed to get the request storage for Azure service {0}",
                        azureServiceName));
            }

            this.requestStorageClient = new AzureStorageClient(tuple.Item1, tuple.Item2);

            this.responseStorageName = responseStorageName;
        }

        /// <summary>
        /// Process message.
        /// </summary>
        /// <param name="request">request message</param>
        /// <returns>response message</returns>
        public Message ProcessMessage(Message request)
        {
            // this method is never called
            throw new NotImplementedException();
        }

        /// <summary>
        /// Async method of ProcessMessage.
        /// </summary>
        /// <param name="request">request message</param>
        /// <param name="callback">callback method</param>
        /// <param name="asyncState">async state</param>
        /// <returns>async result</returns>
        public IAsyncResult BeginProcessMessage(Message request, AsyncCallback callback, object asyncState)
        {
            MessageBuffer buffer = request.CreateBufferedCopy(int.MaxValue);

            byte[] messageData = AzureQueueItem.Serialize(buffer.CreateMessage());

            QueueAsyncResult asyncResult = new QueueAsyncResult(callback, asyncState);

            UniqueId messageId = request.Headers.MessageId;

            asyncResult.MessageId = messageId;

            asyncResult.StorageClient = this.requestStorageClient;

            try
            {
                BrokerTracing.TraceVerbose(
                    "[AzureServiceClient].BeginProcessMessage: Try to add message {0} to the request queue.",
                    messageId);

                var reliableState = new ReliableQueueClient.ReliableState(asyncResult, messageId);

                // Notice: It can happen that response message is back before
                // EndAddMessage is called on the request message. So
                // add/update the callback info to AzureQueueManager before
                // calling BeginAddMessage avoid the issue that response comes
                // back but can't find callback info.
                this.manager.AddQueueAsyncResult(asyncResult, this.requestStorageClient.QueueName, this.responseStorageName);

                this.requestStorageClient.BeginAddMessage(messageData, messageId, this.BeginAddMessageCallback, reliableState);
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError(
                    "[AzureServiceClient].BeginProcessMessage: Failed to add message {0}, {1}",
                    messageId,
                    e);

                this.manager.CompleteCallback(asyncResult, null, e);
            }
            finally
            {
                buffer.Close();
            }

            return asyncResult;
        }

        /// <summary>
        /// Async method of ProcessMessage.
        /// </summary>
        /// <param name="ar">async result</param>
        /// <returns>response message</returns>
        public Message EndProcessMessage(IAsyncResult ar)
        {
            QueueAsyncResult asyncResult = ar as QueueAsyncResult;

            Debug.Assert(asyncResult != null, "ar must be a QueueAsyncResult.");

            using (asyncResult)
            {
                UniqueId messageId = asyncResult.MessageId;

                try
                {
                    this.manager.RemoveQueueAsyncResult(messageId);
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError(
                        "[AzureServiceClient].EndProcessMessage: Failed to remove QueueAsyncResult {0}, {1}",
                        messageId,
                        e);
                }

                if (asyncResult.Exception != null)
                {
                    throw asyncResult.Exception;
                }
                else
                {
                    return asyncResult.ResponseMessage;
                }
            }
        }

        /// <summary>
        /// Dispose the class.
        /// </summary>
        /// <param name="disposing">disposing flag</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    this.requestStorageClient.Close();
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError(
                        "[AzureHttpsServiceClient].Dispose: Failed to close request storage client, {0}",
                        e);
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Callback of the CloudQueue.BeginAddMessage method.
        /// </summary>
        /// <param name="ar">async result</param>
        /// <remarks>
        /// Notice: This method doesn't throw exception. It invokes callback
        /// and pass exception to it in case exception occurs.
        /// </remarks>
        private void BeginAddMessageCallback(IAsyncResult ar)
        {
            BrokerTracing.TraceVerbose("[AzureServiceClient].BeginAddMessageCallback: Enter callback method of BeginAddMessage.");

            var reliableState = ar.AsyncState as ReliableQueueClient.ReliableState;

            QueueAsyncResult asyncResult = reliableState.State as QueueAsyncResult;

            Debug.Assert(asyncResult != null, "reliableState.State must be a QueueAsyncResult.");

            try
            {
                BrokerTracing.TraceVerbose(
                    "[AzureServiceClient].BeginAddMessageCallback: Try to complete adding message {0}",
                    asyncResult.MessageId);

                asyncResult.StorageClient.EndAddMessage(ar);
            }
            catch (StorageException e)
            {
                BrokerTracing.TraceError(
                    "[AzureServiceClient].BeginAddMessageCallback: Failed to complete adding message {0}, {1}",
                    asyncResult.MessageId,
                    e.ToString());

                if (BurstUtility.IsQueueNotFound(e))
                {
                    // StorageException happens here when want to add request
                    // messages, so it must be request queue not found. Handle
                    // the outstanding messages, which are already sent to
                    // request queue, but maybe not got by proxy. And should
                    // consider the multi request queue case when there are
                    // multi azure deployments.
                    this.manager.HandleInvalidRequestQueue(new RequestStorageException(e), this.requestStorageClient.QueueName);
                }

                this.manager.CompleteCallback(asyncResult, null, new RequestStorageException(e));
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError(
                    "[AzureServiceClient].BeginAddMessageCallback: Failed to complete adding message {0}, {1}",
                    asyncResult.MessageId,
                    e.ToString());

                this.manager.CompleteCallback(asyncResult, null, e);
            }
        }
    }
}
