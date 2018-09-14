//--------------------------------------------------------------------------
// <copyright file="IFileStagingRouter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     This interface defines the service contract for the File Staging
//     service. Its operations are implemented by the proxies.
// </summary>
//--------------------------------------------------------------------------

namespace Microsoft.Hpc.Azure.FileStaging
{
    using System;
    using System.ServiceModel;
    using Microsoft.Hpc.Management.FileTransfer;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage;

    /// <summary>
    /// Service contract for the file staging proxy service
    /// </summary>
    [ServiceContract(Namespace = "http://hpc.microsoft.com/filestaging/")]
    public interface IFileStagingRouter
    {
        [OperationContract(Action = "http://hpc.microsoft.com/IFileStaging/ReadFile", ReplyAction = "http://hpc.microsoft.com/IFileStaging/ReadFileResponse")]
        System.ServiceModel.Channels.Message ProcessMessage1(System.ServiceModel.Channels.Message request);

        [OperationContract(Action = "http://hpc.microsoft.com/IFileStaging/WriteFile", ReplyAction = "http://hpc.microsoft.com/IFileStaging/WriteFileResponse")]
        System.ServiceModel.Channels.Message ProcessMessage2(System.ServiceModel.Channels.Message request);

        [OperationContract(Action = "http://hpc.microsoft.com/IFileStaging/DeleteFile", ReplyAction = "http://hpc.microsoft.com/IFileStaging/DeleteFileResponse")]
        System.ServiceModel.Channels.Message ProcessMessage3(System.ServiceModel.Channels.Message request);

        [OperationContract(Action = "http://hpc.microsoft.com/IFileStaging/GetDirectories", ReplyAction = "http://hpc.microsoft.com/IFileStaging/GetDirectoriesResponse")]
        System.ServiceModel.Channels.Message ProcessMessage4(System.ServiceModel.Channels.Message request);

        [OperationContract(Action = "http://hpc.microsoft.com/IFileStaging/DeleteDirectory", ReplyAction = "http://hpc.microsoft.com/IFileStaging/DeleteDirectoryResponse")]
        System.ServiceModel.Channels.Message ProcessMessage5(System.ServiceModel.Channels.Message request);

        [OperationContract(Action = "http://hpc.microsoft.com/IFileStaging/GetFiles", ReplyAction = "http://hpc.microsoft.com/IFileStaging/GetFilesResponse")]
        System.ServiceModel.Channels.Message ProcessMessage6(System.ServiceModel.Channels.Message request);

        [OperationContract(Action = "http://hpc.microsoft.com/IFileStaging/CopyFileToBlob", ReplyAction = "http://hpc.microsoft.com/IFileStaging/CopyFileToBlobResponse")]
        System.ServiceModel.Channels.Message CopyFileToBlob(System.ServiceModel.Channels.Message request);

        [OperationContract(Action = "http://hpc.microsoft.com/IFileStaging/CopyFileFromBlob", ReplyAction = "http://hpc.microsoft.com/IFileStaging/CopyFileFromBlobResponse")]
        System.ServiceModel.Channels.Message CopyFileFromBlob(System.ServiceModel.Channels.Message request);

        [OperationContract(Action = "http://hpc.microsoft.com/IFileStaging/CopyDirectoryToBlob", ReplyAction = "http://hpc.microsoft.com/IFileStaging/CopyDirectoryToBlobResponse")]
        System.ServiceModel.Channels.Message CopyDirectoryToBlob(System.ServiceModel.Channels.Message request);

        [OperationContract(Action = "http://hpc.microsoft.com/IFileStaging/CopyDirectoryFromBlob", ReplyAction = "http://hpc.microsoft.com/IFileStaging/CopyDirectoryFromBlobResponse")]
        System.ServiceModel.Channels.Message CopyDirectoryFromBlob(System.ServiceModel.Channels.Message request);

        [OperationContract(Action = "http://hpc.microsoft.com/IFileStaging/GetContainerUrl", ReplyAction = "http://hpc.microsoft.com/IFileStaging/GetContainerUrlResponse")]
        [FaultContract(typeof(InternalFaultDetail))]
        string GetContainerUrl(out string sas);

        [OperationContract(Action = "http://hpc.microsoft.com/IFileStaging/GenerateSASForBlob", ReplyAction = "http://hpc.microsoft.com/IFileStaging/GenerateSASForBlobResponse")]
        [FaultContract(typeof(InternalFaultDetail))]
        string GenerateSASForBlob(string blobName, SharedAccessBlobPermissions permissions);

        [OperationContract(Action = "http://hpc.microsoft.com/IFileStagingRouter/KeepAlive", ReplyAction = "http://hpc.microsoft.com/IFileStagingRouter/KeepAliveResponse")]
        void KeepAlive();

        [OperationContract(Action = "http://hpc.microsoft.com/IFileStagingRouter/GetAzureLocalLogFileList", ReplyAction = "http://hpc.microsoft.com/IFileStagingRouter/GetAzureLocalLogFileListResponse")]
        [FaultContract(typeof(FileTransferFaultDetail))]
        HpcFileInfo[] GetAzureLocalLogFileList(string instanceName, DateTime startTime, DateTime endTime);

        [OperationContract(Action = "http://hpc.microsoft.com/IFileStagingRouter/GetAzureLocalLogFile", ReplyAction = "http://hpc.microsoft.com/IFileStagingRouter/GetAzureLocalLogFileResponse")]
        [FaultContract(typeof(FileTransferFaultDetail))]
        byte[] GetAzureLocalLogFile(string instanceName, string fileName);
    }
}
