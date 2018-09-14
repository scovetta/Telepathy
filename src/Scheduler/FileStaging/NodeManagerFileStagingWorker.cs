//--------------------------------------------------------------------------
// <copyright file="NodeManagerFileStagingWorker.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     This implementation of the worker runs in the node manager on
//     compute nodes in the on-premise cluster. It allows clients to stream
//     file contents to and from compute nodes in the same way as is
//     possible on Azure nodes.
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
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using Microsoft.Hpc.Azure.DataMovement;
    using Microsoft.Hpc.Azure.FileStaging.Client;

    /// <summary>
    /// NodeManagerFileStagingWorker runs a web service that implements
    /// IFileStaging interfaces for on-premise nocde. It runs on on-premise
    /// node.
    /// </summary>
    public class NodeManagerFileStagingWorker : FileStagingWorker
    {
        /// <summary>
        /// Starts the service and listens for clients
        /// </summary>
        public override void Start()
        {
            this.Trace(TraceLevel.Information, "Start NodeManagerFileStagingWorker.");
            Uri listenUri = FileStagingCommon.GetFileStagingEndpoint().Uri;
            this.WorkerServiceHost = new ServiceHost(this);

#if DEBUG
            // Reconstruct service host with a base address that allows us to publish metadata
            this.WorkerServiceHost = new ServiceHost(this, new Uri("http://localhost:8081/FileStagingMetadata"));

            // Check to see if the service host already has a ServiceMetadataBehavior
            ServiceMetadataBehavior smb = this.WorkerServiceHost.Description.Behaviors.Find<ServiceMetadataBehavior>();

            // If not, add one
            if (smb == null)
            {
                smb = new ServiceMetadataBehavior();
            }

            smb.HttpGetEnabled = true;
            smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
            this.WorkerServiceHost.Description.Behaviors.Add(smb);

            // Add MEX endpoint
            this.WorkerServiceHost.AddServiceEndpoint(
              ServiceMetadataBehavior.MexContractName,
              MetadataExchangeBindings.CreateMexHttpBinding(),
              "mex");
#endif

            try
            {
                this.WorkerServiceHost.AddServiceEndpoint(typeof(IFileStaging), FileStagingCommon.GetSecureFileStagingBinding(), listenUri);
                this.WorkerServiceHost.Open();

                this.Trace(TraceLevel.Information, "Node module is listening on {0}.", listenUri.ToString());
            }
            catch (Exception ex)
            {
                this.Trace(TraceLevel.Critical, ex, "Exception thrown while trying to create the node file staging service on {0}.", listenUri.ToString());
                throw;
            }
        }

        #region Implementation of the service contract with impersonation

        /// <summary>
        /// Read contents of the target file
        /// </summary>
        /// <returns>a stream that can be used to read contents of the target file</returns>
        public override System.IO.Stream ReadFile()
        {
            using (WindowsImpersonationContext context = this.ImpersonateUsingHeaders())
            {
                try
                {
                    return base.ReadFile();
                }
                finally
                {
                    context.Undo();
                }
            }
        }

        /// <summary>
        /// Write contents in a stream to the target file
        /// </summary>
        /// <param name="contents">stream to be written</param>
        public override void WriteFile(System.IO.Stream contents)
        {
            using (WindowsImpersonationContext context = this.ImpersonateUsingHeaders())
            {
                try
                {
                    base.WriteFile(contents);
                }
                finally
                {
                    context.Undo();
                }
            }
        }

        /// <summary>
        /// Delete the target file
        /// </summary>
        public override void DeleteFile()
        {
            using (WindowsImpersonationContext context = this.ImpersonateUsingHeaders())
            {
                try
                {
                    base.DeleteFile();
                }
                finally
                {
                    context.Undo();
                }
            }
        }

        /// <summary>
        /// Returns an array of directories that matches specified search pattern and search option under target directory
        /// </summary>
        /// <param name="searchPattern">search pattern</param>
        /// <param name="searchOption">search option</param>
        /// <returns>an array of directories that matches specified search pattern and search option under target directory</returns>
        public override ClusterDirectoryInfo[] GetDirectories(string searchPattern, System.IO.SearchOption searchOption)
        {
            using (WindowsImpersonationContext context = this.ImpersonateUsingHeaders())
            {
                try
                {
                    return base.GetDirectories(searchPattern, searchOption);
                }
                finally
                {
                    context.Undo();
                }
            }
        }

        /// <summary>
        /// Delete the target directory
        /// </summary>
        /// <param name="recursive">if delete the directory recursively. if set to false, only empty directory can be deleted</param>
        public override void DeleteDirectory(bool recursive)
        {
            using (WindowsImpersonationContext context = this.ImpersonateUsingHeaders())
            {
                try
                {
                    base.DeleteDirectory(recursive);
                }
                finally
                {
                    context.Undo();
                }
            }
        }

        /// <summary>
        /// Returns an array of files that matches specified search pattern and search option under target directory
        /// </summary>
        /// <param name="searchPattern">search pattern</param>
        /// <param name="searchOption">search option</param>
        /// <returns>an array of files that matches specified search pattern and search option under target directory</returns>
        public override ClusterFileInfo[] GetFiles(string searchPattern, System.IO.SearchOption searchOption)
        {
            using (WindowsImpersonationContext context = this.ImpersonateUsingHeaders())
            {
                try
                {
                    return base.GetFiles(searchPattern, searchOption);
                }
                finally
                {
                    context.Undo();
                }
            }
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
            // Note: Duplicating a impersonation token created via S4U will throw exception
            // "Invalid token for impersonation - it cannot be duplicated".
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
        /// Creates a channel to the scheduler proxy
        /// </summary>
        /// <returns>a channel to the scheduler proxy</returns>
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
                    LocalWorkerTraceHelper.TraceCritical(format, args);
                    break;
                case TraceLevel.Error:
                    LocalWorkerTraceHelper.TraceError(format, args);
                    break;
                case TraceLevel.Warning:
                    LocalWorkerTraceHelper.TraceWarning(format, args);
                    break;
                case TraceLevel.Information:
                    LocalWorkerTraceHelper.TraceInformation(format, args);
                    break;
                case TraceLevel.Verbose:
                    LocalWorkerTraceHelper.TraceVerbose(format, args);
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
            switch (level)
            {
                case TraceLevel.Critical:
                    LocalWorkerTraceHelper.TraceCritical(ex, format, args);
                    break;
                case TraceLevel.Error:
                    LocalWorkerTraceHelper.TraceError(ex, format, args);
                    break;
                case TraceLevel.Warning:
                    LocalWorkerTraceHelper.TraceWarning(ex, format, args);
                    break;
                case TraceLevel.Information:
                    LocalWorkerTraceHelper.TraceInformation(ex, format, args);
                    break;
                case TraceLevel.Verbose:
                    LocalWorkerTraceHelper.TraceVerbose(ex, format, args);
                    break;
            }
        }

        /// <summary>
        /// Impersonate the user specified in message header
        /// </summary>
        /// <returns>a WindowsImpersoateContext object that represents the
        /// Windows user prior to impersonation</returns>
        private WindowsImpersonationContext ImpersonateUsingHeaders()
        {
            string userSDDL;
            string userDomainName, userUserName, callerDomainName, callerUserName;

            try
            {
                // Get the user from headers
                userSDDL = GetInputFromHeader<string>(FileStagingCommon.WcfHeaderUserSddl);
                this.Trace(TraceLevel.Verbose, "Found SDDL in headers: {0}", userSDDL);
            }
            catch (Exception ex)
            {
                this.Trace(TraceLevel.Error, ex, "The user's SDDL is missing from the headers.");
                throw new FaultException<InternalFaultDetail>(new InternalFaultDetail(string.Format(Resources.Common_MissingHeaders, FileStagingCommon.WcfHeaderUserSddl), FileStagingErrorCode.AuthenticationFailed));
            }

            try
            {
                // Get the SDDL of the caller, and look up the user and domain names with the SDDLs
                string callerSDDL = ServiceSecurityContext.Current.WindowsIdentity.User.ToString();

                SecurityHelper.GetInfoForSDDL(userSDDL, out userDomainName, out userUserName);
                this.Trace(TraceLevel.Verbose, "SDLL lookup returned domain {0} and user {1} for impersonation.", userDomainName, userUserName);

                SecurityHelper.GetInfoForSDDL(callerSDDL, out callerDomainName, out callerUserName);
                this.Trace(TraceLevel.Verbose, "SDLL lookup returned domain {0} and user {1} for caller.", callerDomainName, callerUserName);
            }
            catch (Exception ex)
            {
                this.Trace(TraceLevel.Error, ex, "Could not parse credentials.");
                throw new FaultException<InternalFaultDetail>(new InternalFaultDetail(Resources.OnPremise_CantParseCredentials, FileStagingErrorCode.AuthenticationFailed));
            }

            // Get the identity of the head node that this node belongs to
            if (!ServiceSecurityContext.Current.WindowsIdentity.IsSystem)
            {
                // The caller is from another computer, so use its domain and the head node hostname found in the
                // CCP_SCHEDULER to build the head node's identity. If CCP_SCHEDULER is not set, then the call to
                // GetEnvironmentVariable() returns null, in which case the generic "only the head node is allowed"
                // exception is thrown (which is inaccurate, but it is also unreasonable to not have the var set).
                string headNode = Environment.GetEnvironmentVariable(HpcConstants.SchedulerEnvironmentVariableName);
                if (string.IsNullOrEmpty(headNode))
                {
                    throw new FaultException<InternalFaultDetail>(new InternalFaultDetail(Resources.OnPremise_OnlyHeadNodeAllowed, FileStagingErrorCode.AuthenticationFailed));
                }

                string headNodeAccount = string.Format(CultureInfo.CurrentCulture, SecurityHelper.computerAccountFormat, headNode);
                string headNodeUpn = string.Format(CultureInfo.CurrentCulture, SecurityHelper.upnFormat, headNodeAccount, callerDomainName);

                this.Trace(TraceLevel.Verbose, "The head node's expected identity is (UPN) {0}.", headNodeUpn);
                using (WindowsIdentity headNodeIdentity = new WindowsIdentity(headNodeUpn))
                {
                    // The caller must be verified as the head node. The SID of the head node must match the caller.
                    if (!headNodeIdentity.User.Equals(ServiceSecurityContext.Current.WindowsIdentity.User))
                    {
                        this.Trace(TraceLevel.Error, "Only the head node may call the service.");
                        throw new FaultException<InternalFaultDetail>(new InternalFaultDetail(Resources.OnPremise_OnlyHeadNodeAllowed, FileStagingErrorCode.AuthenticationFailed));
                    }
                }
            }
            else
            {
                // The caller is from the same machine (as a system account), so just assume the caller is the head node
                this.Trace(TraceLevel.Verbose, "The service was called from the local system account.");
            }

            try
            {
                // Create a UPN for the user and use the S4U constructor for WindowsIdentity to impersonate it
                string upn = string.Format(CultureInfo.CurrentCulture, SecurityHelper.upnFormat, userUserName, userDomainName);
                using (WindowsIdentity windowsId = new WindowsIdentity(upn))
                {
                    this.Trace(TraceLevel.Verbose, "Impersonating identity (UPN) {0}.", upn);
                    return windowsId.Impersonate();
                }
            }
            catch (Exception ex)
            {
                this.Trace(TraceLevel.Error, ex, "Impersonation failed.");
                string message = string.Format(CultureInfo.CurrentCulture, Resources.OnPremise_ImpersonationFailure, userSDDL, ex.Message);
                throw new FaultException<InternalFaultDetail>(new InternalFaultDetail(message, FileStagingErrorCode.AuthenticationFailed));
            }
        }

        /// <summary>
        /// Check if user has specified access rights to a file
        /// </summary>
        /// <param name="userName">target user name</param>
        /// <param name="filePath">target file path</param>
        /// <param name="rights">file access rights</param>
        /// <returns>true if user has specified access rights to the file, false otherwise</returns>
        protected override void CheckFilePermissions(string userName, string filePath, FileSystemRights rights)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return;
                }

                using (PrincipalContext context = new PrincipalContext(ContextType.Domain))
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
    }
}
