//------------------------------------------------------------------------------
// <copyright file="DataContainerHelper.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Data container helper
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data.DataContainer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Hpc.Scheduler.Session.Data.Internal;

    /// <summary>
    /// Data container helper
    /// </summary>
    internal static class DataContainerHelper
    {
        /// <summary>
        /// Maximum number of data container subdirectories
        /// </summary>
        private const int MaxSubDirectoryCount = 1024;

        /// <summary>
        /// Http prefix
        /// </summary>
        private const string HttpPrefix = "http://";

        /// <summary>
        /// Https prefix
        /// </summary>
        private const string HttpsPrefix = "https://";

        /// <summary>
        /// Check whether a container path is a blob
        /// </summary>
        /// <param name="containerPath">the container path</param>
        /// <returns>true or false</returns>
        public static bool IsBlobBasedDataContainerPath(string containerPath)
        {
            if (containerPath.StartsWith(HttpPrefix, StringComparison.InvariantCultureIgnoreCase) ||
                containerPath.StartsWith(HttpsPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }
        /// <summary>
        /// Create an IDataContainer instance according to container path
        /// </summary>
        /// <param name="containerPath">data container path</param>
        /// <returns>IDataContainer instance that refers to specified container path</returns>
        public static IDataContainer GetDataContainer(string containerPath)
        {
            if (string.IsNullOrEmpty(containerPath))
            {
                return new EmptyDataContainer();
            }

            // Currently we support FileDataContainer and BlobDataContainer.
            if(IsBlobBasedDataContainerPath(containerPath))
            {
                return new BlobBasedDataContainer(containerPath);
            }
            else
            {
                return new FileDataContainer(containerPath);
            }
        }

        /// <summary>
        /// Create an IDataContainer instance that refers to one primary container path and one secondary container path
        /// </summary>
        /// <param name="primaryContainerPath">primay container path</param>
        /// <param name="secondaryContainerPath">secondary container path</param>
        /// <returns>IDataContainer instance that refers to specified paths</returns>
        public static IDataContainer GetDataContainer(string primaryContainerPath, string secondaryContainerPath)
        {
            return new ComplexDataContainer(primaryContainerPath, new List<string>() { secondaryContainerPath });
        }

        /// <summary>
        /// Open data container locally.
        /// Note: this method is introduced to avoid invoking OpenDataClient call onto data service. 
        /// This is a shortcut for ServiceContext use only.
        /// </summary>
        /// <param name="dsInfo">data server info</param>
        /// <param name="containerName">data container name</param>
        /// <returns>data container store path</returns>
        public static string OpenDataContainer(DataServerInfo dsInfo, string containerName)
        {
            if (dsInfo == null)
            {
                throw new DataException(DataErrorCode.NoDataServerConfigured, string.Empty);
            }

            Utility.ValidateDataServerInfo(dsInfo);

            try
            {
                string containerPath = GenerateDataContainerPath(dsInfo, containerName);
                FileDataContainer.CheckReadPermission(containerPath);
                return containerPath;
            }
            catch (DataException e)
            {
                e.DataServer = dsInfo.AddressInfo;
                e.DataClientId = containerName;
                throw;
            }
        }

        /// <summary>
        /// Generate data container store path from data server info and data container name
        /// </summary>
        /// <param name="dsInfo">data server info</param>
        /// <param name="containerName">data container name</param>
        /// <returns>store path of the data container</returns>
        private static string GenerateDataContainerPath(DataServerInfo dsInfo, string containerName)
        {
            // Note: below code snippet is copied from data service code.
            int hashValue = 0;
            for (int i = 0; i < containerName.Length; i++)
            {
                hashValue = (hashValue * 37) + containerName[i];
            }

            hashValue = Math.Abs(hashValue);
            hashValue = hashValue % MaxSubDirectoryCount;

            string containerPath = Path.Combine(dsInfo.AddressInfo, hashValue.ToString());
            containerPath = Path.Combine(containerPath, containerName);
            return containerPath;
        }
    }
}
