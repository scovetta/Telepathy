// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Data.Standalone
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    public class StandaloneDataClient 
    {
        private const string DefaultContainerName = "hpc-soa-common-data";

        private CloudStorageAccount storageAccount;
        private CloudBlobContainer blobContainer;

        private static readonly SharedAccessBlobPolicy ReadOnlySas = new SharedAccessBlobPolicy()
                                                                         {
                                                                             SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24),
                                                                             Permissions = SharedAccessBlobPermissions.Read
                                                                         };


        private static string GetBlobSasUri(CloudBlob blob, SharedAccessBlobPolicy policy)
        {
            var signature = blob.GetSharedAccessSignature(policy);
            return blob.Uri + signature;
        }
       

        public StandaloneDataClient(string storageCredential, string containerName)
        {
            this.storageAccount = CloudStorageAccount.Parse(storageCredential);
            var blobClient = this.storageAccount.CreateCloudBlobClient();
            this.blobContainer = blobClient.GetContainerReference(containerName);
        }

        public StandaloneDataClient( string storageCredential) : this( storageCredential, DefaultContainerName)
        {
        }

        public async Task<string[]> UploadFilesAsync(IEnumerable<string> paths)
        {
            var uploadTasks = paths.Select(this.UploadFileAsync);
            var sasTokens = await Task.WhenAll(uploadTasks);
            return sasTokens;
        }

        public async Task<string> UploadFileAsync(string path)
        {
            if (!File.Exists(path))
            {
                throw new ArgumentException($"File {path} now exist.", nameof(path));
            }

            await this.blobContainer.CreateIfNotExistsAsync();

            CloudBlockBlob blob = this.blobContainer.GetBlockBlobReference(Path.GetFileName(path));
            await blob.UploadFromFileAsync(path);

            return GetBlobSasUri(blob, ReadOnlySas);
        }

        public async Task<string> DownloadFileAsync(string fileName, string destinationFilePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destinationFilePath));

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException($"File name is null or empty.", nameof(fileName));
            }

            await this.blobContainer.CreateIfNotExistsAsync();
            
            CloudBlockBlob blob = this.blobContainer.GetBlockBlobReference(fileName);
            await blob.DownloadToFileAsync(destinationFilePath, FileMode.CreateNew);
            return destinationFilePath;
        }

        public static async Task<string> DownloadFileFromSasAsync(string sas, string destinationFilePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destinationFilePath));

            if (string.IsNullOrEmpty(destinationFilePath))
            {
                throw new ArgumentException($"Destination is null or empty.", nameof(destinationFilePath));
            }

            CloudBlockBlob blob = new CloudBlockBlob(new Uri(sas));
            if (File.Exists(destinationFilePath))
            {
                File.Delete(destinationFilePath);
            }

            await blob.DownloadToFileAsync(destinationFilePath, FileMode.CreateNew);
            return destinationFilePath;
        }
    }
}
