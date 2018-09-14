//------------------------------------------------------------------------------
// <copyright file="EmptyDataContainer.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      A read-only data container that contains no data.
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session.Data.DataContainer
{
    using System;

    /// <summary>
    /// A read-only data container that contains no data.
    /// </summary>
    internal class EmptyDataContainer : IDataContainer
    {
        /// <summary>
        /// Gets data container id
        /// </summary>
        public string Id
        {
            get
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Returns a path that tells where the data is stored
        /// </summary>
        /// <returns>path telling where the data is stored</returns>
        public string GetStorePath()
        {
            return string.Empty;
        }

        /// <summary>
        /// Write a data item into data container and flush
        /// </summary>
        /// <param name="data">data content to be written</param>
        public void AddDataAndFlush(DataContent data)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets data content from the data container
        /// </summary>
        /// <returns>data content in the data container</returns>
        public byte[] GetData()
        {
            return null;
        }

        /// <summary>
        /// Gets content md5
        /// </summary>
        /// <returns></returns>
        public string GetContentMd5()
        {
            return string.Empty;
        }

        /// <summary>
        /// Delete the data container.
        /// </summary>
        public void DeleteIfExists()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Check if the data container exists on data server or not
        /// </summary>
        /// <returns>true if the data container exists, false otherwise</returns>
        public bool Exists()
        {
            throw new NotSupportedException();
        }
    }
}
