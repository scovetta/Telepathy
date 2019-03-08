namespace Microsoft.Hpc.Telepathy
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    // TODO: azure storage client side retry
    // TODO: TEST CASE 1 - mixed cases in service name
    public class AzureBlobServiceRegistrationStore : IServiceRegistrationStore
    {
        private CloudStorageAccount cloudStorageAccount;

        private CloudBlobClient blobClient;

        private CloudBlobContainer blobContainer;

        private const string ServiceRegistrationBlobContainerName = "service-registration";

        public AzureBlobServiceRegistrationStore(string connectionString)
        {
            this.blobContainer = CloudStorageAccount.Parse(connectionString).CreateCloudBlobClient().GetContainerReference(ServiceRegistrationBlobContainerName);
            this.blobContainer.CreateIfNotExists();
        }

        private CloudBlockBlob GetServiceRegistrationBlockBlobReference(string serviceName, Version serviceVersion)
        {
            var fileName = SoaRegistrationAuxModule.GetRegistrationFileName(serviceName, serviceVersion);
            Trace.TraceInformation($"Getting block blob reference for {fileName}.");
            return this.blobContainer.GetBlockBlobReference(fileName);
        }

        public async Task<string> GetMd5Async(string serviceName, Version serviceVersion)
        {
            var blob = this.GetServiceRegistrationBlockBlobReference(serviceName, serviceVersion);
            if (await blob.ExistsAsync())
            {
                await blob.FetchAttributesAsync();
                return blob.Properties.ContentMD5;
            }
            else
            {
                return string.Empty;
            }
        }

        public async Task<string> GetAsync(string serviceName, Version serviceVersion)
        {
            var blob = this.GetServiceRegistrationBlockBlobReference(serviceName, serviceVersion);
            if (await blob.ExistsAsync())
            {
                return await blob.DownloadTextAsync();
            }
            else
            {
                return string.Empty;
            }
        }

        public async Task SetAsync(string serviceName, Version serviceVersion, string serviceRegistration)
        {
            var blob = this.GetServiceRegistrationBlockBlobReference(serviceName, serviceVersion);
            await blob.UploadTextAsync(serviceRegistration);
        }

        public async Task DeleteAsync(string serviceName, Version serviceVersion)
        {
            var blob = this.GetServiceRegistrationBlockBlobReference(serviceName, serviceVersion);
            await blob.DeleteIfExistsAsync();
        }

        // TODO: continuously query
        public async Task<List<string>> EnumerateAsync() =>
            (await this.blobContainer.ListBlobsSegmentedAsync(null, null)).Results.Select(b => Path.GetFileNameWithoutExtension(b.Uri.ToString())).ToList();

        public async Task ImportFromFileAsync(string filePath, string serviceName)
        {
            Debug.Assert(filePath != null);
            if (!File.Exists(filePath))
            {
                throw new InvalidOperationException($"File {filePath} for uploading does not exist.");
            }

            string svcName;
            if (string.IsNullOrEmpty(serviceName))
            {
                svcName = Path.GetFileNameWithoutExtension(filePath).ToLowerInvariant();
            }
            else
            {
                svcName = SoaRegistrationAuxModule.GetRegistrationFileName(serviceName, null);
            }

            var blob = this.GetServiceRegistrationBlockBlobReference(svcName, null);
            await blob.UploadFromFileAsync(filePath);
        }

        // TODO: Design - do we still need check MD5 here?
        public async Task<string> ExportToTempFileAsync(string serviceName, Version serviceVersion)
        {
            var blob = this.GetServiceRegistrationBlockBlobReference(serviceName, serviceVersion);
            var filePath = SoaRegistrationAuxModule.GetServiceRegistrationTempFilePath(Path.GetFileNameWithoutExtension(blob.Uri.ToString()));
            Trace.TraceInformation($"Will write Service Registration file to {filePath}");
            if (await blob.ExistsAsync())
            {
                await blob.DownloadToFileAsync(filePath, FileMode.Create);
                return filePath;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}