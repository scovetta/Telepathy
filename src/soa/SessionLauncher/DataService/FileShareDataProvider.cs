//------------------------------------------------------------------------------
// <copyright file="FileShareDataProvider.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      File share data provider implementation
// </summary>
//------------------------------------------------------------------------------
#if HPCPACK

namespace Microsoft.Hpc.Scheduler.Session.Data.DataProvider
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security;
    using System.Security.AccessControl;
    using System.Security.Principal;
    using Microsoft.Hpc.Scheduler.Session.Data.Internal;
    using Microsoft.Win32.SafeHandles;

    using TelepathyCommon.HpcContext;
    using TelepathyCommon.HpcContext.Extensions;

    using TraceHelper = Microsoft.Hpc.Scheduler.Session.Data.Internal.DataServiceTraceHelper;

    /// <summary>
    /// File share data provider
    /// </summary>
    internal class FileShareDataProvider : IDataProvider
    {
#region private fields

        /// <summary>
        /// Maximum number of subdirectories under file server
        /// </summary>
        private const int MaxSubDirectoryCount = 1024;

        /// <summary>
        /// Max retry count
        /// </summary>
        private const int MaxRetryCount = 3;

        /// <summary>
        /// Container attribute data stream name
        /// </summary>
        private const string AttributeStreamName = ":attribute";

        /// <summary>
        /// Maximum attribute data stream length
        /// </summary>
        private const int MaxAttributeStreamLength = 8192;

        /// <summary>
        /// Local system account name
        /// </summary>
        private static string localSystemAccount = @"NT AUTHORITY\SYSTEM";

        /// <summary>
        /// Administrators account name
        /// </summary>
        private static string administratorsAccount = "Administrators";

        /// <summary>
        /// data server information
        /// </summary>
        private DataServerInfo dataServerInfo;

        /// <summary>
        /// root directory path of container files
        /// </summary>
        private string containerRootDirPath;

#endregion

        /// <summary>
        /// Initializes a new instance of the FileShareDataProvider class
        /// </summary>
        /// <param name="info">data server information</param>
        public FileShareDataProvider(DataServerInfo info)
        {
            Utility.ValidateDataServerInfo(info);

            this.dataServerInfo = info;
            this.containerRootDirPath = info.AddressInfo;
        }

        /// <summary>
        /// Prepare the data server for use
        /// </summary>
        /// <param name="dataServerInfo">information about the data server</param>
        public static void InitializeDataServer(DataServerInfo dataServerInfo)
        {
            Utility.ValidateDataServerInfo(dataServerInfo);

            // Create soa file share root: $HPC_RUNTIMESHARE\SOA
            string containerRootDirPath = dataServerInfo.AddressInfo;
            CreateRootDirectory(containerRootDirPath);

            // Create data container folders under soa file folder: $HPC_RUNTIMESHARE\SOA\0...1023
            for (int i = 0; i < MaxSubDirectoryCount; i++)
            {
                string path = Path.Combine(containerRootDirPath, i.ToString());
                CreateContainerDirectory(path);
            }
        }

        /// <summary>
        /// Create a new data container
        /// </summary>
        /// <param name="name">data container name</param>
        /// <returns>info for accessing the data container</returns>
        public DataClientInfo CreateDataContainer(string name)
        {
            TraceHelper.TraceEvent(TraceEventType.Verbose, "[FileShareDataProvider].CreateDataContainer: name={0}", name);
            string containerFilePath = this.GenerateContainerFilePath(name);
            try
            {
                DataClientInfo info = new DataClientInfo();
                info.PrimaryDataPath = CreateFileContainer(containerFilePath);
                return info;
            }
            catch (DataException e)
            {
                e.DataClientId = name;
                e.DataServer = this.dataServerInfo.AddressInfo;
                throw;
            }
        }

        /// <summary>
        /// Open an existing data container
        /// </summary>
        /// <param name="name">name of the data container to be opened</param>
        /// <returns>info for accessing the data container</returns>
        public DataClientInfo OpenDataContainer(string name)
        {
            TraceHelper.TraceEvent(TraceEventType.Verbose, "[FileShareDataProvider].OpenDataContainer: name={0}", name);

            DataClientInfo info = new DataClientInfo();
            info.PrimaryDataPath = this.GenerateContainerFilePath(name);
            return info;
        }

        /// <summary>
        /// Delete a data container
        /// </summary>
        /// <param name="name">name of the data container to be deleted</param>
        public void DeleteDataContainer(string name)
        {
            TraceHelper.TraceEvent(TraceEventType.Verbose, "[FileShareDataProvider].DeleteDataContainer: name={0}", name);
            string containerFilePath = this.GenerateContainerFilePath(name);
            try
            {
                DeleteFileContainer(containerFilePath);
            }
            catch (DataException e)
            {
                e.DataClientId = name;
                e.DataServer = this.dataServerInfo.AddressInfo;
                throw;
            }
        }

        /// <summary>
        /// Sets container attributes
        /// </summary>
        /// <param name="name">data container name</param>
        /// <param name="attributes">attribute key and value pairs</param>
        /// <remarks>if attribute with the same key already exists, its value will be
        /// updated; otherwise, a new attribute is inserted. Valid characters for 
        /// attribute key and value are: 0~9, a~z</remarks>
        public void SetDataContainerAttributes(string name, Dictionary<string, string> attributes)
        {
            string containerFilePath = this.GenerateContainerFilePath(name);
            string attributePath = containerFilePath + AttributeStreamName;

            Dictionary<string, string> attributesDic = GetAttributesDictionary(attributePath);
            if (attributesDic == null)
            {
                // attributePath doesn't exist
                attributesDic = new Dictionary<string, string>();
            }

            foreach (KeyValuePair<string, string> attribute in attributes)
            {
                TraceHelper.TraceEvent(
                    TraceEventType.Verbose,
                    "[FileShareDataProvider] .SetDataContainerAttribute: name={0}, attribute key={1}, attribute value={2}",
                    name,
                    attribute.Key,
                    attribute.Value);
                attributesDic[attribute.Key] = attribute.Value;
            }

            try
            {
                int error;
                while (true)
                {
                    SafeFileHandle handle = FileNativeMethods.CreateFile(
                        attributePath,
                        (uint)FileNativeMethods.NativeFileAccess.GENERIC_WRITE,
                        (uint)FileNativeMethods.NativeFileShare.FILE_SHARE_DELETE,
                        IntPtr.Zero,
                        (uint)FileNativeMethods.NativeFileMode.CREATE_ALWAYS,
                        0,
                        IntPtr.Zero);

                    if (handle.IsInvalid)
                    {
                        error = Marshal.GetLastWin32Error();
                        break;
                    }

                    byte[] attributesBytes = SerializeAttributesDictionary(attributesDic);
                    Debug.Assert(attributesBytes.Length <= MaxAttributeStreamLength, "too long attributes bytes");

                    uint writtenBytes = 0;
                    bool ret = FileNativeMethods.WriteFile(handle, attributesBytes, (uint)attributesBytes.Length, ref writtenBytes, IntPtr.Zero);
                    if (!ret)
                    {
                        error = Marshal.GetLastWin32Error();
                        handle.Close();
                        break;
                    }

                    handle.Close();
                    return;
                }

                Win32Exception exception = new Win32Exception(error);
                if (error == FileNativeMethods.ErrorFileExists)
                {
                    throw new DataException(DataErrorCode.DataClientLifeCycleSet, exception);
                }
                else
                {
                    throw new DataException(MapIOErrorCodeToDataErrorCode(error), exception);
                }
            }
            catch (InvalidOperationException e)
            {
                // check permission throw InvalidOperationException if data server is not configured properly
                throw new DataException(DataErrorCode.DataServerUnreachable, e);
            }
            catch (IOException e)
            {
                int errorCode = GetIOErrorCode(e);
                if (errorCode == FileNativeMethods.ErrorFileNotFound)
                {
                    throw new DataException(DataErrorCode.DataClientDeleted, e);
                }
                else
                {
                    int dataErrorCode = GetDataErrorCode(e);
                    throw new DataException(dataErrorCode, e);
                }
            }
            catch (SecurityException)
            {
                throw;
            }
            catch (UnauthorizedAccessException)
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

        /// <summary>
        /// Gets container attributes
        /// </summary>
        /// <param name="name"> data container name</param>
        /// <returns>data container attribute key and value pairs</returns>
        public Dictionary<string, string> GetDataContainerAttributes(string name)
        {
            TraceHelper.TraceEvent(TraceEventType.Verbose, "[FileShareDataProvider] .GetDataContainerAttribute: container={0}", name);

            string containerFilePath = this.GenerateContainerFilePath(name);
            string attributePath = containerFilePath + AttributeStreamName;

            return GetAttributesDictionary(attributePath);
        }

        /// <summary>
        /// List all data containers
        /// </summary>
        /// <returns>List of all data containers</returns>
        public IEnumerable<string> ListAllDataContainers()
        {
            TraceHelper.TraceEvent(TraceEventType.Verbose, "[FileShareDataProvider].ListAllDataContainers");

            List<string> allDataContainers = new List<string>();
            for (int i = 0; i < MaxSubDirectoryCount; i++)
            {
                string path = Path.Combine(this.containerRootDirPath, i.ToString());
                allDataContainers.AddRange(ListFileContainers(path));
            }

            return allDataContainers;
        }

        /// <summary>
        /// Set data container permissions
        /// </summary>
        /// <param name="name">data container name</param>
        /// <param name="userName">data container owner</param>
        /// <param name="allowedUsers">privileged users of the data container</param>
        public void SetDataContainerPermissions(string name, string userName, string[] allowedUsers)
        {
            TraceHelper.TraceEvent(TraceEventType.Verbose, "[FileShareDataProvider].SetDataContainerPermissions: name={0}", name);
            string containerFilePath = this.GenerateContainerFilePath(name);

            try
            {
                SetFileContainerPermissions(containerFilePath, userName, allowedUsers);
            }
            catch (DataException e)
            {
                e.DataClientId = name;
                e.DataServer = this.dataServerInfo.AddressInfo;
                throw;
            }
        }

        /// <summary>
        /// Check if a user has specified permission to a data container
        /// </summary>
        /// <param name="name">data container name</param>
        /// <param name="userIdentity">identity of the user to be checked</param>
        /// <param name="permissions">permissions to be checked</param>
        public void CheckDataContainerPermissions(string name, WindowsIdentity userIdentity, DataPermissions permissions)
        {
            TraceHelper.TraceEvent(TraceEventType.Verbose, "[FileShareDataProvider].CheckDataContainerPermissions: name={0}, username={1}", name, userIdentity.Name);
            string containerFilePath = this.GenerateContainerFilePath(name);

            try
            {
                CheckFileContainerPermissions(containerFilePath, userIdentity, permissions);
            }
            catch (DataException e)
            {
                e.DataClientId = name;
                e.DataServer = this.dataServerInfo.AddressInfo;
                throw;
            }
        }

        /// <summary>
        /// Create a new instance of FileDataContainer clas
        /// </summary>
        /// <param name="containerFilePath">container file name</param>
        /// <returns>data container store path</returns>
        private static string CreateFileContainer(string containerFilePath)
        {
            TraceHelper.TraceEvent(TraceEventType.Verbose, "[FileShareDataProvider].Create data container: path={0}", containerFilePath);

            int retryCount = 0;
            while (true)
            {
                try
                {
                    string containerDirPath = Path.GetDirectoryName(containerFilePath);

                    // Open file with:
                    //    FileMode = FileMode.CreateNew.  If two processes performs the operation at the same time, one will be failed
                    //    FileAccess = FileAccess.Write.  This asks for write permission to the file
                    //    FileShare = FileShare.None.  This means the file cannot be accessed by anyone else until it is released by current process
                    //    buffer size.
                    using (FileStream fs = new FileStream(containerFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    {
                        return containerFilePath;
                    }
                }
                catch (InvalidOperationException e)
                {
                    // check permission throw InvalidOperationException if data server is not configured properly
                    throw new DataException(DataErrorCode.DataServerUnreachable, e);
                }
                catch (DirectoryNotFoundException e)
                {
                    throw new DataException(DataErrorCode.DataServerMisconfigured, e);
                }
                catch (IOException e)
                {
                    int errorCode = GetIOErrorCode(e);
                    switch (errorCode)
                    {
                        case FileNativeMethods.ErrorSharingViolation:
                            // file is opened by someone else for write. so return DataClientAlreadyExist exception
                            throw new DataException(DataErrorCode.DataClientAlreadyExists, e);
                        default:
                            // for all other error codes, map it to DataErrorCode.
                            int dataErrorCode = GetDataErrorCode(e);
                            throw new DataException(dataErrorCode, e);
                    }
                }
                catch (SecurityException)
                {
                    throw;
                }
                catch (UnauthorizedAccessException)
                {
                    // Create file may throw UnauthorizedAccessException if the file is being deleted. Retry on it.
                    retryCount++;
                    if (retryCount >= MaxRetryCount)
                    {
                        throw;
                    }
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
        /// Delete a data container file
        /// </summary>
        /// <param name="containerFilePath">data container file path</param>
        private static void DeleteFileContainer(string containerFilePath)
        {
            TraceHelper.TraceEvent(TraceEventType.Verbose, "[FileShareDataProvider].DeleteFileContainer: data path={0}", containerFilePath);

            int retryCount = 0;
            while (true)
            {
                try
                {
                    // remove data file
                    // Note: make sure "access based enumeration" is unchecked on the share.
                    File.Delete(containerFilePath);
                    return;
                }
                catch (InvalidOperationException e)
                {
                    // check permission throw InvalidOperationException if data server is not configured properly
                    throw new DataException(DataErrorCode.DataServerUnreachable, e);
                }
                catch (IOException e)
                {
                    int errorCode = GetIOErrorCode(e);
                    switch (errorCode)
                    {
                        case FileNativeMethods.ErrorFileNotFound:
                        case FileNativeMethods.ErrorPathNotFound:
                            // ignore FileNotFound/PathNotFound error
                            return;
                        default:
                            int dataErrorCode = GetDataErrorCode(e);
                            throw new DataException(dataErrorCode, e);
                    }
                }
                catch (SecurityException)
                {
                    throw;
                }
                catch (UnauthorizedAccessException)
                {
                    // File.Delete may throw UnauthorizedAccessException if the file is being deleted by someone else. Retry on this.
                    retryCount++;
                    if (retryCount >= MaxRetryCount)
                    {
                        throw;
                    }
                }
                catch (Exception e)
                {
                    throw new DataException(DataErrorCode.Unknown, e);
                }
            }
        }

        /// <summary>
        /// List all data containers under specified container directory
        /// </summary>
        /// <param name="containerDirPath">path to the container directory</param>
        /// <returns>List of all data containers under the specified container directory</returns>
        private static IEnumerable<string> ListFileContainers(string containerDirPath)
        {
            List<string> retNs = new List<string>();
            try
            {
                foreach (string filePath in Directory.GetFiles(containerDirPath))
                {
                    retNs.Add(Path.GetFileName(filePath));
                }

                return retNs;
            }
            catch (SecurityException)
            {
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (DirectoryNotFoundException)
            {
                return retNs;
            }
            catch (IOException e)
            {
                int errorCode = GetDataErrorCode(e);
                throw new DataException(errorCode, e);
            }
            catch (Exception e)
            {
                throw new DataException(DataErrorCode.Unknown, e);
            }
        }

        /// <summary>
        /// set ACL for specified container file
        /// </summary>
        /// <param name="containerFilePath">container file path</param>
        /// <param name="userName">user who has full control access to the file</param>
        /// <param name="allowedUsers">privileged users who have read access to the file</param>
        private static void SetFileContainerPermissions(string containerFilePath, string userName, string[] allowedUsers)
        {
            List<FileSystemAccessRule> accessRules = GenerateFileSystemAccessRules(userName, allowedUsers);

            try
            {
                // set access control
                AddAccessRules(containerFilePath, accessRules);
            }
            catch (InvalidOperationException e)
            {
                // check permission throw InvalidOperationException if data server is not configured properly
                throw new DataException(DataErrorCode.DataServerUnreachable, e);
            }
            catch (DirectoryNotFoundException e)
            {
                throw new DataException(DataErrorCode.DataServerMisconfigured, e);
            }
            catch (IOException e)
            {
                int dataErrorCode = GetDataErrorCode(e);
                throw new DataException(dataErrorCode, e);
            }
            catch (SecurityException)
            {
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (System.Security.Principal.IdentityNotMappedException)
            {
                // throw exception out if allowed user is not valid
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

        /// <summary>
        /// Check if a user has specified permission to a container file
        /// </summary>
        /// <param name="containerFilePath">container file path</param>
        /// <param name="userIdentity">identity of the user to be checked</param>
        /// <param name="permissions">permissions to be checked</param>
        private static void CheckFileContainerPermissions(string containerFilePath, WindowsIdentity userIdentity, DataPermissions permissions)
        {
            try
            {
                // check if the caller has permission to the data conatiner
                FileSystemRights rights = 0;
                if ((permissions & DataPermissions.Read) != 0)
                {
                    rights |= FileSystemRights.Read;
                }

                if ((permissions & DataPermissions.SetAttribute) != 0 || (permissions & DataPermissions.Write) != 0)
                {
                    rights |= FileSystemRights.Write;
                }

                if ((permissions & DataPermissions.Delete) != 0)
                {
                    rights |= FileSystemRights.Delete;
                }

                FilePermission.CheckPermission(userIdentity, containerFilePath, false, rights);
            }
            catch (InvalidOperationException e)
            {
                // check permission throw InvalidOperationException
                throw new DataException(DataErrorCode.DataServerUnreachable, e);
            }
            catch (FileNotFoundException e)
            {
                throw new DataException(DataErrorCode.DataClientNotFound, e);
            }
            catch (IOException e)
            {
                int dataErrorCode = GetDataErrorCode(e);
                throw new DataException(dataErrorCode, e);
            }
            catch (SecurityException)
            {
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new DataException(DataErrorCode.Unknown, e);
            }
        }

        /// <summary>
        /// Create root directory for soa common data
        /// </summary>
        /// <param name="rootDirPath">root directory path</param>
        private static void CreateRootDirectory(string rootDirPath)
        {
            try
            {
                IEnumerable<string> hnAccounts = TelepathyContext.Get().GetNodesAsync().GetAwaiter().GetResult().Select(h => string.Format(@"{0}\{1}$", Environment.UserDomainName, h));
                // prepare ACL setting for the directory
                DirectorySecurity ds = new DirectorySecurity();
                ds.SetAccessRuleProtection(true, false);

                // 1. grant "system" (for local file share) and machine accounts (for remote file share) full control access. This rule applies to this folder, subfolders, and files.
                ds.AddAccessRule(new FileSystemAccessRule(localSystemAccount, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                foreach (var hn in hnAccounts)
                {
                    ds.AddAccessRule(new FileSystemAccessRule(hn, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                }

                // 2. grant "administrators" read access to folders and subfolders, and delete access to folders, subfolders, and files.
                ds.AddAccessRule(new FileSystemAccessRule(administratorsAccount, FileSystemRights.Read, InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow));
                ds.AddAccessRule(new FileSystemAccessRule(administratorsAccount, FileSystemRights.Delete, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));

                // 3. grant "everyone" list folder/read data access.  This rule applies to this folder and subfolders.
                ds.AddAccessRule(new FileSystemAccessRule("everyone", FileSystemRights.ListDirectory, InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow));

                // if SOA folder already exists, return.
                if (Directory.Exists(rootDirPath))
                {
                    // check if ACL of the existing dir is expected. if not, log warning.
                    // Bug 14701: Do not override ACL of folder $HPC_RUNTIMESHARE\SOA.
                    DirectorySecurity dirSecurity = Directory.GetAccessControl(rootDirPath);

                    if (!AreSameDirectorySecurityObjects(dirSecurity, ds))
                    {
                        TraceHelper.TraceEvent(TraceEventType.Warning, "[FileShareDataProvider].CreateRootDirectory: {0} already exists but with unexpected ACL settings. This may disrupt common data security", rootDirPath);
                    }

                    return;
                }

                // create the root dir for soa common data and set ACL
                Directory.CreateDirectory(rootDirPath);
                Directory.SetAccessControl(rootDirPath, ds);
            }
            catch (IOException e)
            {
                int dataErrorCode = GetDataErrorCode(e);
                throw new DataException(dataErrorCode, e);
            }
            catch (SecurityException)
            {
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new DataException(DataErrorCode.Unknown, e);
            }
        }

        /// <summary>
        /// Create directory for container file
        /// </summary>
        /// <param name="containerDirPath">container directory path</param>
        private static void CreateContainerDirectory(string containerDirPath)
        {
            try
            {
                Directory.CreateDirectory(containerDirPath);
            }
            catch (IOException e)
            {
                int dataErrorCode = GetDataErrorCode(e);
                throw new DataException(dataErrorCode, e);
            }
            catch (SecurityException)
            {
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new DataException(DataErrorCode.Unknown, e);
            }
        }

        /// <summary>
        /// Map IOException to DataErrorCode
        /// </summary>
        /// <param name="e">the IOException to be interpreted</param>
        /// <returns>corresponding DataErrorCode</returns>
        private static int GetDataErrorCode(IOException e)
        {
            int errorCode = GetIOErrorCode(e);
            return MapIOErrorCodeToDataErrorCode(errorCode);
        }

        /// <summary>
        /// Map IO error code to DataErrorCode
        /// </summary>
        /// <param name="errorCode">the IO error code to be interpreted</param>
        /// <returns>corresponding DataErrorCode</returns>
        private static int MapIOErrorCodeToDataErrorCode(int errorCode)
        {
            switch (errorCode)
            {
                case FileNativeMethods.ErrorFileExists:
                    return DataErrorCode.DataClientAlreadyExists;
                case FileNativeMethods.ErrorDiskFull:
                    return DataErrorCode.DataServerNoSpace;
                case FileNativeMethods.ErrorFileNotFound:
                case FileNativeMethods.ErrorPathNotFound:
                    return DataErrorCode.DataClientNotFound;
                case FileNativeMethods.ErrorAccessDenied:
                    return DataErrorCode.DataNoPermission;
                case FileNativeMethods.ErrorSharingViolation:
                    return DataErrorCode.DataClientBusy;
                case FileNativeMethods.ErrorNetWorkPathNotFound:
                case FileNativeMethods.ErrorNetworkPathNotFound2:
                    return DataErrorCode.DataServerUnreachable;
                case FileNativeMethods.ErrorNetworkBusy:
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

        /// <summary>
        /// Gernerate FileSystemAccessRule for DataClient owner and allowed users
        /// </summary>
        /// <param name="owner">DataClient owner</param>
        /// <param name="allowedUsers">DataClient allowed users</param>
        /// <returns>corresponding FileSystemAccesRules</returns>
        private static List<FileSystemAccessRule> GenerateFileSystemAccessRules(string owner, string[] allowedUsers)
        {
            List<FileSystemAccessRule> accessRules = new List<FileSystemAccessRule>();

            // grant owner full control permission
            accessRules.Add(new FileSystemAccessRule(owner, FileSystemRights.FullControl, AccessControlType.Allow));

            // apply allowed user read & delete permission
            if (allowedUsers != null)
            {
                foreach (string user in allowedUsers)
                {
                    accessRules.Add(new FileSystemAccessRule(user, FileSystemRights.Read | FileSystemRights.Delete, AccessControlType.Allow));
                }
            }

            return accessRules;
        }

        /// <summary>
        /// Add specifiled FileSystemAccessRule to file's ACL
        /// </summary>
        /// <param name="filePath">target file</param>
        /// <param name="accessRules">access rules to be added</param>
        private static void AddAccessRules(string filePath, List<FileSystemAccessRule> accessRules)
        {
            FileSecurity fileSecurity = File.GetAccessControl(filePath);
            foreach (FileSystemAccessRule accessRule in accessRules)
            {
                fileSecurity.AddAccessRule(accessRule);
            }

            File.SetAccessControl(filePath, fileSecurity);
        }

        /// <summary>
        /// Check if 2 DirectorySecurity objects are the same
        /// </summary>
        /// <param name="ds1">the first DirectorySecurity object</param>
        /// <param name="ds2">the second DirectorySecurity object</param>
        /// <returns>true if the 2 objects are the same</returns>
        private static bool AreSameDirectorySecurityObjects(DirectorySecurity ds1, DirectorySecurity ds2)
        {
            if (ds1.AreAccessRulesProtected != ds2.AreAccessRulesProtected)
            {
                return false;
            }

            AuthorizationRuleCollection arc1 = ds1.GetAccessRules(true, true, typeof(NTAccount));
            AuthorizationRuleCollection arc2 = ds2.GetAccessRules(true, true, typeof(NTAccount));
            if (arc1.Count != arc2.Count)
            {
                return false;
            }

            for (int i = 0; i < arc1.Count; i++)
            {
                FileSystemAccessRule fsar = arc1[i] as FileSystemAccessRule;
                FileSystemAccessRule fsar2 = arc2[i] as FileSystemAccessRule;
                if (fsar.AccessControlType != fsar2.AccessControlType ||
                       fsar.FileSystemRights != fsar2.FileSystemRights ||
                       !fsar.IdentityReference.Equals(fsar2.IdentityReference) ||
                       fsar.InheritanceFlags != fsar2.InheritanceFlags ||
                       fsar.IsInherited != fsar2.IsInherited ||
                       fsar.PropagationFlags != fsar2.PropagationFlags)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Helper function that hash a string into an integer from 0 to MaxSubDirectoryCount
        /// </summary>
        /// <param name="str">string to be hashed</param>
        /// <returns>hash value</returns>
        private static int HashString(string str)
        {
            // A simple string hashing function shall be enough
            int hashValue = 0;
            for (int i = 0; i < str.Length; i++)
            {
                hashValue = (hashValue * 37) + str[i];
            }

            hashValue = Math.Abs(hashValue);

            return hashValue % MaxSubDirectoryCount;
        }

        /// <summary>
        /// Get attributes dictionary
        /// </summary>
        /// <param name="attributeFilePath">attribute file path</param>
        /// <returns>attributes dictionary</returns>
        private static Dictionary<string, string> GetAttributesDictionary(string attributeFilePath)
        {
            int error;
            while (true)
            {
                SafeFileHandle handle = FileNativeMethods.CreateFile(
                    attributeFilePath,
                    (uint)FileNativeMethods.NativeFileAccess.GENERIC_READ,
                    (uint)FileNativeMethods.NativeFileShare.FILE_SHARE_DELETE | (uint)FileNativeMethods.NativeFileShare.FILE_SHARE_READ,
                    IntPtr.Zero,
                    (uint)FileNativeMethods.NativeFileMode.OPEN_EXISTING,
                    0,
                    IntPtr.Zero);

                if (handle.IsInvalid)
                {
                    error = Marshal.GetLastWin32Error();
                    break;
                }

                byte[] attributesBytes = new byte[MaxAttributeStreamLength];
                uint readBytes = 0;
                bool ret = FileNativeMethods.ReadFile(handle, attributesBytes, MaxAttributeStreamLength, ref readBytes, IntPtr.Zero);
                if (!ret)
                {
                    error = Marshal.GetLastWin32Error();
                    handle.Close();
                    break;
                }

                handle.Close();
                return DeserializeAttributesDictionary(attributesBytes, 0, (int)readBytes);
            }

            if (error == FileNativeMethods.ErrorFileNotFound)
            {
                // if attribute file doesn't exist, return null
                return new Dictionary<string, string>();
            }

            Win32Exception exception = new Win32Exception(error);
            throw new DataException(MapIOErrorCodeToDataErrorCode(error), exception);
        }

        /// <summary>
        /// Serialize an attributes dictionary
        /// </summary>
        /// <param name="attributesDic">attributes dictionary</param>
        /// <returns>byte array representation of attributes dictionary</returns>
        private static byte[] SerializeAttributesDictionary(Dictionary<string, string> attributesDic)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, attributesDic);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Deserialize attributes dictionary
        /// </summary>
        /// <param name="attributesBytes">the byte array from which to deserialize the attributes dictionary</param>
        /// <param name="index">The index into buffer at which the deserialization begins.</param>
        /// <param name="count">length of the bytes to be deserialized</param>
        /// <returns>attribute name and value pairs in a dictionary</returns>
        private static Dictionary<string, string> DeserializeAttributesDictionary(byte[] attributesBytes, int index, int count)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(attributesBytes, index, count))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    return (Dictionary<string, string>)formatter.Deserialize(ms);
                }
            }
            catch (Exception ex)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[FileShareDataProvider].DeserializeAttributesDictionary throw exception={0}.", ex);
                return null;
            }
        }

        /// <summary>
        /// Generate container data file path for the specified data container.
        /// </summary>
        /// <param name="name">data container name</param>
        /// <returns>container data file path </returns>
        internal string GenerateContainerFilePath(string name)
        {
            // to avoid too many files under the file directory,  a second level directory is created
            string path = Path.Combine(this.containerRootDirPath, HashString(name).ToString());
            return Path.Combine(path, name);
        }
    }
}
#endif