//------------------------------------------------------------------------------
// <copyright file="DataException.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Data operation exceptions
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data
{
    using System;
    using Microsoft.Hpc.Scheduler.Session;
    using System.Globalization;

    /// <summary>
    /// Data exception
    /// </summary>
    [Serializable]
    public class DataException : Exception
    {
        private int errorCode;

        private string dataClientId;

        private string dataServerAddress;

        /// <summary>
        /// Creates a new DataException instance
        /// </summary>
        /// <param name="errorCode">data operation error code</param>
        /// <param name="errorMsg">error message</param>
        public DataException(int errorCode, string errorMsg) :
            base(errorMsg, null)
        {
            this.errorCode = errorCode;
        }

        /// <summary>
        /// Creates a new DataException instance
        /// </summary>
        /// <param name="errorCode">data operation error code</param>
        /// <param name="innerException">detail exception</param>
        public DataException(int errorCode, Exception innerException) :
            base(string.Empty, innerException)
        {
            this.errorCode = errorCode;
        }

        /// <summary>
        /// Creates a new DataException instance
        /// </summary>
        /// <param name="errorCode">data operation error code</param>
        /// <param name="innerException">detail exception</param>
        /// <param name="dataClientId">DataClientId</param>
        public DataException(int errorCode, Exception innerException, string dataClientId) :
            base(string.Empty, innerException)
        {
            this.errorCode = errorCode;
            this.dataClientId = dataClientId;
        }

        /// <summary>
        /// Gets/sets ID of the DataClient on which this exception is thrown
        /// </summary>
        public string DataClientId
        {
            get { return this.dataClientId; }
            set { this.dataClientId = value; }
        }

        /// <summary>
        /// Gets/sets data server information
        /// </summary>
        public string DataServer
        {
            get {return this.dataServerAddress;}
            set { this.dataServerAddress = value; }
        }

        /// <summary>
        /// Gets the data  operation error code
        /// </summary>
        public int ErrorCode
        {
            get { return this.errorCode; }
        }

        /// <summary>
        /// Gets a message that describes this exception
        /// </summary>
        public override string Message
        {
            get
            {
                if(string.IsNullOrEmpty(base.Message))
                {
                    return DataErrorCodeToMessage(errorCode, this.dataClientId, this.dataServerAddress);
                }

                return base.Message;
            }
        }

        /// <summary>
        /// Map data error code to error message
        /// </summary>
        internal static string DataErrorCodeToMessage(int errorCode, string dataClientId, string dataServerAddress)
        {
            switch (errorCode)
            {
                case DataErrorCode.DataServerNoSpace:
                case DataErrorCode.DataFeatureNotSupported:
                case DataErrorCode.NoDataServerConfigured:
                case DataErrorCode.DataServerForAzureBurstMisconfigured:
                case DataErrorCode.DataClientIdInvalid:
                case DataErrorCode.DataClientDisposed:
                case DataErrorCode.ConnectDataServiceFailure:
                case DataErrorCode.ConnectDataServiceTimeout:
                case DataErrorCode.DataRetry:
                    return SR.ResourceManager.GetString(DataErrorCode.Name(errorCode), System.Globalization.CultureInfo.CurrentCulture);
                case DataErrorCode.DataClientAlreadyExists:
                case DataErrorCode.DataClientNotFound:
                case DataErrorCode.DataClientBusy:
                case DataErrorCode.DataClientBeingCreated:
                case DataErrorCode.DataClientNotWritable:
                case DataErrorCode.DataClientReadOnly:
                case DataErrorCode.DataClientLifeCycleSet:
                case DataErrorCode.DataClientDeleted:
                case DataErrorCode.DataInconsistent:
                case DataErrorCode.NoDataAvailable:
                    string strMessage = SR.ResourceManager.GetString(DataErrorCode.Name(errorCode), System.Globalization.CultureInfo.CurrentCulture);
                    return string.Format(CultureInfo.CurrentCulture, strMessage, dataClientId);
                case DataErrorCode.DataServerMisconfigured:
                case DataErrorCode.DataServerUnreachable:
                case DataErrorCode.DataServerBadAddress:
                    string strMessage2 = SR.ResourceManager.GetString(DataErrorCode.Name(errorCode), System.Globalization.CultureInfo.CurrentCulture);
                    return string.Format(CultureInfo.CurrentCulture, strMessage2, dataServerAddress);
                case DataErrorCode.DataServerUnsupported:
                case DataErrorCode.DataMaxSizeExceeded:
                case DataErrorCode.DataVersionUnsupported:
                case DataErrorCode.DataClientIdTooLong:
                case DataErrorCode.DataTypeMismatch:
                    {
                        // these error codes requires more information to build error message.
                        System.Diagnostics.Debug.Assert(false, string.Format("cannot build error message for error code {0}", errorCode));
                        return SR.DataUnknownError;
                    }
                default:
                    return SR.DataUnknownError;

            }
        }
    }
}
