// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session.QueueAdapter.Module
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;

    /// <summary>
    /// Starting point for multi-queue implementation: Change back to non-static class and implement abstract factory pattern
    /// </summary>
    public static class CloudQueueCreationModule
    {
        public static SharedAccessQueuePolicy AddMessageSasPolicy =>
            new SharedAccessQueuePolicy() { Permissions = SharedAccessQueuePermissions.Add, SharedAccessExpiryTime = DateTime.UtcNow.AddDays(7) };

        public static SharedAccessQueuePolicy ProcessMessageSasPolicy =>
            new SharedAccessQueuePolicy() { Permissions = SharedAccessQueuePermissions.ProcessMessages, SharedAccessExpiryTime = DateTime.UtcNow.AddDays(7) };

        public static CloudQueue GetCloudQueueReference(string connectionString, string queueName)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentNullException(nameof(queueName));
            }

            var account = CloudStorageAccount.Parse(connectionString);

            return account.CreateCloudQueueClient().GetQueueReference(queueName);
        }

        public static CloudQueue GetCloudQueueReference(string sasUri)
        {
            if (string.IsNullOrEmpty(sasUri))
            {
                throw new ArgumentException("SAS URI of CloudQueue is null or empty.", nameof(sasUri));
            }

            return new CloudQueue(new Uri(sasUri));
        }

        public static async Task<CloudQueue> CreateCloudQueueIfNotExistsAsync(CloudQueue queue)
        {
            await queue.CreateIfNotExistsAsync();
            return queue;
        }

        public static string GetCloudQueueSas(CloudQueue queue, SharedAccessQueuePolicy queuePolicy)
        {
            return queue.Uri + queue.GetSharedAccessSignature(queuePolicy);
        }

        public static async Task<string> CreateCloudQueueAndGetSas(string connectionString, string queueName, SharedAccessQueuePolicy queuePolicy)
        {
            return GetCloudQueueSas(await CreateCloudQueueIfNotExistsAsync(GetCloudQueueReference(connectionString, queueName)), queuePolicy);
        }

        public static async Task ClearCloudQueueAsync(string connectionString, string queueName) => await GetCloudQueueReference(connectionString, queueName).ClearAsync();

        public static Task ClearCloudQueuesAsync(string connectionString, string[] queueNames) => Task.WhenAll(queueNames.Select(n => ClearCloudQueueAsync(connectionString, n)));
    }
}