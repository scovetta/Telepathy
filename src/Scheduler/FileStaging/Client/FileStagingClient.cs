//--------------------------------------------------------------------------
// <copyright file="FileStagingClient.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     FileStaging client
// </summary>
//--------------------------------------------------------------------------

namespace Microsoft.Hpc.Azure.FileStaging
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using Microsoft.Hpc.Azure.FileStaging.Client;
    using Microsoft.WindowsAzure.Storage.Blob;

    /// <summary>
    /// File staging client
    /// </summary>
    public class FileStagingClient : ClientBase<IFileStagingClientAsync>, IFileStagingClient
    {
        /// <summary>
        /// Initializes a new instance of the FileStagingClient class
        /// </summary>
        /// <param name="binding">binding information</param>
        /// <param name="remoteAddress">address of file staging service</param>
        public FileStagingClient(Binding binding, EndpointAddress endpoint) : base(binding, endpoint)
        {
        }

        /// <summary>
        /// Read contents of file
        /// </summary>
        /// <returns>a stream that can be used to read contents of the file</returns>
        public Stream ReadFile()
        {
            // Call async version and block on completion in order to workaround System.Net.Socket bug #750028
            IAsyncResult result = this.Channel.BeginReadFile(null, null);
            return this.Channel.EndReadFile(result);
        }

        /// <summary>
        /// Write contents in a stream to the target file
        /// </summary>
        /// <param name="contents">stream to be written</param>
        public void WriteFile(Stream contents)
        {
            // Call async version and block on completion in order to workaround System.Net.Socket bug #750028
            IAsyncResult result = this.Channel.BeginWriteFile(contents, null, null);
            this.Channel.EndWriteFile(result);
        }

        /// <summary>
        /// Delete the target file
        /// </summary>
        public void DeleteFile()
        {
            // Call async version and block on completion in order to workaround System.Net.Socket bug #750028
            IAsyncResult result = this.Channel.BeginDeleteFile(null, null);
            this.Channel.EndDeleteFile(result);
        }

        /// <summary>
        /// Returns an array of directories that matches specified search pattern and search option under target directory
        /// </summary>
        /// <param name="searchPattern">search pattern</param>
        /// <param name="searchOption">search option</param>
        /// <returns>an array of directories that match specified search pattern and search option under target directory</returns>
        public ClusterDirectoryInfo[] GetDirectories(string searchPattern, SearchOption searchOption)
        {
            // Call async version and block on completion in order to workaround System.Net.Socket bug #750028
            IAsyncResult result = this.Channel.BeginGetDirectories(searchPattern, searchOption, null, null);
            return this.Channel.EndGetDirectories(result);
        }

        /// <summary>
        /// Delete the target directory
        /// </summary>
        /// <param name="recursive">if delete the directory recursively. if set to false, only empty directory can be deleted</param>
        public void DeleteDirectory(bool recursive)
        {
            // Call async version and block on completion in order to workaround System.Net.Socket bug #750028
            IAsyncResult result = this.Channel.BeginDeleteDirectory(recursive, null, null);
            this.Channel.EndDeleteDirectory(result);
        }

        /// <summary>
        /// Returns an array of files that matches specified search pattern and search option under target directory
        /// </summary>
        /// <param name="searchPattern">search pattern</param>
        /// <param name="searchOption">search option</param>
        /// <returns>an array of files that match specified search pattern and search option under target directory</returns>
        public ClusterFileInfo[] GetFiles(string searchPattern, SearchOption searchOption)
        {
            // Call async version and block on completion in order to workaround System.Net.Socket bug #750028
            IAsyncResult result = this.Channel.BeginGetFiles(searchPattern, searchOption, null, null);
            return this.Channel.EndGetFiles(result);
        }

        /// <summary>
        /// Copy a file from local to an Azure blob with the specified blob url
        /// </summary>
        /// <param name="blobUrl">destination blob url</param>
        /// <param name="sas">SAS for accessing the blob</param>
        /// <param name="sourceFilePath">source file path</param>
        public void CopyFileToBlob(string blobUrl, string sas, string sourceFilePath)
        {
            IAsyncResult result = this.Channel.BeginCopyFileToBlob(blobUrl, sas, sourceFilePath, null, null);
            this.Channel.EndCopyFileToBlob(result);
        }

        /// <summary>
        /// Copy a file from Azure blob to local
        /// </summary>
        /// <param name="blobUrl">source blob url</param>
        /// <param name="sas">SAS for accessing the blob</param>
        /// <param name="destFilePath">destination file path</param>
        /// <param name="overwrite">if overwrite existing files</param>
        public void CopyFileFromBlob(string blobUrl, string sas, string destFilePath, bool overwrite)
        {
            IAsyncResult result = this.Channel.BeginCopyFileFromBlob(blobUrl, sas, destFilePath, overwrite, null, null);
            this.Channel.EndCopyFileFromBlob(result);
        }

        /// <summary>
        /// Copy files that match specified file patterns under sourceDir to Azure blobs with the specified blob url prefix
        /// </summary>
        /// <param name="blobUrlPrefix">blob url prefix</param>
        /// <param name="sas">SAS for accessing the blob</param>
        /// <param name="sourceDir">source directory path</param>
        /// <param name="filePatterns">file patterns</param>
        /// <param name="recursive">if copy directories to blob recursively</param>
        /// <param name="overwrite">if overwrite existing blobs</param>
        public void CopyDirectoryToBlob(string blobUrlPrefix, string sas, string sourceDir, List<string> filePatterns, bool recursive, bool overwrite)
        {
            IAsyncResult result = this.Channel.BeginCopyDirectoryToBlob(blobUrlPrefix, sas, sourceDir, filePatterns, recursive, overwrite, null, null);
            this.Channel.EndCopyDirectoryToBlob(result);
        }

        /// <summary>
        /// Copy blobs that match specified file patterns on blob storage to destDir
        /// </summary>
        /// <param name="blobUrlPrefix">blob url prefix</param>
        /// <param name="sas">SAS for accessing the blob</param>
        /// <param name="destDir">destination directory path</param>
        /// <param name="filePatterns">file patterns</param>
        /// <param name="recursive">if copy blobs recursively</param>
        /// <param name="overwrite">if overwrite existing files</param>
        public void CopyDirectoryFromBlob(string blobUrlPrefix, string sas, string destDir, List<string> filePatterns, bool recursive, bool overwrite)
        {
            IAsyncResult result = this.Channel.BeginCopyDirectoryFromBlob(blobUrlPrefix, sas, destDir, filePatterns, recursive, overwrite, null, null);
            this.Channel.EndCopyDirectoryFromBlob(result);
        }

        /// <summary>
        /// Returns an URL pointing to user's container on the intermediate blob
        /// storage, and an Azure SAS for accessing the container
        /// </summary>
        /// <param name="sas">Azure SAS for accessing user's container</param>
        /// <returns>an URL pointing to user's container on the intermediate blob storage</returns>
        public string GetContainerUrl(out string sas)
        {
            // Call async version and block on completion in order to workaround System.Net.Socket bug #750028
            IAsyncResult result = this.Channel.BeginGetContainerUrl(null, null);
            return this.Channel.EndGetContainerUrl(out sas, result);
        }

        /// <summary>
        /// Returns an Azure SAS that grants specified permissions to a blob under user's container
        /// </summary>
        /// <param name="blobName">target blob name</param>
        /// <param name="permissions">permissions to be granted by the SAS</param>
        /// <returns>an Azure SAS that grants specified permissions to the target blob</returns>
        public string GenerateSASForBlob(string blobName, SharedAccessBlobPermissions permissions)
        {
            // Call async version and block on completion in order to workaround System.Net.Socket bug #750028
            IAsyncResult result = this.Channel.BeginGenerateSASForBlob(blobName, permissions, null, null);
            return this.Channel.EndGenerateSASForBlob(result);
        }
    }
}
