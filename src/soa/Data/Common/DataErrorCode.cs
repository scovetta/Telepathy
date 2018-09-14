//------------------------------------------------------------------------------
// <copyright file="DataErrorCode.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Data operation error codes
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data
{
    using System.Collections.Generic;
    using System.Reflection;
    using Microsoft.Hpc.Scheduler.Session;

    /// <summary>
    /// Data operation error code
    /// </summary>
    public static class DataErrorCode
    {
        /// <summary>
        /// Operation success
        /// </summary>
        public const int Success = 0x0000;

        /// <summary>
        /// Unknown error
        /// </summary>
        public const int Unknown = (int)SOAFaultCodeCategory.Unknown | 0x0000;

#region DataFatalError
        /// <summary>
        /// Data server type is not supported
        /// </summary>
        public const int DataServerUnsupported = (int)SOAFaultCodeCategory.DataFatalError | 0x0001;

        /// <summary>
        /// Data server is not correctly configured
        /// </summary>
        public const int DataServerMisconfigured = (int)SOAFaultCodeCategory.DataFatalError | 0x0002;

        /// <summary>
        /// Unsupported data version
        /// </summary>
        public const int DataVersionUnsupported = (int)SOAFaultCodeCategory.DataFatalError | 0x0003;

        /// <summary>
        /// No data server is configured
        /// </summary>
        public const int NoDataServerConfigured = (int)SOAFaultCodeCategory.DataFatalError | 0x0004;

        /// <summary>
        /// Data feature is not supported
        /// </summary>
        public const int DataFeatureNotSupported = (int)SOAFaultCodeCategory.DataFatalError | 0x0005;

        /// <summary>
        /// Bad data server address/path
        /// </summary>
        public const int DataServerBadAddress = (int)SOAFaultCodeCategory.DataFatalError | 0x0006;

        /// <summary>
        /// Fail to connect to data service
        /// </summary>
        public const int ConnectDataServiceFailure = (int)SOAFaultCodeCategory.DataFatalError | 0x0007;

        /// <summary>
        /// Connect to data service timed out
        /// </summary>
        public const int ConnectDataServiceTimeout = (int)SOAFaultCodeCategory.DataFatalError | 0x0008;

        /// <summary>
        /// Data server for azure burst is not configured properly
        /// </summary>
        public const int DataServerForAzureBurstMisconfigured = (int)SOAFaultCodeCategory.DataFatalError | 0x0009;

#endregion //DataFatalError

#region DataError
        /// <summary>
        /// Data server is unreachable
        /// </summary>
        public const int DataServerUnreachable = (int)SOAFaultCodeCategory.DataError | 0x0001;

        /// <summary>
        /// Data server is out of space
        /// </summary>
        public const int DataServerNoSpace = (int)SOAFaultCodeCategory.DataError | 0x0002;

        /// <summary>
        /// Data is corrupted.
        /// </summary>
        public const int DataInconsistent = (int)SOAFaultCodeCategory.DataError | 0x0003;

        /// <summary>
        /// Sugguest user to retry on this error
        /// </summary>
        public const int DataRetry = (int)SOAFaultCodeCategory.DataError | 0x0004;

        /// <summary>
        /// Failed to transfer data from on-premise to Azure blob
        /// </summary>
        public const int DataTransferToAzureFailed = (int)SOAFaultCodeCategory.DataError | 0x0005;

#endregion //DataError

#region DataApplicationError
        /// <summary>
        /// DataClient ID is too long
        /// </summary>
        public const int DataClientIdTooLong = (int)SOAFaultCodeCategory.DataApplicationError | 0x0001;

        /// <summary>
        /// DataClient ID contains invalid characters
        /// </summary>
        public const int DataClientIdInvalid = (int)SOAFaultCodeCategory.DataApplicationError | 0x0002;

        /// <summary>
        /// Maximum supported data size exceeded
        /// </summary>
        public const int DataMaxSizeExceeded = (int)SOAFaultCodeCategory.DataApplicationError | 0x0003;

        /// <summary>
        /// DataClient with specified ID already exists
        /// </summary>
        public const int DataClientAlreadyExists = (int)SOAFaultCodeCategory.DataApplicationError | 0x0004;

        /// <summary>
        /// DataClient with specified ID is not found
        /// </summary>
        public const int DataClientNotFound = (int)SOAFaultCodeCategory.DataApplicationError | 0x0005;

        /// <summary>
        /// DataClient is being used by someone else
        /// </summary>
        public const int DataClientBusy = (int)SOAFaultCodeCategory.DataApplicationError | 0x0006;

        /// <summary>
        /// DataClient is being created.
        /// </summary>
        public const int DataClientBeingCreated = (int)SOAFaultCodeCategory.DataApplicationError | 0x0007;

        /// <summary>
        /// DataClient is read only and cannot be written.
        /// </summary>
        public const int DataClientNotWritable = (int)SOAFaultCodeCategory.DataApplicationError | 0x0008;

        /// <summary>
        /// DataClient instance is disposed.
        /// </summary>
        public const int DataClientDisposed = (int)SOAFaultCodeCategory.DataApplicationError | 0x0009;

        /// <summary>
        /// Readback data type doesn't match the type specified by user
        /// </summary>
        public const int DataTypeMismatch = (int)SOAFaultCodeCategory.DataApplicationError | 0x000a;

        /// <summary>
        /// DataClient instance is read only and cannot be modified
        /// </summary>
        public const int DataClientReadOnly = (int)SOAFaultCodeCategory.DataApplicationError | 0x000b;

        /// <summary>
        /// DataLifeCycle has already been set on the DataClient instance
        /// </summary>
        public const int DataClientLifeCycleSet = (int)SOAFaultCodeCategory.DataApplicationError | 0x000c;

        /// <summary>
        /// Failed to perform read/write operation because the DataClient is deleted by someone else.
        /// </summary>
        public const int DataClientDeleted = (int)SOAFaultCodeCategory.DataApplicationError | 0x000d;

        /// <summary>
        /// No data available in the DataClient.
        /// </summary>
        public const int NoDataAvailable = (int)SOAFaultCodeCategory.DataApplicationError | 0x000e;

        /// <summary>
        /// No permission to access the DataClient
        /// </summary>
        public const int DataNoPermission = (int)SOAFaultCodeCategory.DataApplicationError | 0x000f;

        /// <summary>
        /// Invalid allowed user
        /// </summary>
        public const int InvalidAllowedUser = (int)SOAFaultCodeCategory.DataApplicationError | 0x0010;
#endregion //DataApplicationError

        /// <summary>
        /// Stores error code to error code name mapping
        /// </summary>
        private static readonly Dictionary<int, string> errorCodeNameDic;

        /// <summary>
        /// Initializes static members of the DataErrorCode class
        /// </summary>
        static DataErrorCode()
        {
            errorCodeNameDic = new Dictionary<int, string>();
            foreach (FieldInfo fi in typeof(DataErrorCode).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                try
                {
                    int value = (int)fi.GetValue(null);
                    errorCodeNameDic.Add(value, fi.Name);
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Get category of specified error code
        /// </summary>
        /// <param name="errorCode">data error code</param>
        /// <returns>SOAFaultCodeCategory of the specified error code</returns>
        public static SOAFaultCodeCategory Category(int errorCode)
        {
            int category = (int)(errorCode & 0xffff0000);
            switch (category)
            {
                case (int)SOAFaultCodeCategory.DataError:
                case (int)SOAFaultCodeCategory.DataFatalError:
                case (int)SOAFaultCodeCategory.DataApplicationError:
                    return (SOAFaultCodeCategory)category;
                default:
                    return SOAFaultCodeCategory.Unknown;
            }
        }

        /// <summary>
        /// Get name of the specified error code
        /// </summary>
        /// <param name="errorCode">data error code</param>
        /// <returns>Name of the specified error code</returns>
        public static string Name(int errorCode)
        {
            return errorCodeNameDic[errorCode];
        }
    }
}
