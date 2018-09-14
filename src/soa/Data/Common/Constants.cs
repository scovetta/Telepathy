//------------------------------------------------------------------------------
// <copyright file="Constants.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Related constants definition
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data.Internal
{
    /// <summary>
    /// Defines common data related constants
    /// </summary>
    internal static class Constant
    {
        /// <summary>
        /// Max data size supported: 2Gb
        /// </summary>
        public const int MaxDataSize = int.MaxValue;

        /// <summary>
        /// Max DataClient ID length: 128
        /// </summary>
        public const int MaxDataClientIdLength = 128;

        /// <summary>
        /// User name environment variable name
        /// </summary>
        public const string UserNameEnvVar = "CCP_USERNAME";

        /// <summary>
        /// Message header name for storing job id
        /// </summary>
        public const string JobIdHeaderName = "JobId";

        /// <summary>
        /// Message header name for storing job secret
        /// </summary>
        public const string JobSecretHeaderName = "JobSecret";

        /// <summary>
        /// Message header namespace for data feature
        /// </summary>
        public const string DataMessageHeaderNamespace = "Microsoft.Hpc.Scheduler.Data.Internal";

        /// <summary>
        /// DataRequest queue format: "hpccommondata-clustername-request"
        /// </summary>
        public const string DataRequestQueueNameFormat = "hpccommondata-{0}-request";

        /// <summary>
        /// DataResponse queue format: "hpccommondata-custername-proxyid-response"
        /// </summary>
        public const string DataResponseQueueNameFormat = "hpccommondata-{0}-response";

        /// <summary>
        /// Key of "error code" metadata
        /// </summary>
        public const string MetadataKeyErrorCode = "CommonDataErrorCode";

        /// <summary>
        /// Key of "exception" metadata
        /// </summary>
        public const string MetadataKeyException = "CommonDataException";

        /// <summary>
        /// Key of "data synced" metadata
        /// </summary>
        public const string MetadataKeySynced = "CommonDataSynced";

        /// <summary>
        /// Key of "last update time" metadata
        /// </summary>
        public const string MetadataKeyLastUpdateTime = "CommonDataLastUpdateTime";

        /// <summary>
        /// "last update time" metadata update interval: 30 seconds
        /// </summary>
        public const int LastUpdateTimeUpdateIntervalInMilliseconds = 30000;

        /// <summary>
        /// Data proxy endpoint address format
        /// </summary>
        public const string DataProxyEndpointFormat = "net.tcp://{0}:{1}/DataProxy";

        /// <summary>
        /// Data proxy operation timeout
        /// </summary>
        public const int DataProxyOperationTimeoutInMinutes = 5;

        /// <summary>
        /// Data operation timeout
        /// </summary>
        public const int DataOperationTimeoutInMinutes = 5;

        /// <summary>
        /// Attribute key for session id
        /// </summary>
        public const string DataAttributeSessionId = "session";

        /// <summary>
        /// Attribute key for blob unique id
        /// </summary>
        public const string DataAttributeBlobId = "blobid";

        /// <summary>
        /// Attribute key for blob url
        /// </summary>
        public const string DataAttributeBlobUrl = "bloburl";

        /// <summary>
        /// Attribute key for "write done" flag
        /// </summary>
        public const string DataAttributeWriteDone = "writedone";

        /// <summary>
        /// Attribute key for "sync done" flag
        /// </summary>
        public const string DataAttributeSyncDone = "syncdone";

        /// <summary>
        /// Attribute key for "blob primary" flag
        /// </summary>
        public const string DataAttributeBlobPrimary = "blobprimary";
    }
}
