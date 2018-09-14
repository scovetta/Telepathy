//--------------------------------------------------------------------------
// <copyright file="ClusterFile.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     This makes up the File part of the public API. It is designed to
//     imitate the System.IO.File class.
// </summary>
//--------------------------------------------------------------------------

namespace Microsoft.Hpc.Azure.FileStaging.Client
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using Microsoft.Hpc.Azure.FileStaging;
    using Microsoft.WindowsAzure.Storage;

    /// <summary>
    /// This class provides methods to read, write, and delete files.
    /// </summary>
    public class ClusterFile
    {
        /// <summary>
        /// Blob url format: ContainerUrl/UniqueOperationId.FileName
        /// </summary>
        private const string BlobUrlFormat = @"{0}/{1}.{2}";

        /// <summary>
        /// Defines amount of concurrent file transfers to use
        /// </summary>
        private const int ConcurrentFileTransferCount = 4;

        /// <summary>
        /// Initializes static members of the ClusterFile class
        /// </summary>
        static ClusterFile()
        {
            if (FileStagingCommon.SchedulerOnAzure)
            {
                // Do nothing but just making sure static constructor of FileStagingCommon
                // class is called before any member of ClusterFile is accessed so that
                // assembly resolvers are installed for related assemblies
                // (Microsoft.WindowsAzure.StorageClient.dll, Microsoft.Hpc.Azure.DataMovement.dll)
            }
        }

        /// <summary>
        /// Returns a stream that can be used to read the contents of a file, beginning at 
        /// the specified position. The FileMode option is used to control whether or not the 
        /// file (and containing directory) are implicitly created if they do not exist.
        /// </summary>
        /// <param name="connectionString">head node name</param>
        /// <param name="targetNode">target node name</param>
        /// <param name="path">file path on target node</param>
        /// <param name="position">position from which to begin read</param>
        /// <returns>a stream that can be used to read the contents of the file</returns>
        public static Stream Read(string connectionString, string targetNode, string path, long position)
        {
            FileStagingClientWithHeaders client = null;
            bool errorFlag = true;
            try
            {
                client = FileStagingClientWithHeaders.CreateInstance(connectionString, targetNode, path);
                client.AddFileTransferHeaders(FileMode.Open, position, false, false, string.Empty);
                ClusterStream stream = new ClusterStream(client.Client.ReadFile(), client);
                errorFlag = false;
                return stream;
            }
            catch (FaultException<InternalFaultDetail> ex)
            {
                throw new FileStagingException(ex.Detail);
            }
            catch (FaultException ex)
            {
                throw new FileStagingException(ex);
            }
            catch (CommunicationException ex)
            {
                throw new FileStagingException(ex, FileStagingErrorCode.CommunicationFailure);
            }
            catch (Exception ex)
            {
                throw new FileStagingException(ex);
            }
            finally
            {
                if (errorFlag)
                {
                    client.Dispose();
                }
            }
        }

        /// <summary>
        /// Returns a stream that can be used to read the contents of the last "nLines" of a
        /// file.
        /// </summary>
        /// <param name="connectionString">head node name</param>
        /// <param name="targetNode">target node name</param>
        /// <param name="path">file path on the target node</param>
        /// <param name="nLines">number of lines to read</param>
        /// <param name="encoding">encoding used to read source file</param>
        /// <returns>a stream that can be used to read the last "nLines" of the file</returns>
        public static Stream Tail(string connectionString, string targetNode, string path, long nLines, string encoding)
        {
            FileStagingClientWithHeaders client = null;
            bool errorFlag = true;
            try
            {
                client = FileStagingClientWithHeaders.CreateInstance(connectionString, targetNode, path);
                client.AddFileTransferHeaders(FileMode.Open, nLines, true, true, encoding);
                ClusterStream stream = new ClusterStream(client.Client.ReadFile(), client);
                errorFlag = false;
                return stream;
            }
            catch (FaultException<InternalFaultDetail> ex)
            {
                throw new FileStagingException(ex.Detail);
            }
            catch (FaultException ex)
            {
                throw new FileStagingException(ex);
            }
            catch (CommunicationException ex)
            {
                throw new FileStagingException(ex, FileStagingErrorCode.CommunicationFailure);
            }
            catch (Exception ex)
            {
                throw new FileStagingException(ex);
            }
            finally
            {
                if (errorFlag)
                {
                    client.Dispose();
                }
            }
        }

        /// <summary>
        /// Writes a file using the specified stream, beginning at the specified position. 
        /// The FileMode option is used to control whether or not the file (and containing 
        /// directory) are implicitly created if they do not exist. If the stream becomes 
        /// unavailable in the middle of the write, the file will be left incomplete, but 
        /// the handle will be closed.
        /// </summary>
        /// <param name="connectionString">head node name</param>
        /// <param name="targetNode">target node name</param>
        /// <param name="path">file path on target node</param>
        /// <param name="mode">whether or not create the file if it doesn't exist</param>
        /// <param name="position">position from which the write operation begins</param>
        /// <param name="contents">contents to be written</param>
        public static void Write(string connectionString, string targetNode, string path, FileMode mode, long position, Stream contents)
        {
            try
            {
                using (FileStagingClientWithHeaders client = FileStagingClientWithHeaders.CreateInstance(connectionString, targetNode, path))
                {
                    client.AddFileTransferHeaders(mode, position, false, false, string.Empty);
                    client.Client.WriteFile(contents);
                }
            }
            catch (FaultException<InternalFaultDetail> ex)
            {
                throw new FileStagingException(ex.Detail);
            }
            catch (FaultException ex)
            {
                throw new FileStagingException(ex);
            }
            catch (CommunicationException ex)
            {
                throw new FileStagingException(ex, FileStagingErrorCode.CommunicationFailure);
            }
            catch (Exception ex)
            {
                throw new FileStagingException(ex);
            }
        }

        /// <summary>
        /// Delete the specified file
        /// </summary>
        /// <param name="connectionString">head node name</param>
        /// <param name="targetNode">target node name</param>
        /// <param name="path">file path on target node</param>
        public static void Delete(string connectionString, string targetNode, string path)
        {
            try
            {
                using (FileStagingClientWithHeaders client = FileStagingClientWithHeaders.CreateInstance(connectionString, targetNode, path))
                {
                    client.Client.DeleteFile();
                }
            }
            catch (FaultException<InternalFaultDetail> ex)
            {
                throw new FileStagingException(ex.Detail);
            }
            catch (FaultException ex)
            {
                throw new FileStagingException(ex);
            }
            catch (CommunicationException ex)
            {
                throw new FileStagingException(ex, FileStagingErrorCode.CommunicationFailure);
            }
            catch (Exception ex)
            {
                throw new FileStagingException(ex);
            }
        }

        /// <summary>
        /// Copy a file from a node in cluster to local. If the copy operation is
        /// interrupted in the middle, the local file will be left incomplete.
        /// </summary>
        /// <param name="headNode">head node name</param>
        /// <param name="targetNode">target node name</param>
        /// <param name="path">absolute file path on target node</param>
        /// <param name="localFilePath">local directory path where the file is put</param>
        /// <param name="overwrite">if overwrite is true and destPath exists,
        /// destPath will be overwritten; if overwrite is false and destPath
        /// exists, destPath will be skipped.</param>
        public static void CopyFromNode(
            string headNode, string targetNode, string path, string localFilePath, bool overwrite)
        {
            // if overwrite == false, make sure localFilePath doesn't exist
            FileMode mode = overwrite ? FileMode.OpenOrCreate : FileMode.CreateNew;
            try
            {
                using (FileStream fs = File.Open(localFilePath, mode))
                {
                }
            }
            catch (Exception ex)
            {
                if ((Marshal.GetHRForException(ex) & 0xFFFF) == FileStagingCommon.ErrorFileExists)
                {
                    throw new FileStagingException(ex, FileStagingErrorCode.TargetExists);
                }
                else
                {
                    throw new FileStagingException(ex, FileStagingErrorCode.TargetIOFailure);
                }
            }

            CopyFileFromNodeToLocal(
                headNode,
                localFilePath,
                targetNode,
                path,
                overwrite);
        }

        /// <summary>
        /// Copy a file from local to a node in cluster. If the copy operation is
        /// interrupted in the middle, the file on node in cluster will be left incomplete.
        /// </summary>
        /// <param name="headNode">head node name</param>
        /// <param name="targetNode">target node name</param>
        /// <param name="path">absolute file path on target nodet</param>
        /// <param name="localFilePath">local file path</param>
        /// <param name="overwrite">if overwrite is true, existing destination file
        /// will be overwritten. if overwrite is true, existing destination file will
        /// be skipped. </param>
        public static void CopyToNode(
            string headNode, string targetNode, string path, string localFilePath, bool overwrite)
        {   
            CopyFileFromLocalToNode(
                headNode,
                localFilePath,
                targetNode,
                path,
                overwrite);
        }

        /// <summary>
        /// Copy a file from local to a cluster node
        /// </summary>
        /// <param name="connectionString">head node name</param>
        /// <param name="localFilePath">source local file path</param>
        /// <param name="targetNode">target node name</param>
        /// <param name="remoteFilePath">absolute destination file path on target node</param>
        /// <param name="overwrite">if overwrite existing files</param>
        private static void CopyFileFromLocalToNode(string connectionString, string localFilePath, string targetNode, string remoteFilePath, bool overwrite)
        {
            // steps:
            // 1. call GetContainerUrl, and obtain data url prefix on blob
            // 2. call DataMovement library to move data from local to blob
            // 3. call CopyFileFromBlob to move data from blob to targetNode
            // 4. delete intermediate blob
            string blobUrl = string.Empty;
            string sas = string.Empty;
            try
            {
                using (FileStagingClientWithHeaders client = FileStagingClientWithHeaders.CreateInstance(connectionString, targetNode, remoteFilePath))
                {
                    string containerUrl = client.Client.GetContainerUrl(out sas);
                    string uniqueOperationId = Guid.NewGuid().ToString();
                    blobUrl = FileStagingCommon.GetBlobUrlPrefix(containerUrl, uniqueOperationId) +
                              Path.GetFileName(localFilePath);
                }

                // move data from local to blob
                FileStagingCommon.UploadFileToBlob(blobUrl, sas, localFilePath);

                // move data from blob to target node
                using (FileStagingClientWithHeaders client = FileStagingClientWithHeaders.CreateInstance(connectionString, targetNode, remoteFilePath))
                {
                    client.Client.CopyFileFromBlob(blobUrl, sas, remoteFilePath, overwrite);
                }
            }
            catch (FaultException<InternalFaultDetail> ex)
            {
                throw new FileStagingException(ex.Detail);
            }
            catch (FaultException ex)
            {
                throw new FileStagingException(ex);
            }
            catch (CommunicationException ex)
            {
                throw new FileStagingException(ex, FileStagingErrorCode.CommunicationFailure);
            }
            catch (StorageException ex)
            {
                throw FileStagingClientUtility.GenerateFileStagingException(ex);
            }
            catch (Exception ex)
            {
                throw new FileStagingException(ex);
            }
            finally
            {
                try
                {
                    if (!string.IsNullOrEmpty(blobUrl))
                    {
                        // delete intermediate blob
                        FileStagingCommon.DeleteBlob(blobUrl, sas);
                    }
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Copy a file from a cluster node to local
        /// </summary>
        /// <param name="connectionString">head node name</param>
        /// <param name="localFilePath">local file path</param>
        /// <param name="targetNode">target node name</param>
        /// <param name="remoteFilePath">remote file path</param>
        /// <param name="overwrite">if overwrite existing files</param>
        private static void CopyFileFromNodeToLocal(string connectionString, string localFilePath, string targetNode, string remoteFilePath, bool overwrite)
        {
            // steps:
            // 1. call GetContainerUrl, and obtain data url prefix on blob
            // 2. call CopyFileToBlob to move data from targetNode to blob
            // 3. call DataMovement library to move data from blob to local
            // 4. delete intermediate blob
            string blobUrl = string.Empty;
            string sas = string.Empty;
            try
            {
                using (FileStagingClientWithHeaders client = FileStagingClientWithHeaders.CreateInstance(connectionString, targetNode, remoteFilePath))
                {
                    string containerUrl = client.Client.GetContainerUrl(out sas);
                    string uniqueOperationId = Guid.NewGuid().ToString();
                    blobUrl = FileStagingCommon.GetBlobUrlPrefix(containerUrl, uniqueOperationId) + Path.GetFileName(localFilePath);

                    // move data from target node to blob
                    client.Client.CopyFileToBlob(blobUrl, sas, remoteFilePath);
                }

                // move data from blob to local
                FileStagingCommon.DownloadFileFromBlob(blobUrl, sas, localFilePath);
            }
            catch (FaultException<InternalFaultDetail> ex)
            {
                throw new FileStagingException(ex.Detail);
            }
            catch (FaultException ex)
            {
                throw new FileStagingException(ex);
            }
            catch (CommunicationException ex)
            {
                throw new FileStagingException(ex, FileStagingErrorCode.CommunicationFailure);
            }
            catch (StorageException ex)
            {
                throw FileStagingClientUtility.GenerateFileStagingException(ex);
            }
            catch (Exception ex)
            {
                throw new FileStagingException(ex);
            }
            finally
            {
                try
                {
                    if (!string.IsNullOrEmpty(blobUrl))
                    {
                        // delete intermediate blob
                        FileStagingCommon.DeleteBlob(blobUrl, sas);
                    }
                }
                catch
                {
                }
            }
        }
    }
}
