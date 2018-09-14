//--------------------------------------------------------------------------
// <copyright file="FileStagingWorker.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     This is the generic definitition of the file staging worker. Its
//     implementation may run in Azure or on-premise. This is an abstract
//     class and it cannot be instantiated.
// </summary>
//--------------------------------------------------------------------------

namespace Microsoft.Hpc.Azure.FileStaging
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.AccessControl;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Hpc.Azure.FileStaging.Client;

    /// <summary>
    /// An abstract class that implements core functionality of IFileStaing.
    /// It's derived to SchedulerFileStagingProxy for on-premise and to
    /// AzureFileStagingProxy for Azure.
    /// </summary>
    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any, InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true)]
    public abstract class FileStagingWorker : IFileStaging
    {
        private readonly TimeSpan CloseTimeout = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Keep a channel open with the proxy, in case messages destined for another node are received by this one
        /// </summary>
        private GenericFileStagingClient proxyChannel = null;

        /// <summary>
        /// A lock is needed to prevent the channel from reopening concurrently 
        /// </summary>
        private object channelLock = new object();

        /// <summary>
        /// Specifies valid levels for tracing
        /// </summary>
        protected enum TraceLevel
        {
            /// <summary>
            /// Critical level
            /// </summary>
            Critical = 0,

            /// <summary>
            /// Error level
            /// </summary>
            Error = 1,

            /// <summary>
            /// Warning level
            /// </summary>
            Warning = 2,

            /// <summary>
            /// Information level
            /// </summary>
            Information = 3,

            /// <summary>
            /// Verbose level
            /// </summary>
            Verbose = 4,
        }

        /// <summary>
        /// Gets or sets the service host object needed to run the service
        /// </summary>
        protected ServiceHost WorkerServiceHost
        {
            get;
            set;
        }

        /// <summary>
        /// Gets target file or directory path
        /// </summary>
        protected string TargetPath
        {
            get
            {
                return Environment.ExpandEnvironmentVariables(GetInputFromHeader<string>(FileStagingCommon.WcfHeaderPath));
            }
        }

        /// <summary>
        /// Gets the callback to be called before copying a blob transfer from local to blob
        /// </summary>
        private FileStagingCommon.BeforeBlobTransferCallback BeforeCopyToBlob
        {
            get
            {
                // check that user has read permission to the source file before copying it to blob
                return
                  delegate(string userName, string sourcePath, string destinationPath)
                  {
                      try
                      {
                          this.CheckFilePermissions(userName, sourcePath, FileSystemRights.Read);
                          return true;
                      }
                      catch (Exception)
                      {
                          return false;
                      }
                  };
            }
        }

        /// <summary>
        /// Gets the callback to be called before queueing a blob transfer from blob to local
        /// </summary>
        private FileStagingCommon.BeforeBlobTransferCallback BeforeCopyFromBlob
        {
            get
            {
                // check that user has write permission to the destination file (if it exists)
                return null;
            }
        }

        /// <summary>
        /// Gets the callback to be called after queueing a blob transfer from blob to local
        /// </summary>
        private FileStagingCommon.AfterBlobTransferCallback AfterCopyFromBlob
        {
            get
            {
                // grant user full control permission to the destiniation file
                return null;
            }
        }

        /// <summary>
        /// Starts the service and listens for clients
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Stops the service
        /// </summary>
        public virtual void Stop()
        {
            this.Trace(TraceLevel.Information, "The file staging service is stopping.");

            Task.Factory.FromAsync<TimeSpan>(this.WorkerServiceHost.BeginClose, ar =>
            {
                try
                {
                    this.WorkerServiceHost.EndClose(ar);
                    this.Trace(TraceLevel.Information, "The file staging service is stopped.");
                }
                catch (TimeoutException ex)
                {
                    this.Trace(TraceLevel.Error, "Unable to close the WorkerServiceHost with in {0} seconds. ex:{1}", CloseTimeout.TotalSeconds, ex);
                }
                catch (Exception ex)
                {
                    this.Trace(TraceLevel.Error, "Closing the WorkerServiceHost got exception: {0}", ex);
                }
            }, CloseTimeout, null).Wait();
        }

        #region Implementation of the service contract

        /// <summary>
        /// Read contents of the target file
        /// </summary>
        /// <returns>a stream that can be used to read contents of the target file</returns>
        public virtual Stream ReadFile()
        {
            string logicalName = GetInputFromHeader<string>(FileStagingCommon.WcfHeaderTargetNode);
            this.Trace(TraceLevel.Verbose, "Received ReadFile message targeting node {0}.", logicalName);

            // Forward the message if this node is not the target
            if (!this.IsTargetedNode(logicalName))
            {
                using (OperationContextScope scope = new OperationContextScope(this.GetProxyChannel().InnerChannel))
                {
                    return this.ForwardMessage("ReadFile").GetBody<System.IO.Stream>();
                }
            }
            else
            {
                string path = this.TargetPath;
                FileMode mode = GetInputFromHeader<FileMode>(FileStagingCommon.WcfHeaderMode);
                long position = GetInputFromHeader<long>(FileStagingCommon.WcfHeaderPosition);

                bool backward = false;
                bool lines = false;
                string encoding = string.Empty;
                Encoding inputEncoding = null;

                MessageHeaders incomingHeaders = OperationContext.Current.IncomingMessageHeaders;
                int index = incomingHeaders.FindHeader(FileStagingCommon.WcfHeaderBackward, FileStagingCommon.WcfHeaderNamespace);
                if (index >= 0)
                {
                    backward = incomingHeaders.GetHeader<bool>(index);
                }

                index = incomingHeaders.FindHeader(FileStagingCommon.WcfHeaderLines, FileStagingCommon.WcfHeaderNamespace);
                if (index >= 0)
                {
                    lines = incomingHeaders.GetHeader<bool>(index);
                }

                index = incomingHeaders.FindHeader(FileStagingCommon.WcfHeaderEncoding, FileStagingCommon.WcfHeaderNamespace);
                if (index >= 0)
                {
                    encoding = incomingHeaders.GetHeader<string>(index);

                    try
                    {
                        if (!string.IsNullOrEmpty(encoding))
                        {
                            inputEncoding = Encoding.GetEncoding(encoding);
                        }
                    }
                    catch (ArgumentException e)
                    {
                        this.Trace(TraceLevel.Warning, e, "Could not apply input encoding {0}.", encoding);
                        throw new FaultException<InternalFaultDetail>(new InternalFaultDetail(e.Message, FileStagingErrorCode.TargetIOFailure, e));
                    }
                }

                try
                {
                    // Create the directory if necessary
                    if (mode != FileMode.Open && mode != FileMode.Truncate && !Directory.Exists(Path.GetDirectoryName(path)))
                    {
                        this.Trace(TraceLevel.Verbose, "Attemping to create the directory \"{0}\"", Path.GetDirectoryName(path));
                        Directory.CreateDirectory(Path.GetDirectoryName(path));
                    }

                    FileStream stream = File.Open(path, mode, FileAccess.Read, FileShare.ReadWrite);
                    if (!lines)
                    {
                        if (!backward)
                            stream.Seek(position, SeekOrigin.Begin);
                        else
                            stream.Seek(position, SeekOrigin.End);
                    }
                    else if (backward)
                    {
                        if (null == inputEncoding)
                        {
                            // As encoding auto-detecting may work here, we need to send BOM with the result
                            // to tell user the encoding.
                            MemoryStream bufferStream = new MemoryStream();
                            long pos = FileUtil.TailFile(stream, (int)position, bufferStream, inputEncoding);
                            bufferStream.Seek(0, SeekOrigin.Begin);
                            this.Trace(TraceLevel.Verbose, "Succeeded to read file \"{0}\" at position {1} , mode={2}", path, position, mode);
                            return bufferStream;
                        }
                        else
                        {
                            // No need to considering encoding for user here.
                            long pos = FileUtil.TailFile(stream, (int)position, null, inputEncoding);

                            // Stream is closed after calling TailFile.
                            stream = File.Open(path, mode, FileAccess.Read, FileShare.ReadWrite);
                            stream.Seek(pos, SeekOrigin.Begin);
                        }
                    }

                    this.Trace(TraceLevel.Verbose, "Succeeded to read file \"{0}\" at position {1}, mode={2}", path, position, mode);
                    return stream;
                }
                catch (Exception ex)
                {
                    this.Trace(TraceLevel.Warning, ex, "Could not read file \"{0}\" at position {1}.", path, position);
                    throw new FaultException<InternalFaultDetail>(new InternalFaultDetail(ex.Message, FileStagingErrorCode.TargetIOFailure, ex));
                }
            }
        }

        /// <summary>
        /// Write contents in a stream to the target file
        /// </summary>
        /// <param name="contents">stream to be written</param>
        public virtual void WriteFile(Stream contents)
        {
            string logicalName = GetInputFromHeader<string>(FileStagingCommon.WcfHeaderTargetNode);
            this.Trace(TraceLevel.Verbose, "Received WriteFile message targeting node {0}.", logicalName);

            // Forward the message if this node is not the target
            if (!this.IsTargetedNode(logicalName))
            {
                using (OperationContextScope scope = new OperationContextScope(this.GetProxyChannel().InnerChannel))
                {
                    this.ForwardMessage("WriteFile");
                }
            }
            else
            {
                string path = this.TargetPath;
                FileMode mode = GetInputFromHeader<FileMode>(FileStagingCommon.WcfHeaderMode);
                long position = GetInputFromHeader<long>(FileStagingCommon.WcfHeaderPosition);
                string oldFile = string.Empty;

                try
                {
                    // If the file already exists, rename the old file so that we can revert in an error. The file
                    // must be renamed to something unique, so we use Path.GetRandomFileName to generate a unique
                    // filename, and Path.Combine in order to make sure that it is in the same directory as the
                    // existing file.
                    if (File.Exists(path))
                    {
                        oldFile = Path.Combine(Path.GetDirectoryName(path), Path.GetRandomFileName());
                        File.Move(path, oldFile);
                    }
                }
                catch (Exception ex)
                {
                    this.Trace(TraceLevel.Warning, ex, "Could not write file \"{0}\" at position {1}.", path, position);
                    throw new FaultException<InternalFaultDetail>(new InternalFaultDetail(ex.Message, FileStagingErrorCode.TargetIOFailure, ex));
                }

                try
                {
                    // Create the directory if necessary
                    if (mode != FileMode.Open && mode != FileMode.Truncate && !Directory.Exists(Path.GetDirectoryName(path)))
                    {
                        this.Trace(TraceLevel.Verbose, "Attemping to create the directory \"{0}\"", Path.GetDirectoryName(path));
                        Directory.CreateDirectory(Path.GetDirectoryName(path));
                    }

                    using (FileStream stream = File.Open(path, mode, FileAccess.Write, FileShare.None))
                    {
                        int read;
                        byte[] buffer = new byte[FileStagingCommon.FileWriteChunkSize];
                        stream.Seek(position, SeekOrigin.Begin);

                        do
                        {
                            read = contents.Read(buffer, 0, buffer.Length);
                            if (read > 0)
                            {
                                stream.Write(buffer, 0, read);
                            }
                        }
                        while (read > 0);
                    }

                    // Remove the temporary file
                    if (!string.IsNullOrEmpty(oldFile))
                    {
                        File.Delete(oldFile);
                    }

                    this.Trace(TraceLevel.Verbose, "Suceeded to write file \"{0}\" at position {1}, mode={2}", path, position, mode);
                }
                catch (Exception ex)
                {
                    try
                    {
                        // Clean up by deleting the file and replacing it with the old one, if there was an old one
                        File.Delete(path);
                        if (!string.IsNullOrEmpty(oldFile))
                        {
                            File.Move(oldFile, path);
                        }
                    }
                    catch (Exception exception)
                    {
                        this.Trace(TraceLevel.Warning, exception, "Could not clean up temporary file \"{0}\".", oldFile);
                    }

                    this.Trace(TraceLevel.Warning, ex, "Could not write file \"{0}\" at position {1}.", path, position);
                    throw new FaultException<InternalFaultDetail>(new InternalFaultDetail(ex.Message, FileStagingErrorCode.TargetIOFailure, ex));
                }
            }
        }

        /// <summary>
        /// Delete the target file
        /// </summary>
        public virtual void DeleteFile()
        {
            string logicalName = GetInputFromHeader<string>(FileStagingCommon.WcfHeaderTargetNode);
            this.Trace(TraceLevel.Verbose, "Received DeleteFile message targeting node {0}.", logicalName);

            // Forward the message if this node is not the target
            if (!this.IsTargetedNode(logicalName))
            {
                using (OperationContextScope scope = new OperationContextScope(this.GetProxyChannel().InnerChannel))
                {
                    this.ForwardMessage("DeleteFile");
                }
            }
            else
            {
                string path = this.TargetPath;

                try
                {
                    File.Delete(path);
                    this.Trace(TraceLevel.Verbose, "Succeeded to delete file \"{0}\".", path);
                }
                catch (Exception ex)
                {
                    this.Trace(TraceLevel.Warning, ex, "Could not delete file \"{0}\".", path);
                    throw new FaultException<InternalFaultDetail>(new InternalFaultDetail(ex.Message, FileStagingErrorCode.TargetIOFailure, ex));
                }
            }
        }

        /// <summary>
        /// Returns an array of directories that matches specified search pattern and search option under target directory
        /// </summary>
        /// <param name="searchPattern">search pattern</param>
        /// <param name="searchOption">search option</param>
        /// <returns>an array of directories that matches specified search pattern and search option under target directory</returns>
        public virtual ClusterDirectoryInfo[] GetDirectories(string searchPattern, System.IO.SearchOption searchOption)
        {
            string logicalName = GetInputFromHeader<string>(FileStagingCommon.WcfHeaderTargetNode);
            this.Trace(TraceLevel.Verbose, "Received GetDirectories message targeting node {0}", logicalName);

            // Forward the message if this node is not the target
            if (!this.IsTargetedNode(logicalName))
            {
                using (OperationContextScope scope = new OperationContextScope(this.GetProxyChannel().InnerChannel))
                {
                    return this.ForwardMessage("GetDirectories").GetBody<ClusterDirectoryInfo[]>();
                }
            }
            else
            {
                string path = this.TargetPath;

                try
                {
                    DirectoryInfo directory = new DirectoryInfo(path);
                    DirectoryInfo[] directoryInfo = directory.GetDirectories(searchPattern, searchOption);

                    // Serialize each DirectoryInfo as a ClusterDirectoryInfo
                    ClusterDirectoryInfo[] clusterInfo = new ClusterDirectoryInfo[directoryInfo.Length];
                    for (int i = 0; i < directoryInfo.Length; i++)
                    {
                        clusterInfo[i] = new ClusterDirectoryInfo(directoryInfo[i]);
                    }
                    this.Trace(TraceLevel.Verbose, "Succeeded to get directories in \"{0}\" with searchPattern {1} and option {2}.", path, searchPattern, searchOption);
                    return clusterInfo;
                }
                catch (Exception ex)
                {
                    this.Trace(TraceLevel.Warning, ex, "Could not get directories in \"{0}\" with search string {1} and option {2}.", path, searchPattern, searchOption);
                    throw new FaultException<InternalFaultDetail>(new InternalFaultDetail(ex.Message, FileStagingErrorCode.TargetIOFailure, ex));
                }
            }
        }

        /// <summary>
        /// Delete the target directory
        /// </summary>
        /// <param name="recursive">if delete the directory recursively. if set to false, only empty directory can be deleted</param>
        public virtual void DeleteDirectory(bool recursive)
        {
            string logicalName = GetInputFromHeader<string>(FileStagingCommon.WcfHeaderTargetNode);
            this.Trace(TraceLevel.Verbose, "Received DeleteDirectory message targeting node {0}", logicalName);

            // Forward the message if this node is not the target
            if (!this.IsTargetedNode(logicalName))
            {
                using (OperationContextScope scope = new OperationContextScope(this.GetProxyChannel().InnerChannel))
                {
                    this.ForwardMessage("DeleteDirectory");
                }
            }
            else
            {
                string path = this.TargetPath;

                try
                {
                    Directory.Delete(path, recursive);
                    this.Trace(TraceLevel.Verbose, "Succeeded to delete directory \"{0}\" with recursive={1}.", path, recursive);
                }
                catch (Exception ex)
                {
                    this.Trace(TraceLevel.Warning, ex, "Could not delete directory \"{0}\" with recursive={1}.", path, recursive);
                    throw new FaultException<InternalFaultDetail>(new InternalFaultDetail(ex.Message, FileStagingErrorCode.TargetIOFailure, ex));
                }
            }
        }

        /// <summary>
        /// Returns an array of files that matches specified search pattern and search option under target directory
        /// </summary>
        /// <param name="searchPattern">search pattern</param>
        /// <param name="searchOption">search option</param>
        /// <returns>an array of files that matches specified search pattern and search option under target directory</returns>
        public virtual ClusterFileInfo[] GetFiles(string searchPattern, System.IO.SearchOption searchOption)
        {
            string logicalName = GetInputFromHeader<string>(FileStagingCommon.WcfHeaderTargetNode);
            this.Trace(TraceLevel.Verbose, "Received GetFiles message targeting node {0}.", logicalName);

            // Forward the message if this node is not the target
            if (!this.IsTargetedNode(logicalName))
            {
                using (OperationContextScope scope = new OperationContextScope(this.GetProxyChannel().InnerChannel))
                {
                    return this.ForwardMessage("GetFiles").GetBody<ClusterFileInfo[]>();
                }
            }
            else
            {
                string path = this.TargetPath;

                try
                {
                    DirectoryInfo directory = new DirectoryInfo(path);
                    FileInfo[] fileInfo = directory.GetFiles(searchPattern, searchOption);

                    // Serialize each FileInfo as a ClusterFileInfo
                    ClusterFileInfo[] clusterInfo = new ClusterFileInfo[fileInfo.Length];
                    for (int i = 0; i < fileInfo.Length; i++)
                    {
                        clusterInfo[i] = new ClusterFileInfo(fileInfo[i]);
                    }

                    this.Trace(TraceLevel.Verbose, "Succeeded to get files in \"{0}\" with search string {1} and option {2}.", path, searchPattern, searchOption);
                    return clusterInfo;
                }
                catch (Exception ex)
                {
                    this.Trace(TraceLevel.Warning, ex, "Could not get files in \"{0}\" with search string {1} and option {2}.", path, searchPattern, searchOption);
                    throw new FaultException<InternalFaultDetail>(new InternalFaultDetail(ex.Message, FileStagingErrorCode.TargetIOFailure, ex));
                }
            }
        }

        /// <summary>
        /// Copy a file from local to an Azure with the specified blob url
        /// </summary>
        /// <param name="blobUrl">destination blob url</param>
        /// <param name="sas">SAS for accessing the blob</param>
        /// <param name="sourceFilePath">source file path</param>
        public virtual void CopyFileToBlob(string blobUrl, string sas, string sourceFilePath)
        {
            this.Trace(TraceLevel.Information, "CopyFileToBlob: source={0}, dest={1}", sourceFilePath, blobUrl);

            // copy data from local to blob
            try
            {
                FileStagingCommon.UploadFileToBlob(blobUrl, sas, sourceFilePath);
            }
            catch (Exception ex)
            {
                this.Trace(TraceLevel.Error, ex, "Could not copy file from local \"{0}\" to blob \"{1}\".", sourceFilePath, blobUrl);
                throw new FaultException<InternalFaultDetail>(new InternalFaultDetail(ex.Message, FileStagingErrorCode.TargetIOFailure, ex));
            }
        }

        /// <summary>
        /// Copy a file from Azure blob to local
        /// </summary>
        /// <param name="blobUrl">source blob url</param>
        /// <param name="sas">SAS for accessing the blob</param>
        /// <param name="destFilePath">destination file path</param>
        /// <param name="overwrite">if overwrite existing files</param>
        public virtual void CopyFileFromBlob(string blobUrl, string sas, string destFilePath, bool overwrite)
        {
            this.Trace(TraceLevel.Information, "CopyFileFromBlob: source={0}, dest={1}, overwrite={2}.", blobUrl, destFilePath, overwrite);

            // if overwrite == false, make sure localFilePath doesn't exist
            FileMode mode = overwrite ? FileMode.OpenOrCreate : FileMode.CreateNew;
            try
            {
                using (FileStream fs = File.Open(destFilePath, mode))
                {
                }
            }
            catch (Exception ex)
            {
                this.Trace(TraceLevel.Warning, ex, "Could not open destination file {0}.", destFilePath);

                if ((Marshal.GetHRForException(ex) & 0xFFFF) == FileStagingCommon.ErrorFileExists)
                {
                    throw new FaultException<InternalFaultDetail>(new InternalFaultDetail(ex.Message, FileStagingErrorCode.TargetExists, ex));
                }
                else
                {
                    throw new FaultException<InternalFaultDetail>(new InternalFaultDetail(ex.Message, FileStagingErrorCode.TargetIOFailure, ex));
                }
            }

            // copy data from blob to local
            string userName = this.GetUserName();
            try
            {
                FileStagingCommon.DownloadFileFromBlob(blobUrl, sas, destFilePath);

                if (!string.IsNullOrEmpty(userName))
                {
                    // grant user permission to dest file
                    FilePermission.GrantPermission(userName, destFilePath, false, FileSystemRights.FullControl);
                }
            }
            catch (Exception ex)
            {
                this.Trace(TraceLevel.Error, ex, "Could not copy file from blob \"{0}\" to local \"{1}\". User name is \"{2}\".", blobUrl, destFilePath, userName);

                try
                {
                    File.Delete(destFilePath);
                }
                catch
                {
                }

                throw new FaultException<InternalFaultDetail>(new InternalFaultDetail(ex.Message, FileStagingErrorCode.TargetIOFailure, ex));
            }
        }

        /// <summary>
        /// Copy files that match specified file patterns under sourceDir to an Azure blob with the specified blob url prefix
        /// </summary>
        /// <param name="blobUrlPrefix">blob url prefix</param>
        /// <param name="sas">SAS for accessing the blob</param>
        /// <param name="sourceDir">source directory path</param>
        /// <param name="filePatterns">file patterns</param>
        /// <param name="recursive">if copy directories to blob recursively</param>
        /// <param name="overwrite">if overwrite existing blobs</param>
        public virtual void CopyDirectoryToBlob(string blobUrlPrefix, string sas, string sourceDir, List<string> filePatterns, bool recursive, bool overwrite)
        {
            this.Trace(
                TraceLevel.Information,
                "CopyDirectoryToBlob: source={0}, dest={1}, filepattern={2}, recursive={3}, overwrite={4}.",
                sourceDir,
                blobUrlPrefix,
                (filePatterns == null) ? string.Empty : string.Join(",", filePatterns.ToArray()),
                recursive,
                overwrite);

            // copy data from local to blob
            try
            {
                FileStagingCommon.UploadDirectoryToBlob(blobUrlPrefix, sas, sourceDir, filePatterns, recursive, overwrite, this.GetUserName(), this.BeforeCopyToBlob, null);
            }
            catch (Exception ex)
            {
                this.Trace(TraceLevel.Error, ex, "Could not copy directory from local \"{0}\" to blob \"{1}\".", sourceDir, blobUrlPrefix);
                throw new FaultException<InternalFaultDetail>(new InternalFaultDetail(ex.Message, FileStagingErrorCode.TargetIOFailure, ex));
            }
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
        public virtual void CopyDirectoryFromBlob(string blobUrlPrefix, string sas, string destDir, List<string> filePatterns, bool recursive, bool overwrite)
        {
            this.Trace(
                TraceLevel.Information,
                "CopyDirectoryFromBlob: source={0}, dest={1}, filepattern={2}, recursive={3}, overwrite={4}.",
                blobUrlPrefix,
                destDir,
                (filePatterns == null) ? string.Empty : string.Join(",", filePatterns.ToArray()),
                recursive,
                overwrite);

            // copy data from blob to local
            try
            {
                FileStagingCommon.DownloadDirectoryFromBlob(blobUrlPrefix, sas, destDir, filePatterns, recursive, overwrite, this.GetUserName(), this.BeforeCopyFromBlob, this.AfterCopyFromBlob);
            }
            catch (Exception ex)
            {
                this.Trace(TraceLevel.Error, ex, "Could not copy directory from blob \"{0}\" to local \"{1}\".", blobUrlPrefix, destDir);
                throw new FaultException<InternalFaultDetail>(new InternalFaultDetail(ex.Message, FileStagingErrorCode.TargetIOFailure, ex));
            }
        }

        #endregion

        /// <summary>
        /// Determines whether or not the specified node matches up to the node on which the request is being processed
        /// </summary>
        /// <param name="targetNode">target node name</param>
        /// <returns>true if this node is target node, false otherwise</returns>
        protected virtual bool IsTargetedNode(string targetNode)
        {
            // For V3 SP1, this feature is not supported
            return true;
        }

        /// <summary>
        /// Check if user has specified access rights to a file
        /// </summary>
        /// <param name="userName">target user name</param>
        /// <param name="filePath">target file path</param>
        /// <param name="rights">file access rights</param>
        /// <returns>true if user has specified access rights to the file, false otherwise</returns>
        protected virtual void CheckFilePermissions(string userName, string filePath, FileSystemRights rights)
        {
        }

        /// <summary>
        /// Returns a channel to the nearest proxy
        /// </summary>
        /// <returns>a channel to the nearest proxy</returns>
        protected GenericFileStagingClient GetProxyChannel()
        {
            lock (this.channelLock)
            {
                if (this.proxyChannel == null
                    || this.proxyChannel.State == CommunicationState.Faulted
                    || this.proxyChannel.State == CommunicationState.Closing
                    || this.proxyChannel.State == CommunicationState.Closed)
                {
                    if (this.proxyChannel != null)
                    {
                        try
                        {
                            this.proxyChannel.Abort();
                        }
                        catch (Exception ex)
                        {
                            this.Trace(TraceLevel.Error, ex, "Error aborting problematic proxy channel, endpoint = {0}", this.proxyChannel.Endpoint);
                        }
                    }

                    this.Trace(TraceLevel.Verbose, "Opening or re-opening proxy channel.");
                    this.proxyChannel = this.CreateProxyChannel();
                }

                return this.proxyChannel;
            }
        }

        /// <summary>
        /// Creates a channel to the nearest proxy
        /// </summary>
        /// <returns>a channel to the nearest proxy</returns>
        protected abstract GenericFileStagingClient CreateProxyChannel();

        /// <summary>
        /// Gets a value from the message header with the specified name
        /// </summary>
        /// <typeparam name="T">input header type</typeparam>
        /// <param name="headerName">header name</param>
        /// <returns>value of type T from the message header with the specified header name</returns>
        protected T GetInputFromHeader<T>(string headerName)
        {
            try
            {
                MessageHeaders incomingHeaders = OperationContext.Current.IncomingMessageHeaders;
                T input = incomingHeaders.GetHeader<T>(headerName, FileStagingCommon.WcfHeaderNamespace);

                return input;
            }
            catch (Exception ex)
            {
                this.Trace(TraceLevel.Error, ex, "Could not read \"{0}\" from message headers.", headerName);
                throw new FaultException<InternalFaultDetail>(new InternalFaultDetail(string.Format(Resources.Common_MissingHeaders, headerName), FileStagingErrorCode.CommunicationFailure));
            }
        }

        /// <summary>
        /// Get user name of the request
        /// </summary>
        /// <returns>user name of the request</returns>
        protected virtual string GetUserName()
        {
            string userName;

            try
            {
                userName = GetInputFromHeader<string>(FileStagingCommon.WcfHeaderUserName);
                this.Trace(TraceLevel.Verbose, "Found user name in headers: {0}", userName);
                return userName;
            }
            catch (Exception ex)
            {
                this.Trace(TraceLevel.Error, ex, "The user name is missing from the headers.");
                throw new FaultException<InternalFaultDetail>(new InternalFaultDetail(string.Format(Resources.Common_MissingHeaders, FileStagingCommon.WcfHeaderUserName), FileStagingErrorCode.AuthenticationFailed));
            }
        }

        /// <summary>
        /// Get isAdmin of the request
        /// </summary>
        /// <returns></returns>
        protected virtual bool GetIsAdmin()
        {
            try
            {
                bool isAdmin = GetInputFromHeader<bool>(FileStagingCommon.WcfHeaderIsAdmin);
                this.Trace(TraceLevel.Verbose, "Found isAdmin in headers {0}", isAdmin);
                return isAdmin;
            }
            catch (Exception ex)
            {
                this.Trace(TraceLevel.Warning, ex, "The isAdmin is missing from the headers.");
                return false;
            }
        }

        /// <summary>
        /// Derived classes must provide a way for the abstact implementation to trace messages
        /// </summary>
        /// <param name="level">trace level</param>
        /// <param name="format">trace formatting string</param>
        /// <param name="args">trace arguments</param>
        protected abstract void Trace(TraceLevel level, string format, params object[] args);

        /// <summary>
        /// Derived classes must provide a way for the abstact implementation to trace exceptions
        /// </summary>
        /// <param name="level">trace level</param>
        /// <param name="ex">related exception</param>
        /// <param name="format">trace formatting string</param>
        /// <param name="args">trace arguments</param>        
        protected abstract void Trace(TraceLevel level, Exception ex, string format, params object[] args);

        /// <summary>
        /// Forwards the message that is being processed to the proxy
        /// </summary>
        /// <returns>reply message</returns>
        private Message ForwardMessage(string msgName)
        {
            GenericFileStagingClient proxyClient = this.GetProxyChannel();
            this.Trace(TraceLevel.Verbose, "Forwarding the {0} message to {1}.", msgName, proxyClient.Endpoint);
            return proxyClient.ProcessMessage(OperationContext.Current.RequestContext.RequestMessage);
        }
    }
}
