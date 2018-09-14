//------------------------------------------------------------------------------
// <copyright file="IDataContainer.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Data container interface definition
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data.DataContainer
{
    /// <summary>
    /// Data container interface definition
    /// </summary>
    internal interface IDataContainer
    {
        /// <summary>
        /// Gets data container id
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Returns a path that tells where the data is stored
        /// </summary>
        /// <returns>path telling where the data is stored</returns>
        string GetStorePath();

        /// <summary>
        /// Get the content Md5
        /// </summary>
        /// <returns>The base64 md5 string</returns>
        string GetContentMd5();

        /// <summary>
        /// Write a data item into data container and flush
        /// </summary>
        /// <param name="data">data content to be written</param>
        void AddDataAndFlush(DataContent data);

        /// <summary>
        /// Gets data content from the data container
        /// </summary>
        /// <returns>data content in the data container</returns>
        byte[] GetData();

        /// <summary>
        /// Delete the data container.
        /// </summary>
        void DeleteIfExists();

        /// <summary>
        /// Check if the data container exists on data server or not
        /// </summary>
        /// <returns>true if the data container exists, false otherwise</returns>
        bool Exists();
    }
}
