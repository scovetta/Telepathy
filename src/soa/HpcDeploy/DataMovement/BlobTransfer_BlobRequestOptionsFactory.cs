//------------------------------------------------------------------------------
// <copyright file="BlobTransfer_BlobRequestOptionsFactory.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      BlobRequestOptions Factory for BlobTransfer class.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;
    using Microsoft.WindowsAzure.Storage;

    /// <summary>
    /// BlobRequestOptions Factory for BlobTransfer class.
    /// </summary>
    internal static class BlobTransfer_BlobRequestOptionsFactory
    {
        /// <summary>
        /// Stores the default maximum execution time across all potential retries. 
        /// </summary>
        private static TimeSpan defaultMaximumExecutionTime =
            TimeSpan.FromSeconds(900);

        /// <summary>
        /// Stores the default client retry count in x-ms error.
        /// </summary>
        private static int defaultRetryCountXMsError = 10;

        /// <summary>
        /// Stores the default client retry count in non x-ms error.
        /// </summary>
        private static int defaultRetryCountOtherError = 3;

        /// <summary>
        /// Stores the default backoff.
        /// Increases exponentially used with ExponentialRetry: 3, 9, 21, 45, 93, 120, 120, 120, ...
        /// </summary>
        private static TimeSpan retryPoliciesDefaultBackoff =
            TimeSpan.FromSeconds(3.0);

        /// <summary>
        /// Stores the default retry policy.
        /// </summary>
        private static IRetryPolicy defaultRetryPolicy = 
            new BlobTransferRetryPolicy(
                retryPoliciesDefaultBackoff,
                defaultRetryCountXMsError,
                defaultRetryCountOtherError);

        static BlobTransfer_BlobRequestOptionsFactory()
        {
            CreateContainerRequestOptions = new BlobRequestOptions()
            {
                MaximumExecutionTime = defaultMaximumExecutionTime,
                RetryPolicy = defaultRetryPolicy,
                ServerTimeout = TimeSpan.FromSeconds(15)
            };

            ListBlobsRequestOptions = new BlobRequestOptions()
            {
                MaximumExecutionTime = defaultMaximumExecutionTime,
                RetryPolicy = defaultRetryPolicy,
                ServerTimeout = TimeSpan.FromSeconds(60)
            };

            CreatePageBlobRequestOptions = new BlobRequestOptions()
            {
                MaximumExecutionTime = defaultMaximumExecutionTime,
                RetryPolicy = defaultRetryPolicy,
                ServerTimeout = TimeSpan.FromSeconds(15)
            };

            DeleteRequestOptions = new BlobRequestOptions()
            {
                MaximumExecutionTime = defaultMaximumExecutionTime,
                RetryPolicy = defaultRetryPolicy,
                ServerTimeout = TimeSpan.FromSeconds(15)
            };

            FetchAttributesRequestOptions = new BlobRequestOptions()
            {
                MaximumExecutionTime = defaultMaximumExecutionTime,
                RetryPolicy = defaultRetryPolicy,
                ServerTimeout = TimeSpan.FromSeconds(15)
            };

            GetPageRangesRequestOptions = new BlobRequestOptions()
            {
                MaximumExecutionTime = defaultMaximumExecutionTime,
                RetryPolicy = defaultRetryPolicy,
                ServerTimeout = TimeSpan.FromSeconds(60)
            };

            OpenReadRequestOptions = new BlobRequestOptions()
            {
                MaximumExecutionTime = defaultMaximumExecutionTime,
                RetryPolicy = defaultRetryPolicy,
                ServerTimeout = TimeSpan.FromSeconds(300),
                UseTransactionalMD5 = true
            };

            PutBlockRequestOptions = new BlobRequestOptions()
            {
                MaximumExecutionTime = defaultMaximumExecutionTime,
                RetryPolicy = defaultRetryPolicy,
                ServerTimeout = TimeSpan.FromSeconds(300),
                UseTransactionalMD5 = true
            };

            PutBlockListRequestOptions = new BlobRequestOptions()
            {
                MaximumExecutionTime = defaultMaximumExecutionTime,
                RetryPolicy = defaultRetryPolicy,
                ServerTimeout = TimeSpan.FromSeconds(60)
            };

            DownloadBlockListRequestOptions = new BlobRequestOptions()
            {
                MaximumExecutionTime = defaultMaximumExecutionTime,
                RetryPolicy = defaultRetryPolicy,
                ServerTimeout = TimeSpan.FromSeconds(60)
            };

            SetMetadataRequestOptions = new BlobRequestOptions()
            {
                MaximumExecutionTime = defaultMaximumExecutionTime,
                RetryPolicy = defaultRetryPolicy,
                ServerTimeout = TimeSpan.FromSeconds(15)
            };

            WritePagesRequestOptions = new BlobRequestOptions()
            {
                MaximumExecutionTime = defaultMaximumExecutionTime,
                RetryPolicy = defaultRetryPolicy,
                ServerTimeout = TimeSpan.FromSeconds(300),
                UseTransactionalMD5 = true
            };

            ClearPagesRequestOptions = new BlobRequestOptions()
            {
                MaximumExecutionTime = defaultMaximumExecutionTime,
                RetryPolicy = defaultRetryPolicy,
                ServerTimeout = TimeSpan.FromSeconds(300)
            };

            GetBlobReferenceFromServerRequestOptions = new BlobRequestOptions()
            {
                MaximumExecutionTime = defaultMaximumExecutionTime,
                RetryPolicy = defaultRetryPolicy,
                ServerTimeout = TimeSpan.FromSeconds(60)
            };

            StartCopyFromBlobRequestOptions = new BlobRequestOptions()
            {
                MaximumExecutionTime = defaultMaximumExecutionTime,
                RetryPolicy = defaultRetryPolicy,
                ServerTimeout = TimeSpan.FromSeconds(120)
            };
        }

        public static BlobRequestOptions CreateContainerRequestOptions
        {
            get;
            private set;
        }

        public static BlobRequestOptions ListBlobsRequestOptions
        {
            get;
            private set;
        }

        public static BlobRequestOptions CreatePageBlobRequestOptions
        {
            get;
            private set;
        }

        public static BlobRequestOptions DeleteRequestOptions
        {
            get;
            private set;
        }

        public static BlobRequestOptions FetchAttributesRequestOptions
        {
            get;
            private set;
        }

        public static BlobRequestOptions GetPageRangesRequestOptions
        {
            get;
            private set;
        }

        public static BlobRequestOptions OpenReadRequestOptions
        {
            get;
            private set;
        }

        public static BlobRequestOptions PutBlockRequestOptions
        {
            get;
            private set;
        }

        public static BlobRequestOptions PutBlockListRequestOptions
        {
            get;
            private set;
        }

        public static BlobRequestOptions DownloadBlockListRequestOptions
        {
            get;
            private set;
        }

        public static BlobRequestOptions SetMetadataRequestOptions
        {
            get;
            private set;
        }

        public static BlobRequestOptions WritePagesRequestOptions
        {
            get;
            private set;
        }

        public static BlobRequestOptions ClearPagesRequestOptions
        {
            get;
            private set;
        }

        public static BlobRequestOptions GetBlobReferenceFromServerRequestOptions
        {
            get;
            private set;
        }

        public static BlobRequestOptions StartCopyFromBlobRequestOptions
        {
            get;
            private set;
        }

        /// <summary>
        /// Define retry policy used in blob transfer.
        /// </summary>
        private class BlobTransferRetryPolicy : IRetryPolicy
        {
            /// <summary>
            /// Prefix of Azure Storage reponse keys.
            /// </summary>
            private const string XMsPrefix = "x-ms";

            /// <summary>
            /// Max retry count in non x-ms error.
            /// </summary>
            private int maxAttemptsOtherError;

            /// <summary>
            /// ExponentialRetry retry policy object.
            /// </summary>
            private ExponentialRetry retryPolicy;

            /// <summary>
            /// Indicate whether has met x-ms once or more.
            /// </summary>
            private bool gotXMsError = false;

            /// <summary>
            /// Initializes a new instance of the <see cref="BlobTransferRetryPolicy"/> class.
            /// </summary>
            /// <param name="deltaBackoff">Backoff in ExponentialRetry retry policy.</param>
            /// <param name="maxAttemptsXMsError">Max retry count when meets x-ms error.</param>
            /// <param name="maxAttemptsOtherError">Max retry count when meets non x-ms error.</param>
            public BlobTransferRetryPolicy(TimeSpan deltaBackoff, int maxAttemptsXMsError, int maxAttemptsOtherError)
            {
                Debug.Assert(
                    maxAttemptsXMsError >= maxAttemptsOtherError,
                    "We should retry more times when meets x-ms errors than the other errors.");

                this.retryPolicy = new ExponentialRetry(deltaBackoff, maxAttemptsXMsError);
                this.maxAttemptsOtherError = maxAttemptsOtherError;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="BlobTransferRetryPolicy"/> class.
            /// </summary>
            /// <param name="retryPolicy">ExponentialRetry object.</param>
            /// <param name="maxAttemptsInOtherError">Max retry count when meets non x-ms error.</param>
            private BlobTransferRetryPolicy(ExponentialRetry retryPolicy, int maxAttemptsInOtherError)
            {
                this.retryPolicy = retryPolicy;
                this.maxAttemptsOtherError = maxAttemptsInOtherError;
            }

            /// <summary>
            /// Generates a new retry policy for the current request attempt.
            /// </summary>
            /// <returns>An IRetryPolicy object that represents the retry policy for the current request attempt.</returns>
            public IRetryPolicy CreateInstance()
            {
                return new BlobTransferRetryPolicy(
                    this.retryPolicy.CreateInstance() as ExponentialRetry, 
                    this.maxAttemptsOtherError);
            }

            /// <summary>
            /// Determines if the operation should be retried and how long to wait until the next retry.
            /// </summary>
            /// <param name="currentRetryCount">The number of retries for the given operation.</param>
            /// <param name="statusCode">The status code for the last operation.</param>
            /// <param name="lastException">An Exception object that represents the last exception encountered.</param>
            /// <param name="retryInterval">The interval to wait until the next retry.</param>
            /// <param name="operationContext">An OperationContext object for tracking the current operation.</param>
            /// <returns> true if the operation should be retried; otherwise, false. 
            /// </returns>
            public bool ShouldRetry(
                int currentRetryCount,
                int statusCode,
                Exception lastException,
                out TimeSpan retryInterval,
                OperationContext operationContext)
            {
                if (!this.retryPolicy.ShouldRetry(currentRetryCount, statusCode, lastException, out retryInterval, operationContext))
                {
                    return false;
                }

                if (this.gotXMsError)
                {
                    return true;
                }

                StorageException storageException = lastException as StorageException;

                if (null != storageException)
                {
                    WebException webException = storageException.InnerException as WebException;

                    if (null != webException)
                    {
                        if (WebExceptionStatus.ConnectionClosed == webException.Status)
                        {
                            return true;
                        }

                        HttpWebResponse response = webException.Response as HttpWebResponse;

                        if (null != response)
                        {
                            if (null != response.Headers)
                            {
                                if (null != response.Headers.AllKeys)
                                {
                                    for (int i = 0; i < response.Headers.AllKeys.Length; ++i)
                                    {
                                        if (response.Headers.AllKeys[i].StartsWith(XMsPrefix))
                                        {
                                            this.gotXMsError = true;
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (currentRetryCount < this.maxAttemptsOtherError)
                {
                    return true;
                }

                return false;
            }
        }
    }
}
