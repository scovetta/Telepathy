// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.ServiceBroker.BackEnd
{
    using System;
    using System.ServiceModel.Channels;
    using System.Threading;
    using System.Xml;
    using Microsoft.Hpc.Scheduler.Session.Common;

    /// <summary>
    /// It is the AsyncResult consumed by AzureQueueClient operations.
    /// </summary>
    internal class QueueAsyncResult : DisposableObject, IAsyncResult
    {
        /// <summary>
        /// Instance of ManualResetEvent.
        /// </summary>
        private ManualResetEvent handle = new ManualResetEvent(false);

        /// <summary>
        /// Initializes a new instance of the QueueAsyncResult class.
        /// </summary>
        /// <param name="callback">callback method for response message</param>
        /// <param name="asyncState">async state object</param>
        public QueueAsyncResult(AsyncCallback callback, object asyncState)
        {
            this.Callback = callback;
            this.AsyncState = asyncState;
        }

        /// <summary>
        /// Gets the callback.
        /// </summary>
        public AsyncCallback Callback
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        public Exception Exception
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Azure storage client.
        /// </summary>
        public AzureStorageClient StorageClient
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the message Id.
        /// </summary>
        public UniqueId MessageId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the response message.
        /// </summary>
        public Message ResponseMessage
        {
            get;
            set;
        }

        #region IAsyncResult Members

        /// <summary>
        /// Gets the async state.
        /// </summary>
        public object AsyncState
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the wait handle.
        /// </summary>
        public WaitHandle AsyncWaitHandle
        {
            get
            {
                return this.handle;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the asynchronous operation
        /// completed synchronously.
        /// </summary>
        public bool CompletedSynchronously
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether the async operation is completed.
        /// </summary>
        public bool IsCompleted
        {
            get
            {
                return this.handle.WaitOne(0, false);
            }
        }

        #endregion

        /// <summary>
        /// Complete asynchronous operation.
        /// </summary>
        public void Complete()
        {
            this.handle.Set();

            if (this.Callback != null)
            {
                // it actually calls Dispatcher.ResponseReceived method.
                this.Callback(this);
            }
        }

        /// <summary>
        /// Dispose the object.
        /// </summary>
        /// <param name="disposing">disposing flag</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (this.handle != null)
                {
                    try
                    {
                        this.handle.Close();
                    }
                    catch (Exception ex)
                    {
                        // ignore the exception
                        BrokerTracing.TraceWarning("[QueueAsyncResult].Dispose: Exception {0}", ex);
                    }

                    this.handle = null;
                }
            }
        }
    }
}
