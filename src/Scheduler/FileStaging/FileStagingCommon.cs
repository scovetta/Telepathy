//--------------------------------------------------------------------------
// <copyright file="FileStagingCommon.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     Provides common constants and common methods to all parts of the
//     File Staging project.
// </summary>
//--------------------------------------------------------------------------

namespace Microsoft.Hpc.Azure.FileStaging
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using Microsoft.Hpc.Azure.Common;
    using Microsoft.Hpc.Azure.DataMovement;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Blob;

    /// <summary>
    /// This class provides common constants and common methods to all parts
    /// of the File Staging project.
    /// </summary>
    internal static class FileStagingCommon
    {
        /// <summary>
        /// wcf header namespace
        /// </summary>
        public const string WcfHeaderNamespace  = "Microsoft.Hpc.Azure.FileStaging";

        /// <summary>
        /// header name for target node
        /// </summary>
        public const string WcfHeaderTargetNode = "targetNode";

        /// <summary>
        /// header name for file path
        /// </summary>
        public const string WcfHeaderPath       = "path";

        /// <summary>
        /// header name for file mode
        /// </summary>
        public const string WcfHeaderMode       = "mode";

        /// <summary>
        /// header name for file position
        /// </summary>
        public const string WcfHeaderPosition   = "position";

        /// <summary>
        /// header name for "backward" flag
        /// </summary>
        public const string WcfHeaderBackward = "backward";

        /// <summary>
        /// header name for "lines" flag
        /// </summary>
        public const string WcfHeaderLines = "lines";

        /// <summary>
        /// header name for source file encoding
        /// </summary>
        public const string WcfHeaderEncoding = "encoding";

        /// <summary>
        /// header name for user sddl
        /// </summary>
        public const string WcfHeaderUserSddl   = "userSddl";

        /// <summary>
        /// header name for user name
        /// </summary>
        public const string WcfHeaderUserName   = "userName";

        /// <summary>
        /// header name for whether user is admin
        /// </summary>
        public const string WcfHeaderIsAdmin = "isAdmin";

        /// <summary>
        /// Azure proxy's identity (based on its server cert)
        /// </summary>
        public const string HpcAzureProxyServerIdentity = "Microsoft HPC Azure Service";

        /// <summary>
        /// Name of the Azure proxy SSL server cert that must be specifed as the endpoint's identity when connecting to the proxy
        /// </summary>
        public const string HpcAzureProxyServerCertName = "CN=" + FileStagingCommon.HpcAzureProxyServerIdentity;

        /// <summary>
        /// Name of the Azure proxy SSL client cert
        /// </summary>
        public const string HpcAzureProxyClientCertName = "CN=Microsoft HPC Azure Client";

        /// <summary>
        /// Try to write 64K at a time (to improve disk I/O)
        /// </summary>
        public const int FileWriteChunkSize = 64 * 1024;

        /// <summary>
        /// IO error code: file already exists
        /// </summary>
        public const int ErrorFileExists = 0x50;

        /// <summary>
        /// This prefix is used in net-tcp endpoints for file staging services
        /// </summary>
        private const string FileStagingUriScheme = "net.tcp";

        /// <summary>
        /// This prefix is used in https endpoints for file staging services
        /// </summary>
        private const string HttpsFileStagingUriScheme = "https";

        /// <summary>
        /// This timeout is used to set how long the Close operation is allowed to take before it times out. In SP2, this should become configurable.
        /// </summary>
        private const int CloseTimeoutMs = 3000;

        /// <summary>
        /// The number of calls that can be handled concurrently
        /// </summary>
        private const int MaxConcurrentCallsDefault = 64;

        /// <summary>
        /// The registry key for the MaxConcurrentCalls registry setting
        /// </summary>
        private const string HpcRegKey = @"HKEY_LOCAL_MACHINE\Software\Microsoft\HPC";

        /// <summary>
        /// The value name for the MaxConcurrentCalls registry setting
        /// </summary>
        private const string MaxConcurrentCallsRegSetting = "FileStagingMaxConcurrentCalls";

        /// <summary>
        /// The value name for the SchedulerOnAzure registry setting
        /// </summary>
        private const string SchedulerOnAzureRegSetting = "SchedulerOnAzure";

        /// <summary>
        /// Blob url prefix format: ContainerUrl/UniqueOperationId/
        /// </summary>
        private const string BlobUrlPrefixFormat = @"{0}/{1}/";

        /// <summary>
        /// Defines amount of concurrent file transfers to use
        /// </summary>
        private const int ConcurrentFileTransferCount = 4;

        /// <summary>
        /// Default library directory path on on-premise installation and Azure VM role
        /// </summary>
        private const string HpcAssemblyDir = @"%CCP_HOME%bin";

        /// <summary>
        /// Default library directory path on Azure worker role
        /// </summary>
        private const string HpcAssemblyDir2 = @"%CCP_HOME%";

        /// <summary>
        /// Data movement library name
        /// </summary>
        private const string DataMovementAssemblyName = "Microsoft.WindowsAzure.Storage.DataMovement.dll";

        /// <summary>
        /// Storage client library name
        /// </summary>
        private const string StorageClientAssemblyName = "Microsoft.WindowsAzure.Storage.dll";

        /// <summary>
        /// Azure service runtime library name
        /// </summary>
        private const string AzureServiceRuntimeAssemblyName = "Microsoft.WindowsAzure.ServiceRuntime.dll";

        /// <summary>
        /// Callback triggered before a blob transfer
        /// </summary>
        /// <param name="userName">name of user on behalf of whom the operation is performed</param>
        /// <param name="sourcePath">source file path</param>
        /// <param name="destPath">destination file path</param>
        /// <returns>>true if the file should be transferred; otherwise false</returns>
        public delegate bool BeforeBlobTransferCallback(string userName, string sourcePath, string destPath);

        /// <summary>
        /// Callback triggered after a blob transfer
        /// </summary>
        /// <param name="userName">name of user on behalf of whom the operation is performed</param>
        /// <param name="sourcePath">source file path</param>
        /// <param name="destPath">destination file path</param>
        public delegate void AfterBlobTransferCallback(string userName, string sourcePath, string destPath);

        /// <summary>
        /// Initializes static members of the FileStagingCommon class
        /// </summary>
        static FileStagingCommon()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveHandler;
        }

        /// <summary>
        /// Gets max concurrent calls that File Staging service is able to handle concurrently
        /// </summary>
        public static int MaxConcurrentCalls
        {
            get
            {
                try
                {
                    return (int)Microsoft.Win32.Registry.GetValue(HpcRegKey, MaxConcurrentCallsRegSetting, MaxConcurrentCallsDefault);
                }
                catch
                {
                    return MaxConcurrentCallsDefault;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether scheduler is running on Azure.
        /// </summary>
        public static bool SchedulerOnAzure
        {
            get
            {
                string strSchedulerOnAzure = Environment.GetEnvironmentVariable("CCP_SCHEDULERONAZURE", EnvironmentVariableTarget.Machine);
                return (!string.IsNullOrEmpty(strSchedulerOnAzure)) && strSchedulerOnAzure.Equals("1");
            }
        }

        /// <summary>
        /// Returns an endpoint for the File Staging service on the localhost
        /// </summary>
        /// <returns>an endpoint for the FileStaging service on localhost</returns>
        public static EndpointAddress GetFileStagingEndpoint()
        {
            UriBuilder uri = new UriBuilder(FileStagingUriScheme, "localhost", int.Parse(SchedulerPorts.FileStagingPort), SchedulerEndpointNames.FileStagingService);
            EndpointAddress address = new EndpointAddress(uri.Uri);
            return address;
        }

        /// <summary>
        /// Returns an endpoint for the File Staging service on the specified node
        /// </summary>
        /// <param name="host">target node name</param>
        /// <returns>an endpoint for the File Staging service on the specified node</returns>
        public static EndpointAddress GetFileStagingEndpoint(string host)
        {
            return GetFileStagingEndpoint(host, SchedulerPorts.FileStagingPort, EndpointIdentity.CreateDnsIdentity(host));
        }

        /// <summary>
        /// Returns an endpoint for the File Staging service on the specified node, with the specified port. The port may change
        /// when running an internal endpoint in Azure.
        /// </summary>
        /// <param name="host">target node name</param>
        /// <param name="port">File Staging service listening port</param>
        /// <returns>an endpoint for the File Staging service on the specified node with the specified port</returns>
        public static EndpointAddress GetFileStagingEndpoint(string host, string port)
        {
            return GetFileStagingEndpoint(host, port, EndpointIdentity.CreateDnsIdentity(host));
        }

        /// <summary>
        /// Returns an endpoint for the File Staging service on the specified node with the specified identity
        /// </summary>
        /// <param name="host">target node name</param>
        /// <param name="identity">endpoint identity</param>
        /// <returns>an endpoint for the File Staging service on the specified node with the specified identity</returns>
        public static EndpointAddress GetFileStagingEndpoint(string host, EndpointIdentity identity)
        {
            return GetFileStagingEndpoint(host, SchedulerPorts.FileStagingPort, identity);
        }

        /// <summary>
        /// Returns an endpoint for the File Staging service on the specified node,
        /// with the specified port and endpoint identity. The port may change
        /// when running an internal endpoint in Azure.
        /// </summary>
        /// <param name="host">target node name</param>
        /// <param name="port">File Staging service listening port</param>
        /// <param name="identity">endpoint identity</param>
        /// <returns>an endpoint for the File Staging service on the specified node,
        /// with the specified port and endpoint identity</returns>
        public static EndpointAddress GetFileStagingEndpoint(string host, string port, EndpointIdentity identity)
        {
            foreach (IPAddress resolvedAddress in Dns.GetHostAddresses(host))
            {
                if (resolvedAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    UriBuilder uri = new UriBuilder(FileStagingUriScheme, resolvedAddress.ToString(), int.Parse(port), SchedulerEndpointNames.FileStagingService);
                    EndpointAddress address = new EndpointAddress(uri.Uri, identity, new AddressHeaderCollection());
                    return address;
                }
            }

            throw new EndpointNotFoundException(host);
        }

        /// <summary>
        /// Returns an http endpoint for the File Staging service on the specified node,
        /// with the specified port and endpoint identity. The port may change
        /// when running an internal endpoint in Azure.
        /// Especially, this function is called when establishing a http connection between
        /// SchedulerFileStagingProxy and AzureFileStagingProxy
        /// </summary>
        /// <param name="host">target node name</param>
        /// <param name="port">File Staging service listening port</param>
        /// <param name="identity">endpoint identity</param>
        /// <returns>an endpoint for the File Staging service on the specified node,
        /// with the specified port and endpoint identity</returns>
        public static EndpointAddress GetHttpsFileStagingEndpoint(string host, string port, EndpointIdentity identity)
        {
            if (identity == null)
            {
                identity = EndpointIdentity.CreateDnsIdentity(host);
            }

            foreach (IPAddress resolvedAddress in Dns.GetHostAddresses(host))
            {
                if (resolvedAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    UriBuilder uri = new UriBuilder(HttpsFileStagingUriScheme, resolvedAddress.ToString(), int.Parse(port), SchedulerEndpointNames.FileStagingService);
                    EndpointAddress address = new EndpointAddress(uri.Uri, identity, new AddressHeaderCollection());
                    return address;
                }
            }

            throw new EndpointNotFoundException(host);
        }

        /// <summary>
        /// Returns an endpoint for the File Staging service on the head node.
        /// On the head node, the port is different so that the proxy service
        /// does not interfere with the node manager service.
        /// </summary>
        /// <returns>an endpoint for the File Staging service on the head node</returns>
        public static EndpointAddress GetFileStagingEndpointOnHeadNode()
        {
            UriBuilder uri = new UriBuilder(FileStagingUriScheme, "localhost", int.Parse(SchedulerPorts.FileStagingHeadNodePort), SchedulerEndpointNames.FileStagingService);
            EndpointAddress address = new EndpointAddress(uri.Uri);
            return address;
        }

        /// <summary>
        /// Returns an endpoint for the File Staging service on the head node.
        /// On the head node, the port is different so that the proxy service
        /// does not interfere with the node manager service.
        /// </summary>
        /// <param name="host">head node name</param>
        /// <returns>an endpoint for the File Staging service on the head node</returns>
        public static EndpointAddress GetFileStagingEndpointOnHeadNode(string host)
        {
            return GetFileStagingEndpoint(host, SchedulerPorts.FileStagingHeadNodePort, EndpointIdentity.CreateDnsIdentity(host));
        }

        /// <summary>
        /// Returns the binding for file staging service
        /// </summary>
        /// <returns>the binding for file staging service</returns>
        public static NetTcpBinding GetFileStagingBinding()
        {
            // Create a new NetTcpBinding. Security is on the Transport layer rather than the Message layer because 
            // this allows us to use WCF streaming, which can't be done with Message security.
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None, false);

            // Allow files of up to 4 GB to transfer in chunks of up to 64 KB.
            binding.MaxReceivedMessageSize = 4L * 1024L * 1024L * 1024L; // 4 GB
            binding.MaxBufferSize = 64 * 1024; // 64 K

            // Large file transfers will require a lot of time. We can't disable the send timeout, because it throws exceptions in
            // the proxy (I'm not exactly sure why, but I have some ideas). The best we can do is to set the timeout to be a
            // week. Any value greater than the max int value for milliseconds (works out to be about 24 days) fails.
            // My best idea for why TimeSpan.MaxValue fails is that it has to do with the clocks being out of sync between Azure and
            // anywhere else in the world.

            // I believe that if Azure and the HN ever have clocks that are out of sync by more than 18 days, the proxy will fail.
            binding.SendTimeout = TimeSpan.FromDays(7);

            // Make sure close operations don't take too long.
            binding.CloseTimeout = TimeSpan.FromMilliseconds(CloseTimeoutMs);

            // Enable the stream mode
            binding.TransactionFlow = false;
            binding.TransferMode = TransferMode.Streamed;
            binding.TransactionProtocol = TransactionProtocol.OleTransactions;

            return binding;
        }

        /// <summary>
        /// Get the secure binding for file staging service
        /// </summary>
        /// <returns>the secure binding for file staging service</returns>
        public static NetTcpBinding GetSecureFileStagingBinding()
        {
            NetTcpBinding binding = GetFileStagingBinding();

            // Use Windows credentials (so that they can be impersonated) and encrypt messages so that they
            // can't be read by third parties.
            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;

            return binding;
        }

        /// <summary>
        /// Get certificate based secure binding for file staging service
        /// </summary>
        /// <returns>certificate based secure binding for file staging service</returns>
        public static NetTcpBinding GetCertificateFileStagingBinding()
        {
            NetTcpBinding binding = GetFileStagingBinding();

            // Use a certificate to encrypt messages so that they can't be faked by third parties.
            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;

            return binding;
        }

        public static Binding GetHttpsFileStagingBinding()
        {
            HttpsTransportBindingElement transportElement = new HttpsTransportBindingElement();
            transportElement.TransferMode = TransferMode.Streamed;
            transportElement.RequireClientCertificate = true;

            // Allow files of up to 4 GB to transfer in chunks of up to 64 KB.
            transportElement.MaxReceivedMessageSize = 4L * 1024L * 1024L * 1024L; // 4 GB
            transportElement.MaxBufferSize = 64 * 1024; // 64 K

            TextMessageEncodingBindingElement messageElement = new TextMessageEncodingBindingElement();
            messageElement.MessageVersion = MessageVersion.CreateVersion(EnvelopeVersion.Soap12, AddressingVersion.WSAddressing10);
            messageElement.ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas()
            {
                MaxArrayLength = 64 * 1024 * 1024,          // 64 M
                MaxDepth = 64 * 1024 * 1024,                // 64 M
                MaxStringContentLength = 64 * 1024 * 1024,  // 64 M
                MaxBytesPerRead = 64 * 1024 * 1024,         // 64 M
            };

            CustomBinding binding = new CustomBinding(messageElement, transportElement);
            binding.SendTimeout = TimeSpan.FromDays(7);
            binding.CloseTimeout = TimeSpan.FromMilliseconds(CloseTimeoutMs);

            return binding;
        }

        /// <summary>
        /// Upload a file from local to Azure blob
        /// </summary>
        /// <param name="blobUrl">destination blob url</param>
        /// <param name="sas">Azure SAS for accessing the blob</param>
        /// <param name="filePath">local file path</param>
        public static void UploadFileToBlob(string blobUrl, string sas, string filePath)
        {
            CloudBlockBlob blob = new CloudBlockBlob(new Uri(blobUrl), new StorageCredentials(sas));

            using (BlobTransferManager transferManager = new BlobTransferManager(
                new BlobTransferOptions
                {
                    Concurrency = Environment.ProcessorCount
                }))
            {
                Exception exception = null;                

                transferManager.QueueUpload(
                    blob, 
                    filePath, 
                    null, 
                    null,
                    delegate(object userData, Exception ex)
                    {
                        if (ex != null && exception == null)
                        {
                            exception = ex;
                        }
                    },
                    null);

                transferManager.WaitForCompletion();

                if (exception != null)
                {
                    throw exception;
                }
            }
        }

        /// <summary>
        /// Download a file from Azure blob to local
        /// </summary>
        /// <param name="blobUrl">source blob url</param>
        /// <param name="sas">Azure SAS for accessing the blob</param>
        /// <param name="filePath">destination file path</param>
        public static void DownloadFileFromBlob(string blobUrl, string sas, string filePath)
        {
            CloudBlockBlob blob = new CloudBlockBlob(new Uri(blobUrl), new StorageCredentials(sas));

            using (BlobTransferManager transferManager = new BlobTransferManager(
                new BlobTransferOptions
                {
                    Concurrency = Environment.ProcessorCount
                }))
            {
                Exception exception = null;                

                transferManager.QueueDownload(
                    blob,
                    filePath,
                    true,
                    null,
                    null,
                    delegate(object userData, Exception ex)
                    {
                        if (ex != null && exception == null)
                        {
                            exception = ex;
                        }
                    },
                    null);
                
                transferManager.WaitForCompletion();

                if (exception != null)
                {
                    throw exception;
                }
            }
        }

        /// <summary>
        /// Upload a directory from local to blob
        /// </summary>
        /// <param name="blobUrlPrefix">blob url prefix</param>
        /// <param name="sas">shared access sigature for accessing the blob storage</param>
        /// <param name="dirName">local directory where local files are located</param>
        /// <param name="filePatterns">files to be uploaded</param>
        /// <param name="recursive">if upload directory recursively if filePatterns matches subdirectories</param>
        /// <param name="overwrite">if overwrite existing files</param>
        /// <param name="userName">name of user the operation is performed on behalf of</param>
        /// <param name="beforeUploadCallback">callback triggered before uploading a file</param>
        /// <param name="afterUploadCallback">callback triggered after uploading a file</param>
        public static void UploadDirectoryToBlob(
            string blobUrlPrefix,
            string sas,
            string dirName,
            List<string> filePatterns,
            bool recursive,
            bool overwrite,
            string userName,
            BeforeBlobTransferCallback beforeUploadCallback,
            AfterBlobTransferCallback afterUploadCallback)
        {
            using (BlobTransferManager transferManager = new BlobTransferManager(
                 new BlobTransferOptions
                 {
                     Concurrency = Environment.ProcessorCount * 2
                 }))
            {
                BlobTransferRecursiveTransferOptions options = new BlobTransferRecursiveTransferOptions
                {
                    DestinationSAS = sas,
                    FilePatterns = filePatterns,
                    Recursive = recursive,      
                    ExcludeNewer = !overwrite,
                    ExcludeOlder = !overwrite,
                };
                
                if (beforeUploadCallback != null)
                {
                    // TODO:
                    // options.BeforeQueueCallback = delegate(string sourcePath, string destinationPath) { return beforeUploadCallback(userName, sourcePath, destinationPath); };
                }

                Exception exception = null;

                transferManager.QueueRecursiveTransfer(
                    dirName,
                    blobUrlPrefix,
                    options,
                    null,
                    null,
                    null,
                    null,
                    delegate(object userData, EntryData entryData, Exception entryException)
                    {
                        if (entryException != null && exception == null)
                        {
                            exception = entryException;
                        }
                        else
                        {
                            if (afterUploadCallback != null)
                            {
                                afterUploadCallback(userName, entryData.FileName, entryData.DestinationBlob.Uri.AbsoluteUri);
                            }
                        }
                    },
                    null);

                transferManager.WaitForCompletion();

                if (exception != null)
                {
                    throw exception;
                }
            }
        }

        /// <summary>
        /// Download a directory from blob to local
        /// </summary>
        /// <param name="blobUrlPrefix">blob url prefix</param>
        /// <param name="sas">shared access sigature for accessing the blob storage</param>
        /// <param name="dirName">local directory where downloaded files are placed</param>
        /// <param name="filePatterns">blobs to be downloaded</param>
        /// <param name="recursive">if download blobs recusively or not</param>
        /// <param name="overwrite">if overwrite existing files</param>
        /// <param name="userName">name of user the operation is performed on behalf of</param>
        /// <param name="beforeDownloadCallback">callback triggerred before downloading a file</param>
        /// <param name="afterDownloadCallback">callback triggerred after downloading a file</param>
        public static void DownloadDirectoryFromBlob(
            string blobUrlPrefix,
            string sas,
            string dirName,
            List<string> filePatterns,
            bool recursive,
            bool overwrite,
            string userName,
            BeforeBlobTransferCallback beforeDownloadCallback,
            AfterBlobTransferCallback afterDownloadCallback)
        {
            using (BlobTransferManager transferManager = new BlobTransferManager(
                 new BlobTransferOptions
                 {
                     Concurrency = Environment.ProcessorCount * 2,
                 }))
            {
                BlobTransferRecursiveTransferOptions options = new BlobTransferRecursiveTransferOptions
                {
                    SourceSAS = sas,
                    FilePatterns = filePatterns,
                    Recursive = recursive,
                    ExcludeNewer = !overwrite,
                    ExcludeOlder = !overwrite,
                };

                if (beforeDownloadCallback != null)
                {
                    // TODO:
                    // options.BeforeQueueCallback = delegate(string sourcePath, string destinationPath) { return beforeDownloadCallback(userName, sourcePath, destinationPath); };
                }

                Exception exception = null;

                transferManager.QueueRecursiveTransfer(
                    blobUrlPrefix,
                    dirName,
                    options,
                    null,
                    null,
                    null,
                    null,
                    delegate(object userData, EntryData entryData, Exception entryException)
                    {
                        if (entryException != null && exception == null)
                        {
                            exception = entryException;
                        }
                        else
                        {
                            if (afterDownloadCallback != null)
                            {
                                afterDownloadCallback(userName, entryData.SourceBlob.Uri.AbsoluteUri, entryData.FileName);
                            }
                        }
                    },
                    null);
                
                transferManager.WaitForCompletion();

                if (exception != null)
                {
                    throw exception;
                }
            }
        }

        /// <summary>
        /// Delete a blob
        /// </summary>
        /// <param name="blobUrl">target blob url</param>
        /// <param name="sas">shared access sigature for accessing the blob storage</param>
        public static void DeleteBlob(string blobUrl, string sas)
        {
            CloudBlockBlob blob = new CloudBlockBlob(new Uri(blobUrl), new StorageCredentials(sas));
            blob.DeleteIfExists();
        }

        /// <summary>
        /// Delete blobs with the specified prefix and matching specified file patterns
        /// </summary>
        /// <param name="containerUrl">container url</param>
        /// <param name="blobUrlPrefix">blob url prefix</param>
        /// <param name="sas">shared access sigature for accessing the blob storage</param>
        public static void DeleteBlobs(string containerUrl, string blobUrlPrefix, string sas)
        {
            StorageCredentials credentials = new StorageCredentials(sas);
            CloudBlobContainer container = new CloudBlobContainer(new Uri(containerUrl), credentials);
            CloudBlobDirectory blobDirectory = container.GetDirectoryReference(blobUrlPrefix);

            BlobContinuationToken continuationToken = null;
            var resultSegment = blobDirectory.ListBlobsSegmented(true, BlobListingDetails.None, null, null, null, null);
            while (true)
            {
                if (resultSegment == null)
                {
                    return;
                }

                foreach (var blobItem in resultSegment.Results)
                {
                    CloudBlockBlob blob = new CloudBlockBlob(new Uri(blobItem.Uri.AbsoluteUri), credentials);
                    blob.DeleteIfExists();
                }

                continuationToken = resultSegment.ContinuationToken;
                if (continuationToken == null)
                {
                    break;
                }

                // return next 1000 blob items
                resultSegment = blobDirectory.ListBlobsSegmented(true, BlobListingDetails.None, null, continuationToken, null, null);
            }
        }

        /// <summary>
        /// Get blob url prefix
        /// </summary>
        /// <param name="containerUrl">url of the container this blob belongs to</param>
        /// <param name="uniqueOperationId">unique operation id</param>
        /// <returns>blob url prefix</returns>
        public static string GetBlobUrlPrefix(string containerUrl, string uniqueOperationId)
        {
            return string.Format(BlobUrlPrefixFormat, containerUrl, uniqueOperationId);
        }

        /// <summary>
        /// Load the assembly from some customized path, if it cannot be found automatically.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">A System.ResolveEventArgs that contains the event data.</param>
        /// <returns>targeted assembly</returns>
        private static Assembly ResolveHandler(object sender, ResolveEventArgs args)
        {
            if (string.IsNullOrEmpty(args.Name))
            {
                return null;
            }

            // Session API assembly may be installed in GAC, or %CCP_HOME%bin,
            // or "%CCP_HOME%"; while Microsoft.WindowsAzure.Storage.DataMovement.dll
            // and Microsoft.WindowsAzure.Storage.dll
            // may be installed in %CCP_HOME%bin, or "%CCP_HOME%".  If they are
            // located at different places, we need load it from target folder
            // explicitly
            AssemblyName targetAssemblyName = new AssemblyName(args.Name);
            if (targetAssemblyName.Name.Equals(Path.GetFileNameWithoutExtension(DataMovementAssemblyName), StringComparison.OrdinalIgnoreCase))
            {
                return LoadAssembly(DataMovementAssemblyName);
            }
            else if (targetAssemblyName.Name.Equals(Path.GetFileNameWithoutExtension(StorageClientAssemblyName), StringComparison.OrdinalIgnoreCase))
            {
                return LoadAssembly(StorageClientAssemblyName);
            }
            else if (targetAssemblyName.Name.Equals(Path.GetFileNameWithoutExtension(AzureServiceRuntimeAssemblyName), StringComparison.OrdinalIgnoreCase))
            {
                return LoadAssembly(AzureServiceRuntimeAssemblyName);
            }

            return null;
        }

        /// <summary>
        /// Load assembly from default HPC installation directory
        /// </summary>
        /// <param name="assemblyName">name of the assembly to be loaded</param>
        /// <returns>targeted assembly</returns>
        private static Assembly LoadAssembly(string assemblyName)
        {
            string assemblyPath = Path.Combine(Environment.ExpandEnvironmentVariables(HpcAssemblyDir), assemblyName);
            if (!File.Exists(assemblyPath))
            {
                assemblyPath = Path.Combine(Environment.ExpandEnvironmentVariables(HpcAssemblyDir2), assemblyName);
            }

            return Assembly.LoadFrom(assemblyPath);
        }
    }
}
