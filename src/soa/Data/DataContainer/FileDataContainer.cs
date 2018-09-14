//------------------------------------------------------------------------------
// <copyright file="FileDataContainer.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      File share data container implementation
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session.Data.DataContainer
{
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Cryptography;
    using System.Threading;
    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Data.Internal;
    using Microsoft.Win32;

    /// <summary>
    /// File share data container
    /// </summary>
    internal class FileDataContainer : IDataContainer
    {
#region private fields

        /// <summary>
        /// Retry limit
        /// </summary>
        private const int RetryLimitOnSystemError = 10;

        /// <summary>
        /// Default SMB2.0 client redirector FileNotFoundCache entry lifetime: 5 seconds
        /// </summary>
        private const int DefaultSMB2ClientFileNotFoundCacheLifetime = 5;

        /// <summary>
        /// Wait period before retry on FileNotFoundException: 500 ms.
        /// </summary>
        private const int FileNotFoundExceptionRetryPeriod = 500;

        /// <summary>
        /// Maximum retry times on FileNotFoundException: 11 times. (11 * 500 = 5.5 s).
        /// </summary>
        private const int FileNotFoundExceptionRetryLimit = 11;

        /// <summary>
        /// Wait period before retry on ERROR_NETNAME_DELETED error: 1000 ms
        /// </summary>
        private const int NetNameDeletedExceptionRetryPeriod = 1000;

        /// <summary>
        /// Default maximum retry count
        /// </summary>
        private const int MaxRetryCount = 3;

        /// <summary>
        /// Registry key that control SMB2.0 client redirector cache
        /// </summary>
        private static string smb2ClientFileNotFoundCacheRegistryKey = @"System\CurrentControlSet\Services\Lanmanworkstation\Parameters";

        /// <summary>
        /// Name of the value that controls SMB2.0 client redirector FIleNotFoundCache lifetime
        /// </summary>
        private static string smb2ClientFileNotFoundCacheRegistryName = "FileNotFoundCacheLifetime";

        /// <summary>
        /// Name of the environment variable indicates the local path of common data
        /// </summary>
        private static string localCachePathEnvName = "CCP_DATA_LOCAL_PATH";

        /// <summary>
        /// Name of the environment variable indicates the local path of common data is Memory Mapped File
        /// </summary>
        private static string inMemoryFileIndicator = "in_memory";

        /// <summary>
        /// A flag indicating if SMB2.0 client redirector FileNotFoundCache is disabled
        /// </summary>
        /// <remarks>
        /// Related bug: 14480.
        /// SMB 2.0 is introduced since Windows Server 2008. The smb protocol version to
        /// be used for file operations is decided during the negotiation phase. SMB2.0 is
        /// chosen for communication only if both client and server understand SMB2.0.
        /// So the method here to check if client and smb server is talking via SMB2.0 is
        /// inaccurate if client or server OS version is less than Windows server 2008.
        /// For example, if client runs on Windows XP, client and server actually will
        /// talk via SMB1.0, but IsSMB2ClientFileNotFoundCacheDisabled is set to false.
        /// </remarks>
        private static bool isSMB2ClientFileNotFoundCacheDisabled;

        /// <summary>
        /// Read/write buffer size
        /// </summary>
        private static int bufferSize;

        /// <summary>
        /// container file path
        /// </summary>
        private string filePath;

        /// <summary>
        /// A flag telling if this data container instance has ever been accessed (read/write)
        /// </summary>
        private bool accessFlag;

#endregion

        /// <summary>
        /// Initializes static members of the FileDataContainer class
        /// </summary>
        static FileDataContainer()
        {
            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            DataContainerConfiguration dataProviderConfig = DataContainerConfiguration.GetSection(config);
            bufferSize = dataProviderConfig.FileShareBufferSizeInKiloBytes * 1024;

            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(smb2ClientFileNotFoundCacheRegistryKey))
                {
                    int fileNotFoundCacheLifetime = (int)key.GetValue(smb2ClientFileNotFoundCacheRegistryName, DefaultSMB2ClientFileNotFoundCacheLifetime);
                    isSMB2ClientFileNotFoundCacheDisabled = fileNotFoundCacheLifetime == 0;
                }
            }
            catch (Exception e)
            {
                TraceHelper.TraceSource.TraceEvent(TraceEventType.Error, 0, "[FileDataContainer] .Static constructor: received exception = {0}", e);
            }
        }

        /// <summary>
        /// Initializes a new instance of the FileDataContainer class
        /// </summary>
        /// <param name="containerFilePath">container file path</param>
        public FileDataContainer(string containerFilePath)
        {
            this.filePath = containerFilePath;

            TraceHelper.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "[FileDataContainer] .Constructor: path={0}", this.filePath);
        }

        /// <summary>
        /// Gets data container id
        /// </summary>
        public string Id
        {
            get
            {
                return Path.GetFileName(this.filePath);
            }
        }

        /// <summary>
        /// Check if has read permission to the specified container file
        /// </summary>
        /// <param name="containerFilePath">container file path</param>
        public static void CheckReadPermission(string containerFilePath)
        {
            TraceHelper.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "[FileDataContainer] .CheckReadPermission: path={0}", containerFilePath);

            try
            {
                // try to open it
                using (FileStream fs = File.Open(containerFilePath, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete))
                {
                }
            }
            catch (FileNotFoundException e)
            {
                // If access-based enumeration is enabled on the file share, FileNotFoundException could
                // actually indicates the caller has no permission to the file. So Double check if file
                // exists using File.Exists.
                // TODO: FIXME! if a DataClient with the same id is created before File.Exists() is called,
                // then the double check doesn't work. Solution is to have unique file path for each DataClient.
                if (File.Exists(containerFilePath))
                {
                    throw new UnauthorizedAccessException(SR.DataNoPermission, e);
                }
                else
                {
                    throw new DataException(DataErrorCode.DataClientNotFound, e);
                }
            }
            catch (IOException e)
            {
                int dataErrorCode = GetDataErrorCode(e);
                throw new DataException(dataErrorCode, e);
            }
            catch (SecurityException e)
            {
                throw new SecurityException(SR.DataNoPermission, e);
            }
            catch (UnauthorizedAccessException e)
            {
                throw new UnauthorizedAccessException(SR.DataNoPermission, e);
            }
            catch (Exception e)
            {
                throw new DataException(DataErrorCode.Unknown, e);
            }
        }

        /// <summary>
        /// Returns a path that tells where the data is stored
        /// </summary>
        /// <returns>path telling where the data is stored</returns>
        public string GetStorePath()
        {
            return this.filePath;
        }

        /// <summary>
        /// Get the content Md5
        /// </summary>
        /// <returns>The base64 md5 string</returns>
        public string GetContentMd5()
        {
            using (var md5 = MD5.Create())
            {
                var hashBytes = md5.ComputeHash(this.GetData());
                return Convert.ToBase64String(hashBytes);
            }
        }

        /// <summary>
        /// Write a data item into data container and flush
        /// </summary>
        /// <param name="data">data content to be written</param>
        public void AddDataAndFlush(DataContent data)
        {
            TraceHelper.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "[FileDataContainer] .AddDataAndFlush");

            int retryCount = 0;
            int retryCountForNetNameDeleted = 0;
            while (true)
            {
                try
                {
                    // write data
                    using (FileStream fs = new FileStream(this.filePath, FileMode.Open, FileAccess.Write, FileShare.Read | FileShare.Delete, bufferSize))
                    {
                        this.accessFlag = true;

                        // write data item
                        data.Dump(fs);
                    }

                    return;
                }
                catch (IOException e)
                {
                    int errorCode = GetIOErrorCode(e);
                    if (errorCode == FileNativeMethods.ErrorFileNotFound)
                    {
                        // Bug 14480: FileNotFoundException may be thrown because of SMB2.0 client redirector cache.
                        if (this.accessFlag || isSMB2ClientFileNotFoundCacheDisabled || retryCount >= FileNotFoundExceptionRetryLimit)
                        {
                            throw new DataException(DataErrorCode.DataClientDeleted, e);
                        }

                        Thread.Sleep(FileNotFoundExceptionRetryPeriod);
                        retryCount++;
                    }
                    else if (errorCode == FileNativeMethods.ErrorNetNameDeleted && retryCountForNetNameDeleted < MaxRetryCount)
                    {
                        // Bug 14068: ERROR_NETNAME_DELETED error may occur.  As it is intermediate, fix it by retry
                        Thread.Sleep(NetNameDeletedExceptionRetryPeriod);
                        retryCountForNetNameDeleted++;
                    }
                    else
                    {
                        int dataErrorCode = GetDataErrorCode(e);
                        throw new DataException(dataErrorCode, e);
                    }
                }
                catch (SecurityException e)
                {
                    throw new SecurityException(SR.DataNoPermission, e);
                }
                catch (UnauthorizedAccessException e)
                {
                    throw new UnauthorizedAccessException(SR.DataNoPermission, e);
                }
                catch (SerializationException)
                {
                    throw;
                }
                catch (DataException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new DataException(DataErrorCode.Unknown, e);
                }
            }
        }

        /// <summary>
        /// Gets data content from the data container
        /// </summary>
        /// <returns>data content in the data container</returns>
        public byte[] GetData()
        {
            TraceHelper.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "[FileDataContainer] .GetData");

            // When COMMONDATA_LOCAL_PATH exists, we will read from the local path instead of remote path
            // And fall back to original path if file not exists
            var tempFilePath = this.filePath;
            var localPath = Environment.GetEnvironmentVariable(localCachePathEnvName);
            var inMemoryFile = false;
            if (!string.IsNullOrEmpty(localPath))
            {
                if (string.Compare(localPath, inMemoryFileIndicator, true) == 0)
                {
                    inMemoryFile = true;
                }
                else
                {
                    tempFilePath = Path.Combine(localPath + Id);
                }
            }

            int retryCount = 0;
            int retryCountForNetNameDeleted = 0;
            while (true)
            {
                try
                {
                    // read from in memory file
                    if (inMemoryFile)
                    {
                        using (var mmf = System.IO.MemoryMappedFiles.MemoryMappedFile.OpenExisting(Id, System.IO.MemoryMappedFiles.MemoryMappedFileRights.Read))
                        {
                            using (var mmvs = mmf.CreateViewStream())
                            {
                                this.accessFlag = true;

                                byte[] data = new byte[mmvs.Length];

                                // read data item
                                int len = ReadBytes(mmvs, data, data.Length);
                                TraceHelper.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "[FileDataContainer] .GetData: Memeory Mapped File {0} length={1}, read out {2} bytes", Id, mmvs.Length, len);

                                return data;
                            }
                        }
                    }

                    // TODO: FIXME! GetData may return incorrect data if the DataClient is deleted and another one with the same name
                    // is added in between FileDataContainer.Open and FileDataContainer.GetData().
                    using (FileStream fs = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete, bufferSize, FileOptions.SequentialScan))
                    {
                        this.accessFlag = true;

                        byte[] data = new byte[fs.Length];

                        // read data item
                        int len = ReadBytes(fs, data, data.Length);
                        TraceHelper.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "[FileDataContainer] .GetData: file {0} length={1}, read out {2} bytes", tempFilePath, fs.Length, len);

                        return data;
                    }
                }
                catch (IOException e)
                {
                    // if this is in memory or local path, we need fall back to the orignal path
                    if (inMemoryFile || this.filePath != tempFilePath)
                    {
                        tempFilePath = this.filePath;
                        inMemoryFile = false;
                        continue;
                    }

                    int errorCode = GetIOErrorCode(e);
                    if (errorCode == FileNativeMethods.ErrorFileNotFound)
                    {
                        // Bug 14480: FileNotFoundException may be thrown because of SMB2.0 client redirector cache.
                        if (this.accessFlag || isSMB2ClientFileNotFoundCacheDisabled || retryCount >= FileNotFoundExceptionRetryLimit)
                        {                            
                            throw new DataException(DataErrorCode.DataClientDeleted, e);
                        }

                        Thread.Sleep(FileNotFoundExceptionRetryPeriod);
                        retryCount++;
                    }
                    else if (errorCode == FileNativeMethods.ErrorNetNameDeleted && retryCountForNetNameDeleted < MaxRetryCount)
                    {
                        // Bug 14068: ERROR_NETNAME_DELETED error may occur.  As it is intermediate, fix it by retry
                        Thread.Sleep(NetNameDeletedExceptionRetryPeriod);
                        retryCountForNetNameDeleted++;
                    }
                    else
                    {
                        int dataErrorCode = GetDataErrorCode(e);
                        throw new DataException(dataErrorCode, e);
                    }
                }
                catch (SecurityException e)
                {
                    // if this is in memory or local path, we need fall back to the orignal path
                    if (inMemoryFile || this.filePath != tempFilePath)
                    {
                        tempFilePath = this.filePath;
                        inMemoryFile = false;
                        continue;
                    }
                    throw new UnauthorizedAccessException(SR.DataNoPermission, e);
                }
                catch (UnauthorizedAccessException e)
                {
                    // if this is in memory or local path, we need fall back to the orignal path
                    if (inMemoryFile || this.filePath != tempFilePath)
                    {
                        tempFilePath = this.filePath;
                        inMemoryFile = false;
                        continue;
                    }
                    throw new UnauthorizedAccessException(SR.DataNoPermission, e);
                }
                catch (Exception e)
                {
                    // if this is in memory or local path, we need fall back to the orignal path
                    if (inMemoryFile || this.filePath != tempFilePath)
                    {
                        tempFilePath = this.filePath;
                        inMemoryFile = false;
                        continue;
                    }
                    throw new DataException(DataErrorCode.Unknown, e);
                }
            }
        }

        /// <summary>
        /// Delete the data container.
        /// </summary>
        public void DeleteIfExists()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Check if the data container exists on data server or not
        /// </summary>
        /// <returns>true if the data container exists, false otherwise</returns>
        public bool Exists()
        {
            try
            {
                return File.Exists(this.filePath);
            }
            catch (IOException e)
            {
                int dataErrorCode = GetDataErrorCode(e);
                throw new DataException(dataErrorCode, e);
            }
            catch (SecurityException e)
            {
                throw new SecurityException(SR.DataNoPermission, e);
            }
            catch (UnauthorizedAccessException e)
            {
                throw new UnauthorizedAccessException(SR.DataNoPermission, e);
            }
            catch (Exception e)
            {
                throw new DataException(DataErrorCode.Unknown, e);
            }
        }

        /// <summary>
        /// Reads data from specified file stream, and fill it into an byte array
        /// </summary>
        /// <param name="fs">source stream</param>
        /// <param name="array">byte array to be filled</param>
        /// <param name="arrayLen">number of bytes to be read</param>
        /// <returns>number of bytes read</returns>
        private static int ReadBytes(Stream fs, byte[] array, int arrayLen)
        {
            int offset = 0;
            int len = arrayLen;
            int retryCount = 0;

            do
            {
                try
                {
                    int bytesRead = fs.Read(array, offset, len);

                    // break if end of file is reached
                    if (bytesRead == 0)
                    {
                        break;
                    }

                    offset += bytesRead;
                    len -= bytesRead;
                    retryCount = 0;
                }
                catch (IOException e)
                {
                    // Fix bug 13219: wait and retry on ERROR_NO_SYSTEM_RESOURCE & ERROR_WORKING_SET_QUOTA
                    int errorCode = GetIOErrorCode(e);
                    if ((errorCode == FileNativeMethods.ErrorNoSystemResource || errorCode == FileNativeMethods.ErrorWorkingSetQuota) && (retryCount < RetryLimitOnSystemError))
                    {
                        // sleep 100 ms and retry
                        Thread.Sleep(100);
                        retryCount++;
                        continue;
                    }

                    throw;
                }
            }
            while (offset < arrayLen);

            return offset;
        }

        /// <summary>
        /// Map IOException to DataErrorCode
        /// </summary>
        /// <param name="e">the IOException to be interpreted</param>
        /// <returns>corresponding DataErrorCode</returns>
        private static int GetDataErrorCode(IOException e)
        {
            int hResult = Marshal.GetHRForException(e);
            switch (hResult & 0xFFFF)
            {
                case FileNativeMethods.ErrorFileExists:
                    return DataErrorCode.DataClientAlreadyExists;
                case FileNativeMethods.ErrorDiskFull:
                    return DataErrorCode.DataServerNoSpace;
                case FileNativeMethods.ErrorFileNotFound:
                case FileNativeMethods.ErrorPathNotFound:
                    return DataErrorCode.DataClientNotFound;
                case FileNativeMethods.ErrorSharingViolation:
                    return DataErrorCode.DataClientBusy;
                case FileNativeMethods.ErrorNetWorkPathNotFound:
                case FileNativeMethods.ErrorNetworkPathNotFound2:
                    return DataErrorCode.DataServerUnreachable;
                case FileNativeMethods.ErrorNetworkBusy:
                case FileNativeMethods.ErrorNoSystemResource:
                case FileNativeMethods.ErrorWorkingSetQuota:
                    return DataErrorCode.DataRetry;
                case FileNativeMethods.ErrorBadNetworkName:
                    return DataErrorCode.DataServerBadAddress;
                case FileNativeMethods.ErrorNetworkUnexpected:
                default:
                    return DataErrorCode.Unknown;
            }
        }

        /// <summary>
        /// Get IO error code from IOException
        /// </summary>
        /// <param name="e">the IOException to be interpreted</param>
        /// <returns>corresponding IO error code</returns>
        private static int GetIOErrorCode(IOException e)
        {
            return Marshal.GetHRForException(e) & 0xffff;
        }
    }
}
