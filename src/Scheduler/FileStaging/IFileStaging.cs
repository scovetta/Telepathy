//--------------------------------------------------------------------------
// <copyright file="IFileStaging.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     This interface defines the service contract for the File Staging
//     service. Its operations are implemented by the workers.
// </summary>
//--------------------------------------------------------------------------

namespace Microsoft.Hpc.Azure.FileStaging.Client
{
    using System.Collections.Generic;
    using System.IO;
    using System.ServiceModel;

    /// <summary>
    /// Service contract for file staging worker service.
    /// </summary>
    [ServiceContract(Namespace = "http://hpc.microsoft.com/filestaging/")]
    public interface IFileStaging
    {
        [OperationContract(Action = "http://hpc.microsoft.com/IFileStaging/ReadFile", ReplyAction = "http://hpc.microsoft.com/IFileStaging/ReadFileResponse")]
        [FaultContract(typeof(InternalFaultDetail))]
        Stream ReadFile();

        [OperationContract(Action = "http://hpc.microsoft.com/IFileStaging/WriteFile", ReplyAction = "http://hpc.microsoft.com/IFileStaging/WriteFileResponse")]
        [FaultContract(typeof(InternalFaultDetail))]
        void WriteFile(Stream contents);

        [OperationContract(Action = "http://hpc.microsoft.com/IFileStaging/DeleteFile", ReplyAction = "http://hpc.microsoft.com/IFileStaging/DeleteFileResponse")]
        [FaultContract(typeof(InternalFaultDetail))]
        void DeleteFile();

        [OperationContract(Action = "http://hpc.microsoft.com/IFileStaging/GetDirectories", ReplyAction = "http://hpc.microsoft.com/IFileStaging/GetDirectoriesResponse")]
        [FaultContract(typeof(InternalFaultDetail))]
        ClusterDirectoryInfo[] GetDirectories(string searchPattern, System.IO.SearchOption searchOption);

        [OperationContract(Action = "http://hpc.microsoft.com/IFileStaging/DeleteDirectory", ReplyAction = "http://hpc.microsoft.com/IFileStaging/DeleteDirectoryResponse")]
        [FaultContract(typeof(InternalFaultDetail))]
        void DeleteDirectory(bool recursive);

        [OperationContract(Action = "http://hpc.microsoft.com/IFileStaging/GetFiles", ReplyAction = "http://hpc.microsoft.com/IFileStaging/GetFilesResponse")]
        [FaultContract(typeof(InternalFaultDetail))]
        ClusterFileInfo[] GetFiles(string searchPattern, System.IO.SearchOption searchOption);
        
        [OperationContract(Action = "http://hpc.microsoft.com/IFileStaging/CopyFileToBlob", ReplyAction = "http://hpc.microsoft.com/IFileStaging/CopyFileToBlobResponse")]
        [FaultContract(typeof(InternalFaultDetail))]
        void CopyFileToBlob(string blobUrl, string sas, string sourceFilePath);

        [OperationContract(Action = "http://hpc.microsoft.com/IFileStaging/CopyFileFromBlob", ReplyAction = "http://hpc.microsoft.com/IFileStaging/CopyFileFromBlobResponse")]
        [FaultContract(typeof(InternalFaultDetail))]
        void CopyFileFromBlob(string blobUrl, string sas, string destFilePath, bool overwrite);
                
        [OperationContract(Action = "http://hpc.microsoft.com/IFileStaging/CopyDirectoryToBlob", ReplyAction = "http://hpc.microsoft.com/IFileStaging/CopyDirectoryToBlobResponse")]
        [FaultContract(typeof(InternalFaultDetail))]
        void CopyDirectoryToBlob(string blobUrlPrefix, string sas, string sourceDir, List<string> filePatterns, bool recursive, bool overwrite);

        [OperationContract(Action = "http://hpc.microsoft.com/IFileStaging/CopyDirectoryFromBlob", ReplyAction = "http://hpc.microsoft.com/IFileStaging/CopyDirectoryFromBlobResponse")]
        [FaultContract(typeof(InternalFaultDetail))]
        void CopyDirectoryFromBlob(string blobUrlPrefix, string sas, string destDir, List<string> filePatterns, bool recursive, bool overwrite);
    }
}
