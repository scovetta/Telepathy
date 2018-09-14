//--------------------------------------------------------------------------
// <copyright file="IFileStagingClient.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     This interface defines the service contract for the File Staging
//     service.
// </summary>
//--------------------------------------------------------------------------

namespace Microsoft.Hpc.Azure.FileStaging.Client
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.ServiceModel;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    /// <summary>
    /// Service contract for the file staging service
    /// </summary>
    [ServiceContract(Name = "IFileStaging", Namespace = "http://hpc.microsoft.com/filestaging/")]
    public interface IFileStagingClient
    {
        /// <summary>
        /// Read contents of file
        /// </summary>
        /// <returns>a stream that can be used to read contents of the file</returns>
        [OperationContract(Action = "http://hpc.microsoft.com/IFileStaging/ReadFile", ReplyAction = "http://hpc.microsoft.com/IFileStaging/ReadFileResponse")]
        [FaultContract(typeof(InternalFaultDetail))]
        Stream ReadFile();

        /// <summary>
        /// Write contents in a stream to the target file
        /// </summary>
        /// <param name="contents">stream to be written</param>
        [OperationContract(Action = "http://hpc.microsoft.com/IFileStaging/WriteFile", ReplyAction = "http://hpc.microsoft.com/IFileStaging/WriteFileResponse")]
        [FaultContract(typeof(InternalFaultDetail))]
        void WriteFile(Stream contents);

        /// <summary>
        /// Delete the target file
        /// </summary>
        [OperationContract(Action = "http://hpc.microsoft.com/IFileStaging/DeleteFile", ReplyAction = "http://hpc.microsoft.com/IFileStaging/DeleteFileResponse")]
        [FaultContract(typeof(InternalFaultDetail))]
        void DeleteFile();

        /// <summary>
        /// Returns an array of directories that matches specified search pattern and search option under target directory
        /// </summary>
        /// <param name="searchPattern">search pattern</param>
        /// <param name="searchOption">search option</param>
        /// <returns>an array of directories that match specified search pattern and search option under target directory</returns>
        [OperationContract(Action = "http://hpc.microsoft.com/IFileStaging/GetDirectories", ReplyAction = "http://hpc.microsoft.com/IFileStaging/GetDirectoriesResponse")]
        [FaultContract(typeof(InternalFaultDetail))]
        ClusterDirectoryInfo[] GetDirectories(string searchPattern, SearchOption searchOption);

        /// <summary>
        /// Delete the target directory
        /// </summary>
        /// <param name="recursive">if delete the directory recursively. if set to false, only empty directory can be deleted</param>
        [OperationContract(Action = "http://hpc.microsoft.com/IFileStaging/DeleteDirectory", ReplyAction = "http://hpc.microsoft.com/IFileStaging/DeleteDirectoryResponse")]
        [FaultContract(typeof(InternalFaultDetail))]
        void DeleteDirectory(bool recursive);

        /// <summary>
        /// Returns an array of files that matches specified search pattern and search option under target directory
        /// </summary>
        /// <param name="searchPattern">search pattern</param>
        /// <param name="searchOption">search option</param>
        /// <returns>an array of files that match specified search pattern and search option under target directory</returns>
        [OperationContract(Action = "http://hpc.microsoft.com/IFileStaging/GetFiles", ReplyAction = "http://hpc.microsoft.com/IFileStaging/GetFilesResponse")]
        [FaultContract(typeof(InternalFaultDetail))]
        ClusterFileInfo[] GetFiles(string searchPattern, SearchOption searchOption);

        /// <summary>
        /// Copy a file from local to an Azure blob with the specified blob url
        /// </summary>
        /// <param name="blobUrl">destination blob url</param>
        /// <param name="sas">SAS for accessing the blob</param>
        /// <param name="sourceFilePath">source file path</param>
        [OperationContract(Action = "http://hpc.microsoft.com/IFileStaging/CopyFileToBlob", ReplyAction = "http://hpc.microsoft.com/IFileStaging/CopyFileToBlobResponse")]
        [FaultContract(typeof(InternalFaultDetail))]
        void CopyFileToBlob(string blobUrl, string sas, string sourceFilePath);

        /// <summary>
        /// Copy a file from Azure blob to local
        /// </summary>
        /// <param name="blobUrl">source blob url</param>
        /// <param name="sas">SAS for accessing the blob</param>
        /// <param name="destFilePath">destination file path</param>
        /// <param name="overwrite">if overwrite existing files</param>
        [OperationContract(Action = "http://hpc.microsoft.com/IFileStaging/CopyFileFromBlob", ReplyAction = "http://hpc.microsoft.com/IFileStaging/CopyFileFromBlobResponse")]
        [FaultContract(typeof(InternalFaultDetail))]
        void CopyFileFromBlob(string blobUrl, string sas, string destFilePath, bool overwrite);

        /// <summary>
        /// Copy files that match specified file patterns under sourceDir to Azure blobs with the specified blob url prefix
        /// </summary>
        /// <param name="blobUrlPrefix">blob url prefix</param>
        /// <param name="sas">SAS for accessing the blob</param>
        /// <param name="sourceDir">source directory path</param>
        /// <param name="filePatterns">file patterns</param>
        /// <param name="recursive">if copy directories to blob recursively</param>
        /// <param name="overwrite">if overwrite existing blobs</param>
        [OperationContract(Action = "http://hpc.microsoft.com/IFileStaging/CopyDirectoryToBlob", ReplyAction = "http://hpc.microsoft.com/IFileStaging/CopyDirectoryToBlobResponse")]
        [FaultContract(typeof(InternalFaultDetail))]
        void CopyDirectoryToBlob(string blobUrlPrefix, string sas, string sourceDir, List<string> filePatterns, bool recursive, bool overwrite);

        /// <summary>
        /// Copy blobs that match specified file patterns on blob storage to destDir
        /// </summary>
        /// <param name="blobUrlPrefix">blob url prefix</param>
        /// <param name="sas">SAS for accessing the blob</param>
        /// <param name="destDir">destination directory path</param>
        /// <param name="filePatterns">file patterns</param>
        /// <param name="recursive">if copy blobs recursively</param>
        /// <param name="overwrite">if overwrite existing files</param>
        [OperationContract(Action = "http://hpc.microsoft.com/IFileStaging/CopyDirectoryFromBlob", ReplyAction = "http://hpc.microsoft.com/IFileStaging/CopyDirectoryFromBlobResponse")]
        [FaultContract(typeof(InternalFaultDetail))]
        void CopyDirectoryFromBlob(string blobUrlPrefix, string sas, string destDir, List<string> filePatterns, bool recursive, bool overwrite);

        /// <summary>
        /// Returns an URL pointing to user's container on the intermediate blob
        /// storage, and an Azure SAS for accessing the container
        /// </summary>
        /// <param name="sas">Azure SAS for accessing user's container</param>
        /// <returns>an URL pointing to user's container on the intermediate blob storage</returns>
        [OperationContract(Action = "http://hpc.microsoft.com/IFileStaging/GetContainerUrl", ReplyAction = "http://hpc.microsoft.com/IFileStaging/GetContainerUrlResponse")]
        [FaultContract(typeof(InternalFaultDetail))]
        string GetContainerUrl(out string sas);

        /// <summary>
        /// Returns an Azure SAS that grants specified permissions to a blob under user's container
        /// </summary>
        /// <param name="blobName">target blob name</param>
        /// <param name="permissions">permissions to be granted by the SAS</param>
        /// <returns>an Azure SAS that grants specified permissions to the target blob</returns>
        [OperationContract(Action = "http://hpc.microsoft.com/IFileStaging/GenerateSASForBlob", ReplyAction = "http://hpc.microsoft.com/IFileStaging/GenerateSASForBlobResponse")]
        [FaultContract(typeof(InternalFaultDetail))]
        string GenerateSASForBlob(string blobName, SharedAccessBlobPermissions permissions);
    }
}
