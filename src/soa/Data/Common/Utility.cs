//------------------------------------------------------------------------------
// <copyright file="Utility.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Utility class
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data
{
    using System;
    using System.Globalization;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.Text.RegularExpressions;
    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Internal;

    /// <summary>
    /// Utility class
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Stores the regex checking file share point
        /// TODO: enhance this.
        /// </summary>
        private static Regex fileSharePointValid = new Regex(@"^\\\\(?<Server>\w[-\w]*)(\\[^<>:""/\\|?*]+)+$");

        /// <summary>
        /// Special treated server name "localhost"
        /// </summary>
        private const string LocalHostName = "localhost";

        /// <summary>
        /// Validate head node name
        /// </summary>
        /// <param name="headNode">head node name</param>
        public static void ValidateHeadNode(string headNode)
        {
            Microsoft.Hpc.Scheduler.Session.Internal.Utility.ThrowIfNullOrEmpty(headNode, "headNode");
        }

        /// <summary>
        /// Validate data client id
        /// </summary>
        /// <param name="dataClientId">data client id</param>
        public static void ValidateDataClientId(string dataClientId)
        {
            Microsoft.Hpc.Scheduler.Session.Internal.Utility.ThrowIfNullOrEmpty(dataClientId, "dataClientId");
            ParamCheckUtility.ThrowIfTooLong(dataClientId.Length, "dataClientId", Internal.Constant.MaxDataClientIdLength, SR.DataClientIdTooLong, Internal.Constant.MaxDataClientIdLength);
            ParamCheckUtility.ThrowIfNotMatchRegex(ParamCheckUtility.DataClientIdValid, dataClientId, "dataClientId", SR.DataClientIdInvalid);
        }

        /// <summary>
        /// Validate session id
        /// </summary>
        /// <param name="sessionId">session id</param>
        public static void ValidateSessionId(int sessionId)
        {
            // validate session Id: either a normal session Id (>0), or -1 for inproc broker.
            if (sessionId <= 0 && sessionId != -1)
            {
                throw new ArgumentException(string.Format(SR.SessionIdInvalid, sessionId), "sessionId");
            }
        }

        /// <summary>
        /// Validate data server information
        /// </summary>
        /// <param name="dsInfo">data server information</param>
        public static void ValidateDataServerInfo(DataServerInfo dsInfo)
        {
            // File share point should be in the form of: \\server\share...
            // and server cannot be "localhost"
            string fileSharePath = dsInfo.AddressInfo.Trim();
            Match m = fileSharePointValid.Match(fileSharePath);
            if (!m.Success ||
                string.Equals(m.Groups["Server"].Value, LocalHostName, StringComparison.InvariantCultureIgnoreCase))
            {
                DataException e = new DataException(DataErrorCode.DataServerMisconfigured, string.Format(SR.InvalidFileShareFormat, dsInfo.AddressInfo));
                e.DataServer = dsInfo.AddressInfo;
                throw e;
            }
        }

        /// <summary>
        /// Translate <see cref="FaultException"/> into general <see cref="Exception"/>
        /// </summary>
        /// <param name="faultException"></param>
        /// <returns></returns>
        public static Exception TranslateFaultException(FaultException<DataFault> faultException)
        {
            int errorCode = faultException.Detail.Code;
            switch (errorCode)
            {
                case DataErrorCode.DataNoPermission:
                    {
                        string errorMsg = SR.ResourceManager.GetString(DataErrorCode.Name(errorCode), CultureInfo.CurrentCulture);
                        return new UnauthorizedAccessException(errorMsg, faultException);
                    }
                case DataErrorCode.InvalidAllowedUser:
                    {
                        string errorMsg = SR.ResourceManager.GetString(DataErrorCode.Name(errorCode), CultureInfo.CurrentCulture);
                        return new IdentityNotMappedException(string.Format(CultureInfo.CurrentCulture, errorMsg, faultException.Detail.Context[0] as string), faultException);
                    }
                default:
                    DataException e = new DataException(errorCode, faultException);
                    e.DataClientId = faultException.Detail.Context[0] as string;
                    e.DataServer = faultException.Detail.Context[1] as string;
                    return e;
            }
        }
    }
}
