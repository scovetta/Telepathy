// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Common.UnitTest
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Telepathy.Common;
    using Microsoft.Telepathy.Common.ServiceRegistrationStore;
    using System.Text;

    [TestClass]
    public class AzureBlobServiceRegistrationStoreUnitTest
    {
        private const string storageConnectionString = "UseDevelopmentStorage=true;";

        private CloudStorageAccount storageAccount;

        private AzureBlobServiceRegistrationStore serviceRegistrationStore = new AzureBlobServiceRegistrationStore(storageConnectionString);

        private const string emptyContent = "";

        private const string oneCharContent = "<";

        private const string fileContent = "test file content";

        [TestMethod]
        public async Task CalculateMd5HashEmptyTest()
        {
            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {
                CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference("service-registration");
                if (!cloudBlobContainer.Exists())
                {
                    await cloudBlobContainer.CreateAsync();
                }

                string fileName = "ccpechosvc_empty.config";

                Console.WriteLine("Temp file conetnt : {0}", emptyContent);
                Console.WriteLine("Uploading to Blob storage as blob '{0}'", fileName);

                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);
                await cloudBlockBlob.UploadTextAsync(emptyContent);
                string md5 = cloudBlockBlob.Properties.ContentMD5;
                string calculatedMd5 = serviceRegistrationStore.CalculateMd5Hash(Encoding.ASCII.GetBytes(emptyContent));
                Console.WriteLine("storage property md5 value = {0}", md5);
                Console.WriteLine("calculated md5 value = {0}", calculatedMd5);
                Assert.AreEqual(calculatedMd5, md5);
            }
        }

        [TestMethod]
        public async Task CalculateMd5HashOneCharTest()
        {
            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {
                CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference("service-registration");
                if (!cloudBlobContainer.Exists())
                {
                    await cloudBlobContainer.CreateAsync();
                }

                string fileName = "ccpechosvc_1.config";
              
                Console.WriteLine("Temp file content : {0}", oneCharContent);
                Console.WriteLine("Uploading to Blob storage as blob '{0}'", fileName);

                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);
                await cloudBlockBlob.UploadTextAsync(oneCharContent);
                string md5 = cloudBlockBlob.Properties.ContentMD5;
                string calculatedMd5 = serviceRegistrationStore.CalculateMd5Hash(Encoding.ASCII.GetBytes(oneCharContent));
                Console.WriteLine("storage property md5 value = {0}", md5);
                Console.WriteLine("calculated md5 value = {0}", calculatedMd5);
                Assert.AreEqual(calculatedMd5, md5);
            }
        }

        [TestMethod]
        public async Task CalculateMd5HashTest()
        {
            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {
                CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference("service-registration");
                if (!cloudBlobContainer.Exists())
                {
                    await cloudBlobContainer.CreateAsync();
                }

                string localFileName = "ccpechosvc.config";
            
                Console.WriteLine("Temp file content : {0}", fileContent);
                Console.WriteLine("Uploading to Blob storage as blob '{0}'", localFileName);

                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(localFileName);
                await cloudBlockBlob.UploadTextAsync(fileContent);
                string md5 = cloudBlockBlob.Properties.ContentMD5;
                string calculatedMd5 = serviceRegistrationStore.CalculateMd5Hash(Encoding.ASCII.GetBytes(fileContent));
                Console.WriteLine("storage property md5 value = {0}", md5);
                Console.WriteLine("calculated md5 value = {0}", calculatedMd5);
                Assert.AreEqual(calculatedMd5, md5);
            }
        }

        [TestMethod]
        public async Task CalculateMd5HashContentChangeTest()
        {
            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {
                CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference("service-registration");
                if (!cloudBlobContainer.Exists())
                {
                    await cloudBlobContainer.CreateAsync();
                }

                string localFileName = "ccpechosvc.config";
             
                Console.WriteLine("Temp file content : {0}", fileContent);
                Console.WriteLine("Uploading to Blob storage as blob '{0}'", localFileName);

                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(localFileName);
                await cloudBlockBlob.UploadTextAsync(fileContent);
                string previousMd5 = cloudBlockBlob.Properties.ContentMD5;
                string previuousCalculatedMd5 = serviceRegistrationStore.CalculateMd5Hash(Encoding.ASCII.GetBytes(fileContent));
                Console.WriteLine("storage property previous md5 value = {0}", previousMd5);
                Console.WriteLine("calculated md5 previous value = {0}", previuousCalculatedMd5);
                Assert.AreEqual(previuousCalculatedMd5, previousMd5);

                string changedFileContent = "test file content changed";               

                Console.WriteLine("Changed Temp file content : {0}", changedFileContent);
                Console.WriteLine("Uploading changedFile to Blob storage as blob '{0}'", localFileName);
                await cloudBlockBlob.UploadTextAsync(changedFileContent);

                string currentMd5 = cloudBlockBlob.Properties.ContentMD5;
                string currentCalculatedMd5 = serviceRegistrationStore.CalculateMd5Hash(Encoding.ASCII.GetBytes(changedFileContent));
                Assert.AreNotEqual(previousMd5, currentMd5);
                Assert.AreNotEqual(previuousCalculatedMd5, currentCalculatedMd5);
                Console.WriteLine("storage property current md5 value = {0}", currentMd5);
                Console.WriteLine("calculated md5 current value = {0}", currentCalculatedMd5);
                Assert.AreEqual(currentCalculatedMd5, currentMd5);
            }
        }

        [TestMethod]
        public async Task ExportToTempFileAsyncTest()
        {        
            await serviceRegistrationStore.SetAsync("testservice", new Version("1.0"), fileContent);
            string initMd5 = await serviceRegistrationStore.GetMd5Async("testservice", new Version("1.0"));
            string filePath = await serviceRegistrationStore.ExportToTempFileAsync("testservice", new Version("1.0"));
            string initCalculatedMd5 = serviceRegistrationStore.CalculateMd5Hash(File.ReadAllBytes(filePath));
            Console.WriteLine("Init file content : {0}", File.ReadAllText(filePath));
            Console.WriteLine("Init md5 in storage : {0}", initMd5);
            Console.WriteLine("Init calculated md5 : {0}", initCalculatedMd5);
            Assert.AreEqual(initMd5, initCalculatedMd5);

            const string changedFileContent = "test service version 1.0, content changed";
            await serviceRegistrationStore.SetAsync("testservice", new Version("1.0"), changedFileContent);
            filePath = await serviceRegistrationStore.ExportToTempFileAsync("testservice", new Version("1.0"));
            string currentFileContent = File.ReadAllText(filePath);
            Console.WriteLine("File path : {0}", filePath);
            Console.WriteLine("Changed file content : {0}", currentFileContent);         
            Assert.AreEqual(changedFileContent, currentFileContent);
        }
    }
}
