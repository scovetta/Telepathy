//--------------------------------------------------------------------------
// <copyright file="ClusterDirectory.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     This makes up the Directory part of the public API. It is designed
//     to imitate the System.IO.Directory class.
// </summary>
//--------------------------------------------------------------------------

namespace Microsoft.Hpc.Azure.FileStaging.Client
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.ServiceModel;
    using Microsoft.Hpc.Azure.FileStaging;
    using Microsoft.WindowsAzure.Storage;

    /// <summary>
    /// This class provides methods to list directories, remove directories, and 
    /// list directory contents.
    /// </summary>
    public class ClusterDirectory
    {
        /// <summary>
        /// Initializes static members of the ClusterDirectory class
        /// </summary>
        static ClusterDirectory()
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
        /// Returns an array of each of the directories found under the specified 
        /// path with the given search pattern and options. This method mimics the
        /// GetDirectories method System.IO.Directory.GetDirectories().
        /// </summary>
        /// <param name="connectionString">head node name</param>
        /// <param name="targetNode">target node name</param>
        /// <param name="path">absolute path on target node</param>
        /// <param name="searchPattern">search pattern</param>
        /// <param name="searchOption">search option</param>
        /// <returns>an array of directories found under the specified path with
        /// the given search pattern and options</returns>
        public static ClusterDirectoryInfo[] GetDirectories(string connectionString, string targetNode, string path, string searchPattern, SearchOption searchOption)
        {
            try
            {
                using (FileStagingClientWithHeaders client = FileStagingClientWithHeaders.CreateInstance(connectionString, targetNode, path))
                {
                    return client.Client.GetDirectories(searchPattern, searchOption);
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
        /// Delete the directory at the specified path. If "recursive" is set 
        /// to false, only an empty directory can be deleted.
        /// </summary>
        /// <param name="connectionString">head node name</param>
        /// <param name="targetNode">target node name</param>
        /// <param name="path">directory path on target node</param>
        /// <param name="recursive">if delete files or sub-directories under
        /// the specified directory recursively.  If set to false, only an
        /// empty directory can be deleted</param>
        public static void Delete(string connectionString, string targetNode, string path, bool recursive)
        {
            try
            {
                using (FileStagingClientWithHeaders client = FileStagingClientWithHeaders.CreateInstance(connectionString, targetNode, path))
                {
                    client.Client.DeleteDirectory(recursive);
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
        /// Returns an array of each of the files found under the specified path 
        /// with the given search pattern and options. This method mimics the
        /// GetFiles method System.IO.Directory.GetFiles().
        /// </summary>
        /// <param name="connectionString">head node name</param>
        /// <param name="targetNode">target node name</param>
        /// <param name="path">absolute path on target node</param>
        /// <param name="searchPattern">search pattern</param>
        /// <param name="searchOption">search option</param>
        /// <returns>an array of files found under the specified path with the
        /// given search pattern and options</returns>
        public static ClusterFileInfo[] GetFiles(string connectionString, string targetNode, string path, string searchPattern, SearchOption searchOption)
        {
            try
            {
                using (FileStagingClientWithHeaders client = FileStagingClientWithHeaders.CreateInstance(connectionString, targetNode, path))
                {
                    return client.Client.GetFiles(searchPattern, searchOption);
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
        /// Copy a directory from a node in cluster to local. If the copy operation is
        /// interrupted in the middle, the local files will be left incomplete.
        /// </summary>
        /// <param name="connectionString">head node name</param>
        /// <param name="targetNode">target node name</param>
        /// <param name="path">absolute directory path on target node</param>
        /// <param name="localDirPath">local directory path</param>
        /// <param name="filePatterns">files to be copies</param>
        /// <param name="recursive">if copy directories recursivly</param>
        /// <param name="overwrite">if overwrite is true, existing local files will be overwritten.</param>
        public static void CopyFromNode(
            string connectionString, string targetNode, string path, string localDirPath, List<string> filePatterns, bool recursive, bool overwrite)
        {
            // steps:
            // 1. call GetContainerUrl, and obtain data url prefix on blob
            // 2. call CopyDirectoryToBlob to move data from targetNode to blob
            // 3. call DataMovement library to move data from blob to local
            // 4. delete intermediate blob
            string containerUrl = string.Empty;
            string containerSas = string.Empty;
            string blobUrlPrefix = string.Empty;
            try
            {
                using (FileStagingClientWithHeaders client = FileStagingClientWithHeaders.CreateInstance(connectionString, targetNode, path))
                {
                    containerUrl = client.Client.GetContainerUrl(out containerSas);
                    string uniqueOperationId = Guid.NewGuid().ToString();
                    blobUrlPrefix = FileStagingCommon.GetBlobUrlPrefix(containerUrl, uniqueOperationId);

                    // move data from target node to blob
                    client.Client.CopyDirectoryToBlob(blobUrlPrefix, containerSas, path, filePatterns, recursive, overwrite);
                }

                // move data from blob to local
                FileStagingCommon.DownloadDirectoryFromBlob(blobUrlPrefix, containerSas, localDirPath, filePatterns, recursive, overwrite, null, null, null);
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
                    if (!string.IsNullOrEmpty(blobUrlPrefix))
                    {                        
                        // delete intermediate blobs
                        FileStagingCommon.DeleteBlobs(containerUrl, blobUrlPrefix, containerSas);
                    }
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Copy a directory from local to a node in cluster. If the copy operation is
        /// interrupted in the middle, the file on node in cluster will be left incomplete.
        /// </summary>
        /// <param name="connectionString">head node name</param>
        /// <param name="targetNode">target node name</param>
        /// <param name="path">absolute directory path on target node</param>
        /// <param name="localDirPath">local directory path</param>
        /// <param name="filePatterns">files to be copies</param>
        /// <param name="recursive">if copy directories recursivly</param>
        /// <param name="overwrite">if overwrite is true, existing destination files will be overwritten.</param>
        public static void CopyToNode(
            string connectionString, string targetNode, string path, string localDirPath, List<string> filePatterns, bool recursive, bool overwrite)
        {
            // steps:
            // 1. call GetContainerUrl, and obtain data url prefix on blob
            // 2. call DataMovement library to move data from local to blob
            // 3. call CopyDirectoryFromBlob to move data from blob to targetNode
            // 4. delete intermediate blob
            string containerUrl = string.Empty;
            string blobUrlPrefix = string.Empty;
            string containerSas = string.Empty;
            try
            {
                using (FileStagingClientWithHeaders client = FileStagingClientWithHeaders.CreateInstance(connectionString, targetNode, path))
                {
                    containerUrl = client.Client.GetContainerUrl(out containerSas);
                    string uniqueOperationId = Guid.NewGuid().ToString();
                    blobUrlPrefix = FileStagingCommon.GetBlobUrlPrefix(containerUrl, uniqueOperationId);
                }

                // move data from local to blob
                FileStagingCommon.UploadDirectoryToBlob(blobUrlPrefix, containerSas, localDirPath, filePatterns, recursive, overwrite, null, null, null);

                // move data from blob to target node
                using (FileStagingClientWithHeaders client = FileStagingClientWithHeaders.CreateInstance(connectionString, targetNode, path))
                {
                    client.Client.CopyDirectoryFromBlob(blobUrlPrefix, containerSas, path, filePatterns, recursive, overwrite);
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
                    if (!string.IsNullOrEmpty(blobUrlPrefix))
                    {
                        // delete intermediate blobs
                        FileStagingCommon.DeleteBlobs(containerUrl, blobUrlPrefix, containerSas);
                    }
                }
                catch
                {
                }
            }
        }
    }
}
