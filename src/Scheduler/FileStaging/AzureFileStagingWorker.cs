//--------------------------------------------------------------------------
// <copyright file="AzureFileStagingWorker.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     Runs as part of the proxy worker role in Azure and runs a web service 
//     that streams file contents to or from worker nodes and returns
//     directory information.
// </summary>
//--------------------------------------------------------------------------

namespace Microsoft.Hpc.Azure.FileStaging
{
    using System;
    using System.Collections.Generic;
    using System.DirectoryServices.AccountManagement;
    using System.Globalization;
    using System.IO;
    using System.Security.AccessControl;
    using System.ServiceModel;
    using Microsoft.Hpc.Azure.Common;
    using Microsoft.Hpc.Azure.FileStaging.Client;

    /// <summary>
    /// AzureFileStagingWorker runs on Azure node and runs a web service
    /// that implements IFileStaging interfaces in Azure node.
    /// </summary>
    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any, InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true)]
    public class AzureFileStagingWorker : FileStagingWorker
    {
        /// <summary>
        /// allowed characters for password
        /// </summary>
        private const string AllowedPasswordChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTURWXYZ1234567890~!@#$%^&*()?;";

        /// <summary>
        /// Password suffix. This is to make the password has at least one alphabet, one digital and one other chars
        /// </summary>
        private const string PasswordSuffix = "a1;";

        /// <summary>
        /// Minimum password length
        /// </summary>
        private const int MinPasswordLength = 8;

        /// <summary>
        /// Maximum password length
        /// </summary>
        private const int MaxPasswordLength = 11;

        /// <summary>
        /// lock object for creating local account
        /// </summary>
        private static object createAccountLock = new object();

        /// <summary>
        /// Azure proxy file writer that keeps Azure proxy file up-to-date
        /// </summary>
        private AzureProxyFileWriter azureProxyFileWriter;

        /// <summary>
        /// Starts the service and begins listening for clients.
        /// </summary>
        public override void Start()
        {
            Uri listenUri = FileStagingCommon.GetFileStagingEndpoint().Uri;
            this.WorkerServiceHost = new ServiceHost(this);

            try
            {
                this.WorkerServiceHost.AddServiceEndpoint(typeof(IFileStaging), FileStagingCommon.GetFileStagingBinding(), listenUri, listenUri);
                this.WorkerServiceHost.Open();
                this.Trace(TraceLevel.Information, "Node module is listening on {0}.", listenUri);
            }
            catch (Exception ex)
            {
                this.Trace(TraceLevel.Critical, ex, "Exception thrown while trying to create the node file staging service on {0}.", listenUri);
                throw;
            }

            this.azureProxyFileWriter = new AzureProxyFileWriter();
        }

        #region Implementation of the service contract

        /// <summary>
        /// Read contents of the target file
        /// </summary>
        /// <returns>a stream that can be used to read contents of the target file</returns>
        public override Stream ReadFile()
        {
            this.CheckFilePermissions(this.GetUserName(), this.TargetPath, FileSystemRights.Read);
            return base.ReadFile();
        }

        /// <summary>
        /// Write contents in a stream to the target file
        /// </summary>
        /// <param name="contents">stream to be written</param>
        public override void WriteFile(Stream contents)
        {
            string userName = this.GetUserName();
            this.CheckFilePermissions(userName, this.TargetPath, FileSystemRights.Write);
            base.WriteFile(contents);

            this.GrantFilePermissions(userName, this.TargetPath, FileSystemRights.FullControl);
        }

        /// <summary>
        /// Delete the target file
        /// </summary>
        public override void DeleteFile()
        {
            this.CheckFilePermissions(this.GetUserName(), this.TargetPath, FileSystemRights.Delete);
            base.DeleteFile();
        }

        /// <summary>
        /// Returns an array of directories that matches specified search pattern and search option under target directory
        /// </summary>
        /// <param name="searchPattern">search pattern</param>
        /// <param name="searchOption">search option</param>
        /// <returns>an array of directories that matches specified search pattern and search option under target directory</returns>
        public override ClusterDirectoryInfo[] GetDirectories(string searchPattern, System.IO.SearchOption searchOption)
        {
            this.CheckFilePermissions(this.GetUserName(), this.TargetPath, FileSystemRights.ListDirectory);
            return base.GetDirectories(searchPattern, searchOption);
        }

        /// <summary>
        /// Delete the target directory
        /// </summary>
        /// <param name="recursive">if delete the directory recursively. if set to false, only empty directory can be deleted</param>
        public override void DeleteDirectory(bool recursive)
        {
            FileSystemRights requiredPermission = recursive ? FileSystemRights.DeleteSubdirectoriesAndFiles : FileSystemRights.Delete;
            this.CheckFilePermissions(this.GetUserName(), this.TargetPath, requiredPermission);
            base.DeleteDirectory(recursive);
        }

        /// <summary>
        /// Returns an array of files that matches specified search pattern and search option under target directory
        /// </summary>
        /// <param name="searchPattern">search pattern</param>
        /// <param name="searchOption">search option</param>
        /// <returns>an array of files that matches specified search pattern and search option under target directory</returns>
        public override ClusterFileInfo[] GetFiles(string searchPattern, System.IO.SearchOption searchOption)
        {
            this.CheckFilePermissions(this.GetUserName(), this.TargetPath, FileSystemRights.ListDirectory);
            return base.GetFiles(searchPattern, searchOption);
        }

        /// <summary>
        /// Copy a file from local to an Azure with the specified blob url
        /// </summary>
        /// <param name="blobUrl">destination blob url</param>
        /// <param name="sas">SAS for accessing the blob</param>
        /// <param name="sourceFilePath">source file path</param>
        public override void CopyFileToBlob(string blobUrl, string sas, string sourceFilePath)
        {
            this.CheckFilePermissions(this.GetUserName(), this.TargetPath, FileSystemRights.Read);
            base.CopyFileToBlob(blobUrl, sas, sourceFilePath);
        }

        /// <summary>
        /// Copy a file from Azure blot to local
        /// </summary>
        /// <param name="blobUrl">source blob url</param>
        /// <param name="sas">SAS for accessing the blob</param>
        /// <param name="destFilePath">destination file path</param>
        /// <param name="overwrite">if overwrite existing files</param>
        public override void CopyFileFromBlob(string blobUrl, string sas, string destFilePath, bool overwrite)
        {
            this.CheckFilePermissions(this.GetUserName(), this.TargetPath, FileSystemRights.Modify);
            base.CopyFileFromBlob(blobUrl, sas, destFilePath, overwrite);
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
        public override void CopyDirectoryToBlob(string blobUrlPrefix, string sas, string sourceDir, List<string> filePatterns, bool recursive, bool overwrite)
        {
            base.CopyDirectoryToBlob(blobUrlPrefix, sas, sourceDir, filePatterns, recursive, overwrite);
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
        public override void CopyDirectoryFromBlob(string blobUrlPrefix, string sas, string destDir, List<string> filePatterns, bool recursive, bool overwrite)
        {
            base.CopyDirectoryFromBlob(blobUrlPrefix, sas, destDir, filePatterns, recursive, overwrite);
        }

        #endregion
        
        /// <summary>
        /// Creates a channel to the proxy
        /// </summary>
        /// <returns>a channel to the proxy</returns>
        protected override GenericFileStagingClient CreateProxyChannel()
        {
            // This is not supported in V3 SP1.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Override FileStagingWorker.Trace(TraceLevel, string, params object[])
        /// </summary>
        /// <param name="level">trace level</param>
        /// <param name="format">trace formatting string</param>
        /// <param name="args">trace arguments</param>
        protected override void Trace(TraceLevel level, string format, params object[] args)
        {
            switch (level)
            {
                case TraceLevel.Critical:
                    AzureWorkerTraceHelper.TraceError(format, args);
                    break;
                case TraceLevel.Error:
                    AzureWorkerTraceHelper.TraceError(format, args);
                    break;
                case TraceLevel.Warning:
                    AzureWorkerTraceHelper.TraceWarning(format, args);
                    break;
                case TraceLevel.Information:
                    AzureWorkerTraceHelper.TraceInformation(format, args);
                    break;
                case TraceLevel.Verbose:
                    AzureWorkerTraceHelper.WriteLine(format, args);
                    break;
            }
        }

        /// <summary>
        /// Override FileStagingWorker.Trace(TraceLevel, Exception, string, params object[])
        /// </summary>
        /// <param name="level">trace level</param>
        /// <param name="ex">related exception</param>
        /// <param name="format">trace formatting string</param>
        /// <param name="args">trace arguments</param>     
        protected override void Trace(TraceLevel level, Exception ex, string format, params object[] args)
        {
            try
            {
                // The Azure tracing framework doesn't provide a place to put exception information, so just
                // append it to the end of the string so that we can get the information later.
                string message = string.Format(CultureInfo.CurrentCulture, format, args);
                this.Trace(level, "{0} Exception: {1}", message, ex);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Get local account name of calling user
        /// </summary>
        /// <returns>local account name of corresponding user</returns>
        protected override string GetUserName()
        {
            // user name is not available for scheduler on Azure. so just return an empty string.
            if (FileStagingCommon.SchedulerOnAzure)
            {
                return string.Empty;
            }

            // get domain user name, and convert it to local user name
            string userName = Credentials.ToUniqueLocalAccount(base.GetUserName());
            if (!string.IsNullOrEmpty(userName))
            {
                CreateLocalAccountIfNotExists(userName, this.GetIsAdmin());
            }

            return userName;
        }

        /// <summary>
        /// Check if user has specified access rights to a file
        /// </summary>
        /// <param name="userName">target user name</param>
        /// <param name="filePath">target file path</param>
        /// <param name="rights">file access rights</param>
        protected override void CheckFilePermissions(string userName, string filePath, FileSystemRights rights)
        {
            try
            {
                // no permission check for scheduler on Azure where user name is not available.
                if (string.IsNullOrEmpty(userName))
                {
                    return;
                }

                if (!File.Exists(filePath))
                {
                    return;
                }

                using (PrincipalContext context = new PrincipalContext(ContextType.Machine))
                {
                    using (UserPrincipal principal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, userName))
                    {
                        FilePermission.CheckPermission(principal, filePath, false, rights);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Trace(TraceLevel.Error, ex, "Checking file permission failed: user={0}, file path={1}", userName, filePath);
                throw new FaultException<InternalFaultDetail>(new InternalFaultDetail("access denied", FileStagingErrorCode.AuthenticationFailed, ex));
            }
        }

        /// <summary>
        /// Grant a user specified access rights to a file
        /// </summary>
        /// <param name="userName">target user name</param>
        /// <param name="filePath">target file path</param>
        /// <param name="rights">file access rights</param>
        private void GrantFilePermissions(string userName, string filePath, FileSystemRights rights)
        {
            try
            {
                FileSecurity fileSecurity = File.GetAccessControl(filePath);
                fileSecurity.AddAccessRule(new FileSystemAccessRule(userName, rights, AccessControlType.Allow));
                File.SetAccessControl(filePath, fileSecurity);
            }
            catch (Exception ex)
            {
                this.Trace(TraceLevel.Error, ex, "Grant file permission failed: user={0}, file path={1}", userName, filePath);
                throw new FaultException<InternalFaultDetail>(new InternalFaultDetail(ex.Message, FileStagingErrorCode.TargetIOFailure, ex));
            }
        }

        /// <summary>
        /// Create a local account if it does not exist
        /// </summary>
        /// <param name="account">local account name</param>
        /// <param name="isAdmin">is admin</param>
        private static void CreateLocalAccountIfNotExists(string account, bool isAdmin)
        {
            lock (createAccountLock)
            {
                if (!Credentials.ExistsLocalAccount(account))
                {
                    Credentials.AddLocalAccount(account, GenerateRandomPassword(), isAdmin, false);
                }
            }
        }

        /// <summary>
        /// Generate a random password
        /// </summary>
        /// <returns>random-generated password</returns>
        private static string GenerateRandomPassword()
        {
            Random rd = new Random();
            int passwordLength = rd.Next(MinPasswordLength, MaxPasswordLength + 1);
            char[] password = new char[passwordLength];
            for (int i = 0; i < passwordLength; i++)
            {
                password[i] = AllowedPasswordChars[rd.Next(0, AllowedPasswordChars.Length)];
            }

            string strPassword = new string(password);
            return strPassword + PasswordSuffix;
        }
    }
}
