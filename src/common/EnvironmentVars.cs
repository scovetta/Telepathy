//--------------------------------------------------------------------------
// <copyright file="EnvironmentVars.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     This is a common class for all Environment Variables defined in HPC
//     Together with default environment variable values
// </summary>
//--------------------------------------------------------------------------

namespace Microsoft.Hpc
{
    /// <summary>
    /// This is a common class for all Environment Variables defined in HPC
    /// Together with default environment variable values
    /// </summary>
    internal class EnvironmentVars
    {
        /// <summary>
        /// Environment variable for log fie location for system code
        /// </summary>
        public const string SystemLogRoot = "CCP_LOGROOT_SYS";

        /// <summary>
        /// Default value for log file location for system code, on premise
        /// </summary>
        public const string SystemLogRootDefaultOnPremise = "%CCP_DATA%LogFiles";

        /// <summary>
        /// Default value for log file location for system code, in Azure
        /// </summary>
        public const string SystemLogRootDefaultAzure = @"c:\logs\Hpc";

        /// <summary>
        /// Environment variable for log file location for user code
        /// </summary>
        public const string UserLogRoot = "CCP_LOGROOT_USR";

        /// <summary>
        /// Default value for log file location for user code, on premise and in Azure
        /// </summary>
        public const string UserLogRootDefault = @"%LOCALAPPDATA%\Microsoft\Hpc\LogFiles";
    }
}
