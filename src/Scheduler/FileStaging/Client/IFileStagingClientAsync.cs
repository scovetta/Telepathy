//--------------------------------------------------------------------------
// <copyright file="IFileStagingClientAsync.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     Async version of the File Staging service interface.
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
    /// Async version of the File Staging service interface.
    /// </summary>
    [ServiceContract(Name = "IFileStaging", Namespace = "http://hpc.microsoft.com/filestaging/")]
    public interface IFileStagingClientAsync
    {
        /// <summary>
        /// Begin method of ReadFile
        /// </summary>
        /// <param name="callback">async callback</param>
        /// <param name="asyncState">async state</param>
        /// <returns>async result</returns>
        [OperationContract(AsyncPattern = true, Action = "http://hpc.microsoft.com/IFileStaging/ReadFile", ReplyAction = "http://hpc.microsoft.com/IFileStaging/ReadFileResponse")]
        [FaultContract(typeof(InternalFaultDetail))]
        IAsyncResult BeginReadFile(AsyncCallback callback, object asyncState);

        /// <summary>
        /// End method of ReadFile
        /// </summary>
        /// <param name="result">async result</param>
        /// <returns>a stream that can be used to read contents of the file</returns>
        Stream EndReadFile(IAsyncResult result);

        /// <summary>
        /// Begin method of WriteFile
        /// </summary>
        /// <param name="contents">stream to be written</param>
        /// <param name="callback">async callback</param>
        /// <param name="asyncState">async state</param>
        /// <returns>async result</returns>
        [OperationContract(AsyncPattern = true, Action = "http://hpc.microsoft.com/IFileStaging/WriteFile", ReplyAction = "http://hpc.microsoft.com/IFileStaging/WriteFileResponse")]
        [FaultContract(typeof(InternalFaultDetail))]
        IAsyncResult BeginWriteFile(Stream contents, AsyncCallback callback, object asyncState);

        /// <summary>
        /// End method of WriteFile
        /// </summary>
        /// <param name="result">async result</param>
        void EndWriteFile(IAsyncResult result);

        /// <summary>
        /// Begin method of DeleteFile
        /// </summary>
        /// <param name="callback">async callback</param>
        /// <param name="asyncState">async state</param>
        /// <returns>async result</returns>
        [OperationContract(AsyncPattern = true, Action = "http://hpc.microsoft.com/IFileStaging/DeleteFile", ReplyAction = "http://hpc.microsoft.com/IFileStaging/DeleteFileResponse")]
        [FaultContract(typeof(InternalFaultDetail))]
        IAsyncResult BeginDeleteFile(AsyncCallback callback, object asyncState);

        /// <summary>
        /// End method of DeleteFile
        /// </summary>
        /// <param name="result">async result</param>
        void EndDeleteFile(IAsyncResult result);

        /// <summary>
        /// Begin method of GetDirectories
        /// </summary>
        /// <param name="searchPattern">search pattern</param>
        /// <param name="searchOption">search option</param>
        /// <param name="callback">async callback</param>
        /// <param name="asyncState">async state</param>
        /// <returns>async result</returns>
        [OperationContract(AsyncPattern = true, Action = "http://hpc.microsoft.com/IFileStaging/GetDirectories", ReplyAction = "http://hpc.microsoft.com/IFileStaging/GetDirectoriesResponse")]
        [FaultContract(typeof(InternalFaultDetail))]
        IAsyncResult BeginGetDirectories(string searchPattern, SearchOption searchOption, AsyncCallback callback, object asyncState);

        /// <summary>
        /// End method of GetDirectories
        /// </summary>
        /// <param name="result">async result</param>
        /// <returns>an array of directories that match specified search pattern and search option under target directory</returns>
        ClusterDirectoryInfo[] EndGetDirectories(IAsyncResult result);

        /// <summary>
        /// Begin method if DeleteDirectory
        /// </summary>
        /// <param name="recursive">if delete the directory recursively. if set to false, only empty directory can be deleted</param>
        /// <param name="callback">async callback</param>
        /// <param name="asyncState">async state</param>
        /// <returns>async result</returns>
        [OperationContract(AsyncPattern = true, Action = "http://hpc.microsoft.com/IFileStaging/DeleteDirectory", ReplyAction = "http://hpc.microsoft.com/IFileStaging/DeleteDirectoryResponse")]
        [FaultContract(typeof(InternalFaultDetail))]
        IAsyncResult BeginDeleteDirectory(bool recursive, AsyncCallback callback, object asyncState);

        /// <summary>
        /// End method of DeleteDirectory
        /// </summary>
        /// <param name="result">async result</param>
        void EndDeleteDirectory(IAsyncResult result);

        /// <summary>
        /// Begin method of GetFiles
        /// </summary>
        /// <param name="searchPattern">search pattern</param>
        /// <param name="searchOption">search option</param>
        /// <param name="callback">async callback</param>
        /// <param name="asyncState">async state</param>
        /// <returns>async result</returns>
        [OperationContract(AsyncPattern = true, Action = "http://hpc.microsoft.com/IFileStaging/GetFiles", ReplyAction = "http://hpc.microsoft.com/IFileStaging/GetFilesResponse")]
        [FaultContract(typeof(InternalFaultDetail))]
        IAsyncResult BeginGetFiles(string searchPattern, SearchOption searchOption, AsyncCallback callback, object asyncState);

        /// <summary>
        /// End method of GetFiles
        /// </summary>
        /// <param name="result">async result</param>
        /// <returns>an array of files that match specified search pattern and search option under target directory</returns>
        ClusterFileInfo[] EndGetFiles(IAsyncResult result);

        /// <summary>
        /// Begin method of CopyFileToBlob
        /// </summary>
        /// <param name="blobUrl">destination blob url</param>
        /// <param name="sas">SAS for accessing the blob</param>
        /// <param name="sourceFilePath">source file path</param>
        /// <param name="callback">async callback</param>
        /// <param name="asyncState">async state</param>
        /// <returns>async result</returns>
        [OperationContract(AsyncPattern = true, Action = "http://hpc.microsoft.com/IFileStaging/CopyFileToBlob", ReplyAction = "http://hpc.microsoft.com/IFileStaging/CopyFileToBlobResponse")]
        [FaultContract(typeof(InternalFaultDetail))]
        IAsyncResult BeginCopyFileToBlob(string blobUrl, string sas, string sourceFilePath, AsyncCallback callback, object asyncState);

        /// <summary>
        /// End method of CopyFileToBlob
        /// </summary>
        /// <param name="result">async result</param>
        void EndCopyFileToBlob(IAsyncResult result);

        /// <summary>
        /// Begin method of CopyFileFromBlob
        /// </summary>
        /// <param name="blobUrl">source blob url</param>
        /// <param name="sas">SAS for accessing the blob</param>
        /// <param name="destFilePath">destination file path</param>
        /// <param name="overwrite">if overwrite existing files</param>
        /// <param name="callback">async callback</param>
        /// <param name="asyncState">async state</param>
        /// <returns>async result</returns>
        [OperationContract(AsyncPattern = true, Action = "http://hpc.microsoft.com/IFileStaging/CopyFileFromBlob", ReplyAction = "http://hpc.microsoft.com/IFileStaging/CopyFileFromBlobResponse")]
        [FaultContract(typeof(InternalFaultDetail))]
        IAsyncResult BeginCopyFileFromBlob(string blobUrl, string sas, string destFilePath, bool overwrite, AsyncCallback callback, object asyncState);

        /// <summary>
        /// End method of CopyFileFromBlob
        /// </summary>
        /// <param name="result">async result</param>
        void EndCopyFileFromBlob(IAsyncResult result);

        /// <summary>
        /// Begin method of CopyDirectoryToBlob
        /// </summary>
        /// <param name="blobUrlPrefix">blob url prefix</param>
        /// <param name="sas">SAS for accessing the blob</param>
        /// <param name="sourceDir">source directory path</param>
        /// <param name="filePatterns">file patterns</param>
        /// <param name="recursive">if copy directories to blob recursively</param>
        /// <param name="overwrite">if overwrite existing blobs</param>
        /// <param name="callback">async callback</param>
        /// <param name="asyncState">async state</param>
        /// <returns>async result</returns>
        [OperationContract(AsyncPattern = true, Action = "http://hpc.microsoft.com/IFileStaging/CopyDirectoryToBlob", ReplyAction = "http://hpc.microsoft.com/IFileStaging/CopyDirectoryToBlobResponse")]
        [FaultContract(typeof(InternalFaultDetail))]
        IAsyncResult BeginCopyDirectoryToBlob(string blobUrlPrefix, string sas, string sourceDir, List<string> filePatterns, bool recursive, bool overwrite, AsyncCallback callback, object asyncState);

        /// <summary>
        /// End method of CopyDirectoryToBlob
        /// </summary>
        /// <param name="result">async result</param>
        void EndCopyDirectoryToBlob(IAsyncResult result);

        /// <summary>
        /// Begin method of CopyDirectoryFromBlob
        /// </summary>
        /// <param name="blobUrlPrefix">blob url prefix</param>
        /// <param name="sas">SAS for accessing the blob</param>
        /// <param name="destDir">destination directory path</param>
        /// <param name="filePatterns">file patterns</param>
        /// <param name="recursive">if copy blobs recursively</param>
        /// <param name="overwrite">if overwrite existing files</param>
        /// <param name="callback">async callback</param>
        /// <param name="asyncState">async state</param>
        /// <returns>async result</returns>
        [OperationContract(AsyncPattern = true, Action = "http://hpc.microsoft.com/IFileStaging/CopyDirectoryFromBlob", ReplyAction = "http://hpc.microsoft.com/IFileStaging/CopyDirectoryFromBlobResponse")]
        [FaultContract(typeof(InternalFaultDetail))]
        IAsyncResult BeginCopyDirectoryFromBlob(string blobUrlPrefix, string sas, string destDir, List<string> filePatterns, bool recursive, bool overwrite, AsyncCallback callback, object asyncState);

        /// <summary>
        /// End method of CopyDirectoryFromBlob
        /// </summary>
        /// <param name="result">async result</param>
        void EndCopyDirectoryFromBlob(IAsyncResult result);

        /// <summary>
        /// Begin method of GetContainerUrl
        /// </summary>
        /// <param name="callback">async callback</param>
        /// <param name="asyncState">async state</param>
        /// <returns>async result</returns>
        [OperationContract(AsyncPattern = true, Action = "http://hpc.microsoft.com/IFileStaging/GetContainerUrl", ReplyAction = "http://hpc.microsoft.com/IFileStaging/GetContainerUrlResponse")]
        [FaultContract(typeof(InternalFaultDetail))]
        IAsyncResult BeginGetContainerUrl(AsyncCallback callback, object asyncState);

        /// <summary>
        /// End method of GetContainerUrl
        /// </summary>
        /// <param name="sas">Azure SAS for accessing user's container</param>
        /// <param name="result">async result</param>
        /// <returns>an URL pointing to user's container on the intermediate blob storage</returns>
        string EndGetContainerUrl(out string sas, IAsyncResult result);

        /// <summary>
        /// Begin method of GenerateSASForBlob
        /// </summary>
        /// <param name="blobName">target blob name</param>
        /// <param name="permissions">permissions to be granted by the SAS</param>
        /// <param name="callback">async callback</param>
        /// <param name="asyncState">async state</param>
        /// <returns>async result</returns>
        [OperationContract(AsyncPattern = true, Action = "http://hpc.microsoft.com/IFileStaging/GenerateSASForBlob", ReplyAction = "http://hpc.microsoft.com/IFileStaging/GenerateSASForBlobResponse")]
        [FaultContract(typeof(InternalFaultDetail))]
        IAsyncResult BeginGenerateSASForBlob(string blobName, SharedAccessBlobPermissions permissions, AsyncCallback callback, object asyncState);

        /// <summary>
        /// End method of GenerateSASForBlob
        /// </summary>
        /// <param name="result">async result</param>
        /// <returns>an Azure SAS that grants specified permissions to the target blob</returns>
        string EndGenerateSASForBlob(IAsyncResult result);
    }
}
