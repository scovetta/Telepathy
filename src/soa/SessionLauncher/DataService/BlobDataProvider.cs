//------------------------------------------------------------------------------
// <copyright file="BlobDataProvider.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      An Azure blob based data provider implementation
// </summary>
//------------------------------------------------------------------------------
#if HPCPACK

namespace Microsoft.Hpc.Scheduler.Session.Data.DataProvider
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Net;
    using System.Security.Principal;
    using System.Text.RegularExpressions;
    using Microsoft.Hpc.BrokerBurst;
    using Microsoft.Hpc.Scheduler.Session.Data.Internal;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Blob.Protocol;
    using Microsoft.WindowsAzure.Storage.Shared.Protocol;
    using TraceHelper = Microsoft.Hpc.Scheduler.Session.Data.Internal.DataServiceTraceHelper;

    /// <summary>
    /// An Azure blob based data provider implementation
    /// </summary>
    internal class BlobDataProvider : IDataProvider
    {
        /// <summary>
        /// Http prefix
        /// </summary>
        private const string HttpPrefix = "http://";

        /// <summary>
        /// Https prefix
        /// </summary>
        private const string HttpsPrefix = "https://";

        /// <summary>
        /// Naming format of common data container: "hpccommondata-clusterid"
        /// </summary>
        private const string BlobContainerNameFormat = @"hpccommondata-{0}";

        /// <summary>
        /// Blob SAS expiration period configuration
        /// </summary>
        private const string BlobSASExpirationConfig = "BlobSASExpirationPeriodInMinutes";

        /// <summary>
        /// Default blob level shared access signature expires in 5 ~ 10 days
        /// </summary>
        private const int DefaultBlobSASExpirationPeriodInMinutes = 5 * 24 * 60;

        /// <summary>
        /// Maximum number of results to be returned in on ListBlobsSegemented call
        /// </summary>
        private const int ListBlobsSegmentSize = 1000;

        /// <summary>
        /// Max time period for the operation of setting permission.
        /// </summary>
        private const int SetPermissionMaxTimeInMinutes = 1;

        /// <summary>
        /// Blob level shared access signature expiration period
        /// </summary>
        private static int blobSasExpirationPeriodInMinutes = DefaultBlobSASExpirationPeriodInMinutes;

        /// <summary>
        /// Shift blob SAS start time by 5 minutes to tolerate time difference between machines
        /// </summary>
        private static TimeSpan blobSasStartTimeShift = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Read only SharedAccessPolicies
        /// </summary>
        private static SharedAccessBlobPolicies blobReadOnlyPolicies;

        /// <summary>
        /// Read/write SharedAccessPolicies
        /// </summary>
        private static SharedAccessBlobPolicies blobReadWritePolicies;

        /// <summary>
        /// Azure storage account for accessing the common data blob container
        /// </summary>
        private static CloudStorageAccount blobContainerAccount;

        /// <summary>
        /// Common data blob container name
        /// </summary>
        private static string blobContainerName;

        /// <summary>
        /// Lock object for blobContainerAccount
        /// </summary>
        private static object lockBlobContainerAccount = new object();

        /// <summary>
        /// Current Azure blob container for common data
        /// </summary>
        private CloudBlobContainer blobContainer;

        /// <summary>
        /// Azure storage credentials for accessing current common data blob container
        /// </summary>
        private StorageCredentials blobContainerCredentials;

        /// <summary>
        /// Name of a SharedAccessPolicy of the common data blob container
        /// </summary>
        private string blobAccessPolicy;

        /// <summary>
        /// Initializes static members of the BlobDataProvider class
        /// </summary>
        static BlobDataProvider()
        {
            // apply configured BlobSASExpirationPeriodInMinutes.
            string strBlobSASExpirationPeriod = ConfigurationManager.AppSettings[BlobSASExpirationConfig];
            if (!String.IsNullOrEmpty(strBlobSASExpirationPeriod))
            {
                TraceHelper.TraceEvent(TraceEventType.Information, "BlobDataProvider. value for config {0} is {1}", BlobSASExpirationConfig, strBlobSASExpirationPeriod);

                int temp;
                if (int.TryParse(strBlobSASExpirationPeriod, out temp) && temp > 0)
                {
                    blobSasExpirationPeriodInMinutes = temp;
                }
            }

            TraceHelper.TraceEvent(TraceEventType.Information, "BlobDataProvider. {0} = {1}", BlobSASExpirationConfig, blobSasExpirationPeriodInMinutes);
        }

        /// <summary>
        /// Gets or sets unique cluster id
        /// </summary>
        public static Guid UniqueClusterId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the StorageCredentials for accessing the common data blob container
        /// </summary>
        public StorageCredentials BlobContainerCredentials
        {
            get
            {
                return this.blobContainerCredentials;
            }
        }

        /// <summary>
        /// Set the CloudStorageAccount for accessing the common data blob container
        /// </summary>
        /// <param name="account">cloud storage account</param>
        public static void SetStorageAccount(CloudStorageAccount account)
        {
            lock (lockBlobContainerAccount)
            {
                if (account != null)
                {
                    blobContainerAccount = null;

                    blobContainerName = string.Format(BlobContainerNameFormat, UniqueClusterId.ToString());
                    TraceHelper.TraceEvent(TraceEventType.Information, "BlobDataProvider. Blob container name = {0}", blobContainerName);

                    CloudBlobClient client = new CloudBlobClient(new Uri(account.BlobEndpoint.AbsoluteUri), account.Credentials);
                    CloudBlobContainer container = client.GetContainerReference(blobContainerName);

                    try
                    {
                        bool isNewCreated = container.CreateIfNotExists();
                        blobReadOnlyPolicies = new SharedAccessBlobPolicies();
                        blobReadWritePolicies = new SharedAccessBlobPolicies();
                        if (!isNewCreated)
                        {
                            SharedAccessBlobPolicies policies = container.GetPermissions().SharedAccessPolicies;

                            foreach (KeyValuePair<string, SharedAccessBlobPolicy> policy in policies)
                            {
                                if ((policy.Value.Permissions & SharedAccessBlobPermissions.Read) == policy.Value.Permissions)
                                {
                                    blobReadOnlyPolicies.Add(policy.Key, policy.Value);
                                }
                                else
                                {
                                    blobReadWritePolicies.Add(policy.Key, policy.Value);
                                }
                            }
                        }

                        // set blobContainerAccount after blob container existence check passed
                        blobContainerAccount = account;
                    }
                    catch (StorageException ex)
                    {
                        TraceHelper.TraceEvent(
                            TraceEventType.Error,
                            "[BlobDataProvider].SetStorageAccount: failed to check blob container existence, error code={0}, exception={1}",
                            BurstUtility.GetStorageErrorCode(ex),
                            ex);
                    }
                    catch (Exception ex)
                    {
                        TraceHelper.TraceEvent(TraceEventType.Error, "[BlobDataProvider].SetStorageAccount: failed to check blob container existence, exception={0}", ex);
                    }
                }
                else
                {
                    blobContainerAccount = null;
                }
            }
        }

        /// <summary>
        /// Fetch attributes for the specified blob
        /// </summary>
        /// <param name="blob">target cloud blob</param>
        public static void GetBlobAttributes(CloudBlockBlob blob)
        {
            DataException exception = null;
            try
            {
                blob.FetchAttributes();
            }
            catch (StorageException ex)
            {
                TraceHelper.TraceEvent(
                    TraceEventType.Warning,
                    "[BlobDataProvider].GetBlobAttributes: exception encounterd. name={0}, error code={1}, exception={2}",
                    blob.Name,
                    BurstUtility.GetStorageErrorCode(ex),
                    ex);

                exception = DataUtility.ConvertToDataException(ex);
            }
            catch (Exception ex)
            {
                TraceHelper.TraceEvent(
                    TraceEventType.Error,
                    "[BlobDataProvider].GetBlobAttributes: exception encounterd. name={0}, exception={1}",
                    blob.Name,
                    ex);
                exception = new DataException(DataErrorCode.Unknown, ex);
            }

            if (exception != null)
            {
                exception.DataServer = blobContainerAccount.BlobEndpoint.AbsoluteUri;
                throw exception;
            }
        }

        /// <summary>
        /// Set attributes for the specified blob
        /// </summary>
        /// <param name="blob">target cloud blob</param>
        public static void SetBlobAttributes(CloudBlockBlob blob)
        {
            DataException exception = null;
            try
            {
                blob.Metadata[Constant.MetadataKeyLastUpdateTime] = DateTime.UtcNow.ToString();
                blob.SetMetadata();
            }
            catch (StorageException ex)
            {
                TraceHelper.TraceEvent(
                    TraceEventType.Error,
                    "[BlobDataProvider].SetBlobAttributes: exception encounterd. name={0}, error code={1}, exception={2}",
                    blob.Name,
                    BurstUtility.GetStorageErrorCode(ex),
                    ex);

                exception = DataUtility.ConvertToDataException(ex);
            }
            catch (Exception ex)
            {
                TraceHelper.TraceEvent(
                    TraceEventType.Error,
                    "[BlobDataProvider].SetBlobAttributes: exception encounterd. name={0}, exception={1}",
                    blob.Name,
                    ex);
                exception = new DataException(DataErrorCode.Unknown, ex);
            }

            if (exception != null)
            {
                exception.DataServer = blobContainerAccount.BlobEndpoint.AbsoluteUri;
                throw exception;
            }
        }

        /// <summary>
        /// Create a new data container
        /// </summary>
        /// <param name="name">data container name</param>
        /// <returns>information for accessing the data container</returns>
        public DataClientInfo CreateDataContainer(string name)
        {
            TraceHelper.TraceEvent(TraceEventType.Verbose, "[BlobDataProvider].CreateDataContainer: name={0}", name);
            SharedAccessBlobPermissions permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.Delete;
            CloudBlockBlob dataBlob = this.GetCloudBlobContainer(permissions).GetBlockBlobReference(name);

            // check if target blob already exists
            if (DataBlobExists(dataBlob))
            {
                throw new DataException(DataErrorCode.DataClientAlreadyExists, string.Empty);
            }

            CreateBlockBlob(dataBlob);

            TraceHelper.TraceEvent(TraceEventType.Verbose, "[BlobDataProvider].CreateDataContainer: name={0} created", name);

            DataClientInfo info = new DataClientInfo();
            string sas = dataBlob.GetSharedAccessSignature(new SharedAccessBlobPolicy(), this.blobAccessPolicy);

            // PrimaryDataPath stores data blob url, SecondaryDataPath stores blob url + sas(full control)
            info.PrimaryDataPath = dataBlob.Uri.AbsoluteUri;
            info.SecondaryDataPath = dataBlob.Uri.AbsoluteUri + sas;
            return info;
        }

        /// <summary>
        /// Open an existing data container
        /// </summary>
        /// <param name="name">name of the data container to be opened</param>
        /// <returns>information for accessing the data container</returns>
        public DataClientInfo OpenDataContainer(string name)
        {
            TraceHelper.TraceEvent(TraceEventType.Verbose, "[BlobDataProvider].OpenDataContainer: name={0}", name);

            CloudBlockBlob dataBlob = this.GetCloudBlobContainer(SharedAccessBlobPermissions.Read).GetBlockBlobReference(name);
            try
            {
                DataClientInfo info = new DataClientInfo();
                string sas = dataBlob.GetSharedAccessSignature(new SharedAccessBlobPolicy(), this.blobAccessPolicy);

                // PrimaryDataPath stores blob url, SecondaryDataPath stores blob url + sas(read)
                info.PrimaryDataPath = dataBlob.Uri.AbsoluteUri;
                info.SecondaryDataPath = dataBlob.Uri.AbsoluteUri + sas;
                return info;
            }
            catch (Exception ex)
            {
                throw new DataException(DataErrorCode.Unknown, ex);
            }
        }

        /// <summary>
        /// Delete a data container
        /// </summary>
        /// <param name="name">name of the data container to be deleted</param>
        public void DeleteDataContainer(string name)
        {
            TraceHelper.TraceEvent(TraceEventType.Verbose, "[BlobDataProvider].DeleteDataContainer: name={0}", name);

            DataException exception = null;
            CloudBlockBlob dataBlob = this.GetCloudBlobContainer(SharedAccessBlobPermissions.None).GetBlockBlobReference(name);
            try
            {
                dataBlob.DeleteIfExists();
            }
            catch (StorageException ex)
            {
                exception = DataUtility.ConvertToDataException(ex);
            }
            catch (Exception ex)
            {
                exception = new DataException(DataErrorCode.Unknown, ex);
            }

            if (exception != null)
            {
                exception.DataServer = blobContainerAccount.BlobEndpoint.AbsoluteUri;
                throw exception;
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets container attributes
        /// </summary>
        /// <param name="name"> data container name</param>
        /// <returns>data container attribute key and value pairs</returns>
        public Dictionary<string, string> GetDataContainerAttributes(string name)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// List all data containers
        /// </summary>
        /// <returns>List of all data containers</returns>
        public IEnumerable<string> ListAllDataContainers()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Set data container permissions
        /// </summary>
        /// <param name="name">data container name</param>
        /// <param name="userName">data container owner</param>
        /// <param name="allowedUsers">privileged users of the data container</param>
        public void SetDataContainerPermissions(string name, string userName, string[] allowedUsers)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Check if a user has specified permissions to a data container
        /// </summary>
        /// <param name="name">data container name</param>
        /// <param name="userIdentity">identity of the user to be checked</param>
        /// <param name="permissions">permissions to be checked</param>
        public void CheckDataContainerPermissions(string name, WindowsIdentity userIdentity, DataPermissions permissions)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create a block blob
        /// </summary>
        /// <param name="blob">data blob to be created</param>
        private static void CreateBlockBlob(CloudBlockBlob blob)
        {
            DataException exception = null;
            try
            {
                // create a new block blob by putting an empty block list
                blob.PutBlockList(new List<string>());
            }
            catch (StorageException ex)
            {
                exception = DataUtility.ConvertToDataException(ex);
            }
            catch (Exception ex)
            {
                exception = new DataException(DataErrorCode.Unknown, ex);
            }

            if (exception != null)
            {
                exception.DataServer = blobContainerAccount.BlobEndpoint.AbsoluteUri;
                throw exception;
            }
        }

        /// <summary>
        /// Check if a data blob exists or not
        /// </summary>
        /// <param name="blob">data blob to be checked</param>
        /// <returns>true if blob exists, false otherwise</returns>
        private static bool DataBlobExists(CloudBlockBlob blob)
        {
            DataException exception = null;
            try
            {
                blob.FetchAttributes();
                return true;
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
                {
                    return false;
                }
                else
                {
                    string errorCode = BurstUtility.GetStorageErrorCode(ex);

                    if (errorCode.Equals(StorageErrorCodeStrings.ResourceNotFound, StringComparison.OrdinalIgnoreCase) ||
                        errorCode.Equals(BlobErrorCodeStrings.BlobNotFound, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }

                    exception = DataUtility.ConvertToDataException(ex);
                }
            }
            catch (Exception ex)
            {
                exception = new DataException(DataErrorCode.Unknown, ex);
            }

            Debug.Assert(exception != null, "exception");

            exception.DataServer = blobContainerAccount.BlobEndpoint.AbsoluteUri;
            throw exception;
        }

        /// <summary>
        /// Get shared access policy with specified permission: readonly or not
        /// </summary>
        /// <param name="readOnlyPermission">a flag indicating whether retrieve a readonly SharedAccessPolicy or a read-write SharedAccessPolicy</param>
        /// <returns>name of the shared access policy</returns>
        private static string GetSharedAccessPolicy(bool readOnlyPermission)
        {
            string policyName = string.Empty;

            SharedAccessBlobPolicies policies = readOnlyPermission ? blobReadOnlyPolicies : blobReadWritePolicies;
            foreach (KeyValuePair<string, SharedAccessBlobPolicy> policy in policies)
            {
                if (DateTime.Now.AddMinutes(blobSasExpirationPeriodInMinutes).CompareTo(policy.Value.SharedAccessExpiryTime.Value.UtcDateTime) <= 0)
                {
                    // an active policy is found
                    policyName = policy.Key;
                    break;
                }
            }

            if (string.IsNullOrEmpty(policyName))
            {
                // if no active shared access policy is availabe
                // step 1. remove expired policies
                EvictExpiredPolicies(policies);

                // step 2. add a new policy
                policyName = Guid.NewGuid().ToString();
                policies.Add(
                    policyName,
                    GenerateBlobLevelSharedAccessPolicy(readOnlyPermission ? SharedAccessBlobPermissions.Read : SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.Delete));
                TraceHelper.TraceEvent(TraceEventType.Information, "BlobDataProvider: add new policy {0}", policyName);

                // step 3. update container's policies with the new one
                UpdateContainerPolicies();
            }

            TraceHelper.TraceEvent(TraceEventType.Verbose, "BlobDataProvider: GetSharedAccessPolicy returns {0}", policyName);
            return policyName;
        }

        /// <summary>
        /// Evict expired policies from specified SharedAccessPolicies
        /// </summary>
        /// <param name="policies">target SharedAccessPolicies</param>
        private static void EvictExpiredPolicies(SharedAccessBlobPolicies policies)
        {
            List<string> expiredPolicies = new List<string>();
            foreach (KeyValuePair<string, SharedAccessBlobPolicy> policy in policies)
            {
                if (DateTime.Now.CompareTo(policy.Value.SharedAccessExpiryTime.Value.UtcDateTime) >= 0)
                {
                    expiredPolicies.Add(policy.Key);
                }
            }

            foreach (string expiredPolicy in expiredPolicies)
            {
                TraceHelper.TraceEvent(TraceEventType.Information, "BlobDataProvider: evict expired policy {0}", expiredPolicy);
                policies.Remove(expiredPolicy);
            }
        }

        /// <summary>
        /// Update shared access policies for the common data blob container
        /// </summary>
        private static void UpdateContainerPolicies()
        {
            BlobContainerPermissions permissions = new BlobContainerPermissions();
            permissions.PublicAccess = BlobContainerPublicAccessType.Off;
            foreach (KeyValuePair<string, SharedAccessBlobPolicy> policy in blobReadOnlyPolicies)
            {
                permissions.SharedAccessPolicies.Add(policy.Key, policy.Value);
            }

            foreach (KeyValuePair<string, SharedAccessBlobPolicy> policy in blobReadWritePolicies)
            {
                permissions.SharedAccessPolicies.Add(policy.Key, policy.Value);
            }

            try
            {
                CloudBlobClient client = new CloudBlobClient(new Uri(blobContainerAccount.BlobEndpoint.AbsoluteUri), blobContainerAccount.Credentials);

                CloudBlobContainer container = client.GetContainerReference(blobContainerName);

                BlobRequestOptions options = new BlobRequestOptions()
                {
                    MaximumExecutionTime = TimeSpan.FromMinutes(SetPermissionMaxTimeInMinutes)
                };

                container.SetPermissions(permissions, null, options, null);
            }
            catch (Exception ex)
            {
                TraceHelper.TraceEvent(
                    TraceEventType.Error,
                    "Exception thrown when setting SharedAccessPermission of container {0}: {1}",
                    blobContainerName,
                    ex);

                throw;
            }
        }

        /// <summary>
        /// Generate blob-level shared access policy with specified permissions.
        /// </summary>
        /// <param name="permissions">permissions to be granted</param>
        /// <returns>shared access policy that grant specified permissions</returns>
        private static SharedAccessBlobPolicy GenerateBlobLevelSharedAccessPolicy(SharedAccessBlobPermissions permissions)
        {
            SharedAccessBlobPolicy policy = new SharedAccessBlobPolicy();
            policy.SharedAccessStartTime = DateTime.Now.Subtract(blobSasStartTimeShift);

            // expiration period is set to blobSasExpirationPeriodInMinutes*2.
            // Note: this shared access policy will be reused in
            // blobSasExpirationPeriodInMinutes, so SAS generated using
            // this policy has experiry period between blobSasExpirationPeriodInMinutes
            // and blobSasExpirationPeriodInMinutes*2
            policy.SharedAccessExpiryTime = policy.SharedAccessStartTime.Value.AddMinutes(blobSasExpirationPeriodInMinutes * 2);
            policy.Permissions = permissions;
            return policy;
        }



        /// <summary>
        /// Initialize the blobContainer instance
        /// </summary>
        /// <param name="permissions">shared access permissions required</param>
        /// <returns>cloud blob container for common data</returns>
        private CloudBlobContainer GetCloudBlobContainer(SharedAccessBlobPermissions permissions)
        {
            lock (lockBlobContainerAccount)
            {
                if (this.blobContainer == null)
                {
                    if (blobContainerAccount == null)
                    {
                        throw new DataException(DataErrorCode.DataServerForAzureBurstMisconfigured, string.Empty);
                    }

                    CloudBlobClient client = new CloudBlobClient(new Uri(blobContainerAccount.BlobEndpoint.AbsoluteUri), blobContainerAccount.Credentials);
                    this.blobContainer = client.GetContainerReference(blobContainerName);
                    this.blobContainerCredentials = blobContainerAccount.Credentials;

                    if (permissions != SharedAccessBlobPermissions.None)
                    {
                        if ((permissions & SharedAccessBlobPermissions.Read) == permissions)
                        {
                            this.blobAccessPolicy = GetSharedAccessPolicy(/*readOnly =*/true);
                        }
                        else
                        {
                            this.blobAccessPolicy = GetSharedAccessPolicy(/*readOnly =*/false);
                        }
                    }
                }

                return this.blobContainer;
            }
        }

        /// <summary>
        /// Check whether the data container path is a blob based
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsBlobDataContainerPath(string path)
        {
            if (path.StartsWith(HttpPrefix, StringComparison.InvariantCultureIgnoreCase) ||
                path.StartsWith(HttpsPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}
#endif