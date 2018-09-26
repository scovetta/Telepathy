//------------------------------------------------------------------------------
// <copyright file="AzureUtil.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Helper for dealing with Azure
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Excel
{
    using System;
    using System.IO;

    /// <summary>
    /// Helper for dealing with Azure
    /// </summary>
    internal static class AzureUtil
    {
        /// <summary>
        /// Environment variable set when on Azure node
        /// </summary>
        private static readonly string AZUREEV = "CCP_ONAZURE";

        /// <summary>
        /// Environment variable with Azure Package Root 
        /// </summary>
        private static readonly string PACKAGEEV = "CCP_PACKAGE_ROOT";

        /// <summary>
        /// Check if running on azure
        /// </summary>
        /// <returns>true if on azure</returns>
        internal static bool IsOnAzure()
        {
            return Environment.GetEnvironmentVariable(AZUREEV) == "1";
        }

        /// <summary>
        /// Get path to workbook in package root
        /// </summary>
        /// <param name="workbookName">name of workbook without extension</param>
        /// <returns>path to directory containing workbook or null if directory doesn't exist</returns>
        internal static string GetServiceLocalCacheFullPath(string workbookName)
        {
            string path = Environment.GetEnvironmentVariable(PACKAGEEV);
            if (!string.IsNullOrEmpty(path))
            {
                if (Directory.Exists(path))
                {
                    string result = FindLatestSubDirectory(Path.Combine(path, workbookName));
                    if (!string.IsNullOrEmpty(result) && Directory.Exists(result))
                    {
                        return result;
                    }
                }
            }

            // fall back to actual workbook path
            return null;
        }

        /// <summary>
        /// Get latest sub directory in package dir
        /// </summary>
        /// <param name="parentDir">package directory</param>
        /// <returns>latest sub directory name</returns>
        private static string FindLatestSubDirectory(string parentDir)
        {
            string latestDirectory = string.Empty;
            if (Directory.Exists(parentDir))
            {
                string latestTimeStamp = string.Empty;
                foreach (string subDir in Directory.GetDirectories(parentDir))
                {
                    string directoryName = new DirectoryInfo(subDir).Name;
                    if (string.Compare(directoryName, latestTimeStamp, StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        latestTimeStamp = directoryName;
                        latestDirectory = subDir;
                    }
                }
            }

            return latestDirectory;
        }
    }
}
