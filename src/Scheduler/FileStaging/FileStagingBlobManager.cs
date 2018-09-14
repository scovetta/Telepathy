//--------------------------------------------------------------------------
// <copyright file="FileStagingBlobManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     This class is in charge of managing access to the intermediate Azure blob
//     storage for File Staging service.
// </summary>
//--------------------------------------------------------------------------

namespace Microsoft.Hpc.Azure.FileStaging
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Net;
    using System.Security.Cryptography;
    using System.ServiceModel;
    using System.Text;
    using Microsoft.Hpc.Azure.Common;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;
    using Microsoft.WindowsAzure.Storage.Shared.Protocol;

    /// <summary>
    /// This class manages access to the intermediate Azure blob storage for File Staging service.
    /// </summary>
    internal class FileStagingBlobManager
    {
        /// <summary>
        /// User id format that uniquely identifies a user: "ClusterName-DeploymentId-UserName"
        /// </summary>
        private const string UniqueUserIdFormat = @"{0}-{1}-{2}";

        /// <summary>
        /// Naming format of user's container: "HpcFileStaging-MD5HashOf(UserId)"
        /// </summary>
        private const string UserContainerNameFormat = @"hpcfilestaging-{0}";

        /// <summary>
        /// Deployment id for on-premise cluster
        /// </summary>
        private const string OnPremiseClusterDeploymentId = "00000000-0000-0000-0000-000000000000";

        /// <summary>
        /// Default blob level shared access signature expires in 1 hour (actually 55 minutes)
        /// </summary>
        private const int DefaultBlobSASExpirationPeriodInMinutes = 60;

        /// <summary>
        /// Default container level shared access signature expires in 5 ~ 10 days
        /// </summary>
        private const int DefaultContainerSASExpirationPeriodInMinutes = 24 * 5 * 60;

        /// <summary>
        /// Blob level shared access signature expiration period
        /// </summary>
        private static int blobSASExpirationPeriodInMinutes = DefaultBlobSASExpirationPeriodInMinutes;

        /// <summary>
        /// Container level shared access signature expiration period
        /// </summary>
        private static int containerSASExpirationPeriodInMinutes = DefaultContainerSASExpirationPeriodInMinutes;

        /// <summary>
        /// Default RetryPolicy for accessing Azure blob storage: RetryCount = 3, deltaBackoff=3 seconds
        /// </summary>
        private static ExponentialRetry defaultRetryPolicy = new ExponentialRetry();

        /// <summary>
        /// Default Timeout: 1 minute.
        /// </summary>
        private static TimeSpan defaultTimeout = TimeSpan.FromSeconds(60);

        /// <summary>
        /// Default BlobRequestOptions 
        /// </summary>
        private static BlobRequestOptions defaultBlobRequestOptions = new BlobRequestOptions() { RetryPolicy = defaultRetryPolicy, ServerTimeout = defaultTimeout };

        /// <summary>
        /// Base address of the intermediate blob storage
        /// </summary>
        private string dataBlobBaseAddress;

        /// <summary>
        /// StorageCredentials for accessing the intermediate blob storage
        /// </summary>
        private StorageCredentials dataStorageCredentials;

        /// <summary>
        /// Container shared access policies cache
        /// </summary>
        private Dictionary<string, ContainerPolicies> containerPoliciesCache = new Dictionary<string, ContainerPolicies>();

        /// <summary>
        /// Lock object for containerPoliciesCache
        /// </summary>
        private object lockContainerPoliciesCache = new object();

        /// <summary>
        /// Lock object for this instance
        /// </summary>
        private object lockInstance = new object();

        /// <summary>
        /// Initializes static members of the FileStagingBlobManager class:
        /// BlobSASExpirationPeriodInMinutes, ContainerSASExpirationPeriodInMinutes
        /// </summary>
        static FileStagingBlobManager()
        {  
            // apply configured BlobSASExpirationPeriodInMinutes.
            string strBlobSASExpirationPeriod = ConfigurationManager.AppSettings["FileStagingBlobSASExpirationPeriodInMinutes"];
            if (!String.IsNullOrEmpty(strBlobSASExpirationPeriod))
            {
                LocalProxyTraceHelper.TraceError(
                    "FileStagingBlobManager constructor. FileStagingBlobSASExpirationPeriodInMinutes = {0}",
                    strBlobSASExpirationPeriod);

                int temp;
                if (int.TryParse(strBlobSASExpirationPeriod, out temp) && temp > 0)
                {
                    blobSASExpirationPeriodInMinutes = temp;
                    LocalProxyTraceHelper.TraceError(
                        "FileStagingBlobManager constructor. BlobSASExpirationPeriodInMinutes set to {0}",
                        blobSASExpirationPeriodInMinutes);
                }
            }

            // apply configured ContainerSASExpirationPeriodInMinutes
            string strContainerSASExpirationPeriod = ConfigurationManager.AppSettings["FileStagingContainerSASExpirationPeriodInMinutes"];
            if (!String.IsNullOrEmpty(strContainerSASExpirationPeriod))
            {
                LocalProxyTraceHelper.TraceError(
                    "FileStagingBlobManager constructor. FileStagingContainerSASExpirationPeriodInMinutes = {0}",
                    strContainerSASExpirationPeriod);

                int temp;
                if (int.TryParse(strContainerSASExpirationPeriod, out temp) && temp > 0)
                {
                    containerSASExpirationPeriodInMinutes = temp;
                    LocalProxyTraceHelper.TraceError(
                        "FileStagingBlobManager constructor. ContainerSASExpirationPeriodInMinutes set to {0}",
                        containerSASExpirationPeriodInMinutes);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the FileStagingBlobManager class
        /// </summary>
        /// <param name="storageAccount">CloudStorageAccount for accessing the blob service</param>
        public FileStagingBlobManager(CloudStorageAccount storageAccount)
        {
            Debug.Assert(storageAccount != null, "storage account");

            this.dataBlobBaseAddress = storageAccount.BlobEndpoint.AbsoluteUri;
            this.dataStorageCredentials = storageAccount.Credentials;
        }

        /// <summary>
        /// Sets CloudStorageAccount
        /// </summary>
        public CloudStorageAccount StorageAccount
        {
            set
            {
                Debug.Assert(value != null, "storage account");

                lock (this.lockInstance)
                {
                    this.dataBlobBaseAddress = value.BlobEndpoint.AbsoluteUri;
                    this.dataStorageCredentials = value.Credentials;
                }
            }
        }

        /// <summary>
        /// Gets cluster deployment id
        /// </summary>
        private static string DeploymentId
        {
            get
            {
                if (!FileStagingCommon.SchedulerOnAzure)
                {
                    return OnPremiseClusterDeploymentId;
                }
                else
                {
                    return RoleEnvironment.GetConfigurationSettingValue(SchedulerConfigNames.DeploymentId);
                }
            }
        }

        /// <summary>
        /// Get the URL of user's container and an SAS for accessing the container
        /// </summary>
        /// <param name="userName">target user name</param>
        /// <param name="sas">Azure SAS for accessing user's container</param>
        /// <returns>URL of user's container</returns>
        public string GetContainerUrlForUser(string userName, string clusterName, out string sas)
        {
            CloudBlobClient blobClient = this.GetBlobClient();
            string containerName = GetContainerNameForUser(userName, clusterName);
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            try
            {
                // make sure container exists
                bool newCreated = CreateContainerIfNotExist(container);

                // generate sas for accessing the container
                sas = this.GenerateContainerLevelSharedAccessSignature(container, newCreated);

                LocalProxyTraceHelper.TraceVerbose("Got Container Url for user {0}: {1}",userName, container.Uri);

                // return
                return container.Uri.ToString();
            }
            catch (StorageException ex)
            {
                LocalProxyTraceHelper.TraceError(
                    ex,
                    "GetContainerUrlForUser receives StorageException. user={0}, container={1}",
                    userName,
                    containerName);

                throw GenerateExceptionOnStorageErrorCode(ex);
            }
            catch (Exception ex)
            {
                LocalProxyTraceHelper.TraceError(
                    ex,
                   "GetContainerUrlForUser receives Exception. user={0}, container={1}",
                    userName,
                    containerName);

                throw new FaultException<InternalFaultDetail>(new InternalFaultDetail(Resources.Common_InternalServerError, FileStagingErrorCode.InternalServerError));
            }
        }

        /// <summary>
        /// Generates an Azure SAS that grants specified permissions to a blob under user's container
        /// </summary>
        /// <param name="userName">target user name</param>
        /// <param name="blobName">target blob name</param>
        /// <param name="permissions">shared access permissions</param>
        /// <returns>Azure SAS that grants specified permission to the specified blob</returns>
        public string GenerateSASForBlob(string userName, string blobName, string clusterName, SharedAccessBlobPermissions permissions)
        {
            CloudBlobClient blobClient = this.GetBlobClient();
            string containerName = GetContainerNameForUser(userName, clusterName);

            try
            {
                LocalProxyTraceHelper.TraceVerbose("GenerateSASForBlob, user={0}, blob={1}, container={2}, permission={3}",
                    userName, blobName, containerName, permissions);
                CloudBlobContainer container = blobClient.GetContainerReference(containerName);
                ICloudBlob blob = container.GetBlobReferenceFromServer(blobName);
                return blob.GetSharedAccessSignature(GenerateBlobLevelSharedAccessBlobPolicy(permissions));
            }
            catch (StorageException ex)
            {
                LocalProxyTraceHelper.TraceError(
                    ex,
                    "GenerateSASForBlob receives StorageException. user={0}, container={1}",
                    userName,
                    containerName);

                throw GenerateExceptionOnStorageErrorCode(ex);
            }
            catch (Exception ex)
            {
                LocalProxyTraceHelper.TraceError(
                    ex,
                   "GenerateSASForBlob receives Exception. user={0}, container={1}",
                    userName,
                    containerName);

                throw new FaultException<InternalFaultDetail>(new InternalFaultDetail(Resources.Common_InternalServerError, FileStagingErrorCode.InternalServerError));
            }
        }

        /// <summary>
        /// Get container name for the specified user
        /// </summary>
        /// <param name="userName">target user name</param>
        /// <returns>container name for the specified user</returns>
        private static string GetContainerNameForUser(string userName, string clusterName)
        {
            string uniqueUserId = string.Format(UniqueUserIdFormat, clusterName, DeploymentId, userName);

            // user id might exceed maximum blob container name length.
            // so md5 of user id is used as container name instead.
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] userIdBytes = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(uniqueUserId));

                StringBuilder strBuilder = new StringBuilder();
                for (int i = 0; i < userIdBytes.Length; i++)
                {
                    strBuilder.Append(userIdBytes[i].ToString("x2"));
                }

                // Return the hexadecimal representation of user id md5
                string containerName = string.Format(UserContainerNameFormat, strBuilder.ToString());
                LocalProxyTraceHelper.TraceVerbose(
                    "GetContainerNameForUser: user={0}, unique user id={1}, container={2}",
                    userName,
                    uniqueUserId,
                    containerName);

                return containerName;
            }
        }

        /// <summary>
        /// Generate blob-level shared access policy with specified permissions.
        /// </summary>
        /// <param name="permissions">permissions to be granted</param>
        /// <returns>shared access policy that grant specified permissions</returns>
        private static SharedAccessBlobPolicy GenerateBlobLevelSharedAccessBlobPolicy(SharedAccessBlobPermissions permissions)
        {
            SharedAccessBlobPolicy policy = new SharedAccessBlobPolicy();
            policy.SharedAccessStartTime = DateTime.Now.Subtract(new TimeSpan(0, 5, 0));
            policy.SharedAccessExpiryTime = policy.SharedAccessStartTime.Value.AddMinutes(blobSASExpirationPeriodInMinutes);
            policy.Permissions = permissions;
            return policy;
        }

        /// <summary>
        /// Generate container-level shared access policy
        /// </summary>
        /// <returns>container level shared access policy</returns>
        private static SharedAccessBlobPolicy GenerateContainerLevelSharedAccessBlobPolicy()
        {
            SharedAccessBlobPolicy policy = new SharedAccessBlobPolicy();
            policy.SharedAccessStartTime = DateTime.Now.Subtract(new TimeSpan(0, 5, 0));

            // expiration period is set to ContainerSASExpirationPeriodInMinutes*2.
            // Note: this shared access policy will be reused in
            // ContainerSASExpirationPeriodInMinutes, so SAS generated using
            // this policy has experiry period between ContainerSASExpirationPeriodInMinutes
            // and ContainerSASExpirationPeriodInMinutes*2
            policy.SharedAccessExpiryTime = policy.SharedAccessStartTime.Value.AddMinutes(containerSASExpirationPeriodInMinutes * 2);

            // container-level shared access policy grants full control access
            policy.Permissions = SharedAccessBlobPermissions.Read |
                                 SharedAccessBlobPermissions.Write |
                                 SharedAccessBlobPermissions.Delete |
                                 SharedAccessBlobPermissions.List;
            return policy;
        }

        /// <summary>
        /// Create container if container doesn't exist
        /// </summary>
        /// <param name="container">reference to target container</param>
        /// <returns>true if container didn't exist and was created, false otherwise</returns>
        private static bool CreateContainerIfNotExist(CloudBlobContainer container)
        {
            try
            {
                return container.CreateIfNotExists(defaultBlobRequestOptions);
            }
            catch (Exception ex)
            {
                LocalProxyTraceHelper.TraceError(ex, "Exception thrown when creating container {0}", container.Name);
                throw;
            }
        }

        /// <summary>
        /// Get shared access policies of a container
        /// </summary>
        /// <param name="container">target container</param>
        /// <returns>container's shared access policies</returns>
        private static SharedAccessBlobPolicies GetContainerPolicies(CloudBlobContainer container)
        {
            try
            {
                return container.GetPermissions(null, defaultBlobRequestOptions, null).SharedAccessPolicies;
            }
            catch (Exception ex)
            {
                LocalProxyTraceHelper.TraceError(ex, "Exception thrown when retrieving SharedAccessPermission of container {0}", container.Name);
                throw;
            }
        }

        /// <summary>
        /// Update shared access policies for a container
        /// </summary>
        /// <param name="container">target container</param>
        /// <param name="policies">new shared access policies</param>
        private static void UpdateContainerPolicies(CloudBlobContainer container, ContainerPolicies policies)
        {
            BlobContainerPermissions permissions = new BlobContainerPermissions();
            permissions.PublicAccess = BlobContainerPublicAccessType.Off;
            foreach (KeyValuePair<string, SharedAccessBlobPolicy> policy in policies.Policies)
            {
                permissions.SharedAccessPolicies.Add(policy.Key, policy.Value);
            }

            try
            {
                container.SetPermissions(permissions, null, defaultBlobRequestOptions);
            }
            catch (Exception ex)
            {
                LocalProxyTraceHelper.TraceError(
                    ex,
                    "Exception thrown when setting SharedAccessPermission of container {0}",
                    container.Name);

                throw;
            }
        }

        /// <summary>
        /// Check if a shared access policy is expired.
        /// </summary>
        /// <param name="policy">shared access policy</param>
        /// <returns>true if the shared access policy is expired, false otherwise</returns>
        /// <remarks>
        /// a shared access policy becomes expired after SharedAccessExpiryTime.
        /// </remarks>
        private static bool IsPolicyExpired(SharedAccessBlobPolicy policy)
        {
            return DateTime.Now.CompareTo(policy.SharedAccessExpiryTime) >= 0;
        }

        /// <summary>
        /// Check if a shared access policy is stale.
        /// </summary>
        /// <param name="policy">shared access policy</param>
        /// <returns>true if the shared access policy is stale, false otherwise</returns>
        /// <remarks>
        /// a shared access policy is considered as "stale" if there is no
        /// more than ContainerSASExpirationPeriodInHours(5 days) to its
        /// SharedAccessExpiryTime.
        /// </remarks>
        private static bool IsPolicyStale(SharedAccessBlobPolicy policy)
        {
            return DateTime.Now.AddMinutes(containerSASExpirationPeriodInMinutes).CompareTo(policy.SharedAccessExpiryTime) >= 0;
        }

        /// <summary>
        /// Create and add a container level shared access policy to the
        /// specified SharedAccessBlobPolicies
        /// </summary>
        /// <param name="policies">target SharedAccessBlobPolicies</param>
        /// <returns>name of the added shared access policy</returns>
        private static string AddNewContainerPolicy(ContainerPolicies policies)
        {
            string policyName = Guid.NewGuid().ToString();
            policies.Policies.Add(policyName, GenerateContainerLevelSharedAccessBlobPolicy());
            return policyName;
        }

        /// <summary>
        /// Find an active container level share access policy from the
        /// specified SharedAccessBlobPolicies
        /// </summary>
        /// <param name="policies">target SharedAccessBlobPolicies</param>
        /// <returns>
        /// name of an active shared access policy on success, or string.Empty
        /// on failure
        /// </returns>
        private static string FindActiveContainerPolicy(ContainerPolicies policies)
        {
            foreach (KeyValuePair<string, SharedAccessBlobPolicy> policy in policies.Policies)
            {
                if (!IsPolicyStale(policy.Value))
                {
                    return policy.Key;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Remove expired shared access policies from the specified
        /// SharedAccessBlobPolicies
        /// </summary>
        /// <param name="policies">target SharedAccessBlobPolicies</param>
        private static void EvictExpiredContainerPolicies(ContainerPolicies policies)
        {
            List<string> expiredPolicies = new List<string>();
            foreach (KeyValuePair<string, SharedAccessBlobPolicy> policy in policies.Policies)
            {
                if (IsPolicyExpired(policy.Value))
                {
                    expiredPolicies.Add(policy.Key);
                }
            }

            foreach (string expiredPolicy in expiredPolicies)
            {
                policies.Policies.Remove(expiredPolicy);
            }
        }

        /// <summary>
        /// Generate FaultException for various StorageErrorCode
        /// </summary>
        /// <param name="errorCode">storage error code</param>
        /// <returns>corresponding FaultException</returns>
        private static FaultException<InternalFaultDetail> GenerateExceptionOnStorageErrorCode(StorageException e)
        {
            if (e.RequestInformation == null)
            {
                return new FaultException<InternalFaultDetail>(new InternalFaultDetail(Resources.Common_InternalServerError, FileStagingErrorCode.UnknownFault));
            }

            if (e.RequestInformation.HttpStatusCode == (int)HttpStatusCode.BadGateway ||
                e.RequestInformation.HttpStatusCode == (int)HttpStatusCode.BadRequest)
            {
                return new FaultException<InternalFaultDetail>(
                    new InternalFaultDetail(Resources.Common_IntermediateBlobStorageCommunicationFailure, FileStagingErrorCode.CommunicationFailure)); 
            }

            string errorCode = string.Empty;
            if (e.RequestInformation.ExtendedErrorInformation != null)
            {
                errorCode = e.RequestInformation.ExtendedErrorInformation.ErrorCode;
            }

            if (errorCode.Equals(StorageErrorCodeStrings.AuthenticationFailed, StringComparison.OrdinalIgnoreCase))
            {
                return new FaultException<InternalFaultDetail>(
                    new InternalFaultDetail(Resources.Common_IntermediateBlobStorageMisConfigured, FileStagingErrorCode.IntermediateBlobStorageMisConfigured));
            }
            else if (errorCode.Equals(StorageErrorCodeStrings.OperationTimedOut, StringComparison.OrdinalIgnoreCase) ||
                errorCode.Equals(StorageErrorCodeStrings.ServerBusy, StringComparison.OrdinalIgnoreCase))
            {
                return new FaultException<InternalFaultDetail>(
                    new InternalFaultDetail(Resources.Common_IntermediateBlobStorageCommunicationFailure, FileStagingErrorCode.CommunicationFailure));
            }

            return new FaultException<InternalFaultDetail>(
                    new InternalFaultDetail(Resources.Common_InternalServerError, FileStagingErrorCode.InternalServerError));
        }

        /// <summary>
        /// Generate shared access signature that grants full control access 
        /// (read/write/delete/list) to the specified container.
        /// </summary>
        /// <param name="container">target container</param>
        /// <param name="isNewCreated">if target container is newly created</param>
        /// <returns>SAS that grants full control access to the specified container</returns>
        private string GenerateContainerLevelSharedAccessSignature(CloudBlobContainer container, bool isNewCreated)
        {
            // steps:
            // 1. get shared access policies for the container
            // 2. select an active shared access policy from all policies
            // 2.1 if not found, create a new shared acess policy for the container
            // 3. create a shared access signature using the selected policy
            bool hitCache;
            ContainerPolicies containerPolicies = this.GetContainerPoliciesFromCache(container.Name, out hitCache);

            // for each container, serialize operations on it
            lock (containerPolicies)
            {
                // for existing container, if cache miss, then get its policies from Azure storage
                if (!isNewCreated && !hitCache)
                {
                    SharedAccessBlobPolicies policies = GetContainerPolicies(container);
                    foreach (KeyValuePair<string, SharedAccessBlobPolicy> policy in policies)
                    {
                        containerPolicies.Policies.Add(policy.Key, policy.Value);
                    }
                }

                // try to select an active shared access policy from all policies
                string policyName = FindActiveContainerPolicy(containerPolicies);
                if (string.IsNullOrEmpty(policyName))
                {
                    // if no active shared access policy is availabe
                    // step 1. remove expired policies
                    // step 2. add a new policy
                    // step 3. update container's policies with the new one
                    EvictExpiredContainerPolicies(containerPolicies);
                    policyName = AddNewContainerPolicy(containerPolicies);

                    UpdateContainerPolicies(container, containerPolicies);
                    this.UpdateContainerPoliciesInCache(container.Name, containerPolicies);
                }

                // return sas
                return container.GetSharedAccessSignature(new SharedAccessBlobPolicy(), policyName);
            }  
        }

        /// <summary>
        /// Get a CloudBlobClient instance for accessing the intermediate blob storage
        /// </summary>
        /// <returns>A CloudBlobClient instance for accessing the intermediate blob storage</returns>
        private CloudBlobClient GetBlobClient()
        {
            lock (this.lockInstance)
            {
                return new CloudBlobClient(new Uri(this.dataBlobBaseAddress), this.dataStorageCredentials);
            }
        }

        /// <summary>
        /// Get shared access policies for the specified container from cache,
        /// or create new shared access policies for the specified container and
        /// add it into cache
        /// </summary>
        /// <param name="containerName">target container name</param>
        /// <param name="hitCache">true if hit cache, false otherwise</param>
        /// <returns>shared access policies for the specified container</returns>
        private ContainerPolicies GetContainerPoliciesFromCache(string containerName, out bool hitCache)
        {
            hitCache = true;
            ContainerPolicies policies;
            lock (this.lockContainerPoliciesCache)
            {
                if (!this.containerPoliciesCache.TryGetValue(containerName, out policies))
                {
                    policies = new ContainerPolicies();
                    policies.Policies = new SharedAccessBlobPolicies();
                    this.containerPoliciesCache.Add(containerName, policies);
                    hitCache = false;
                }

                return policies;
            }
        }

        /// <summary>
        /// Update shared access policies for the specified container in cache
        /// </summary>
        /// <param name="containerName">container name</param>
        /// <param name="policies">new shared access policies</param>
        private void UpdateContainerPoliciesInCache(string containerName, ContainerPolicies policies)
        {
            lock (this.lockContainerPoliciesCache)
            {
                this.containerPoliciesCache[containerName] = policies;
            }
        }

        /// <summary>
        /// Container policies wrapper
        /// </summary>
        private class ContainerPolicies
        {
            /// <summary>
            /// Gets or sets internal shared access policies
            /// </summary>
            public SharedAccessBlobPolicies Policies
            {
                get;
                set;
            }
        }
    }
}
